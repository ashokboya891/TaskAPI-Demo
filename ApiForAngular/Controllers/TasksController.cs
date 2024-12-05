using ApiForAngular.ApplicationDbContext;
using ApiForAngular.Models;
using Humanizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;
using ApiForAngular.DTO;
using System.Threading.Tasks;

namespace ApiForAngular.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagerDbContext db;
        private readonly UserManager<ApplicationUser> applicationUserManager;
        private ILogger<TasksController> _logger;

        public TasksController(TaskManagerDbContext db, UserManager<ApplicationUser> applicationUserManager, ILogger<TasksController> logger)
        {
            this.db = db;
            this.applicationUserManager = applicationUserManager;
            this._logger = logger;
        }
        [HttpGet]
        [Route("/api/tasks")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Get()
        {
            // Extract the current user ID from the authenticated user context
            string currentUserId = User.Identity.Name;
            // Initialize the response list for grouped tasks
            List<GroupedTask> groupedTasks = new List<GroupedTask>();
            // Fetch task statuses from the database
            List<TaskStatuses> taskStatuses = db.TaskStatusesTbl.ToList();
            // Fetch tasks created by or assigned to the current user
            List<Tasks> tasks = db.Tasks
                .Include(task => task.CreatedByUser)
                .Include(task => task.AssignedToUser)
                .Include(task => task.Project).ThenInclude(project => project.ClientLocation)
                .Include(task => task.TaskStatusDetails)
                .Include(task => task.TaskPriority)
                .Where(task => task.CreatedBy.ToString() == currentUserId || task.AssignedTo.ToString() == currentUserId)
                .OrderBy(task => task.TaskPriorityID)
                .ThenByDescending(task => task.LastUpdatedOn)
                .ToList();
            // Format task data for response
            foreach (var task in tasks)
            {
                // Convert datetime properties to formatted strings
                task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
                task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");
                // Sort task status details by descending order of ID
                task.TaskStatusDetails = task.TaskStatusDetails.OrderByDescending(detail => detail.TaskStatusDetailID).ToList();
                // Convert status update datetime to formatted string
                foreach (var detail in task.TaskStatusDetails)
                {
                    detail.StatusUpdationDateTimeString = detail.StatusUpdationDateTime.ToString("dd/MM/yyyy");
                }
            }
            // Group tasks by their current status
            foreach (var status in taskStatuses)
            {
                GroupedTask groupedTask = new GroupedTask
                {
                    TaskStatusName = status.TaskStatusName,
                    Tasks = tasks.Where(task => task.CurrentStatus == status.TaskStatusName).ToList()
                };
                // Add to grouped tasks if there are tasks under this status
                if (groupedTask.Tasks.Count > 0)
                {
                    groupedTasks.Add(groupedTask);
                }
            }
            // Return grouped tasks as response
            return Ok(groupedTasks);
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("/api/createtask")]
        public IActionResult Post([FromBody] Tasks task)
        {
            // Assign default values for nullable fields
            task.CreatedOn = DateTime.Now;
            task.LastUpdatedOn = DateTime.Now;
            task.CurrentTaskStatusID = 1;
            task.CurrentStatus ??= "Holding"; // Default to "Holding" if null
            task.CreatedOnString ??= task.CreatedOn.ToString("dd/MM/yyyy");
            task.LastUpdatedOnString ??= task.LastUpdatedOn.ToString("dd/MM/yyyy");
            db.Tasks.Add(task);
            db.SaveChanges();
            // Create TaskStatusDetail
            TaskStatusDetail taskStatusDetail = new TaskStatusDetail
            {
                TaskID = task.TaskID,
                UserID = task.CreatedBy,
                TaskStatusID = 1,
                StatusUpdationDateTime = DateTime.Now,
                Description = "Task Created"
            };
            db.TaskStatusDetails.Add(taskStatusDetail);
            db.SaveChanges();
            Tasks existingTask = db.Tasks.FirstOrDefault(temp => temp.TaskID == task.TaskID);
            return Ok(existingTask);
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //// Map the DTO to the Tasks entity
            //var task = new Tasks
            //{
            //    TaskName = taskDto.TaskName,
            //    Description = taskDto.Description,
            //    ProjectID = taskDto.ProjectID,
            //    CreatedBy = taskDto.CreatedBy,
            //    AssignedTo = taskDto.AssignedTo,
            //    TaskPriorityID = taskDto.TaskPriorityID,
            //    CreatedOn = DateTime.Now,
            //    LastUpdatedOn = DateTime.Now,
            //    CurrentStatus = "Holding", // Default value
            //    CurrentTaskStatusID = 1,   // Default value
            //    CreatedOnString = DateTime.Now.ToString("dd/MM/yyyy"), // Computed value
            //    LastUpdatedOnString = DateTime.Now.ToString("dd/MM/yyyy") // Computed value
            //};

            //db.Tasks.Add(task);
            //db.SaveChanges();

            //var taskStatusDetail = new TaskStatusDetail
            //{
            //    TaskID = task.TaskID,
            //    UserID = task.CreatedBy,
            //    TaskStatusID = 1,
            //    StatusUpdationDateTime = DateTime.Now,
            //    Description = "Task Created"
            //};
            //db.TaskStatusDetails.Add(taskStatusDetail);
            //db.SaveChanges();

            //return Ok(new { Message = "Task created successfully", TaskID = task.TaskID });
        }


        //[HttpPost]
        //    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //    [Route("/api/createtask")]
        //    public IActionResult Post([FromBody] Tasks task)
        //    {
        //        // Set default or computed properties
        //        task.CreatedOn = DateTime.Now;
        //        task.LastUpdatedOn = DateTime.Now;
        //        task.CurrentStatus = "Holding";
        //        task.CurrentTaskStatusID = 1;
        //        task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
        //        task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");

        //        // Fetch the logged-in user's ID
        //        var user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
        //        if (user == null)
        //        {
        //            return BadRequest("User not found.");
        //        }
        //        task.CreatedBy = user.Id;

        //        // Set navigation properties to null (if not provided in payload)
        //        task.Project = null;
        //        task.CreatedByUser = null;
        //        task.AssignedToUser = null;
        //        task.TaskPriority = null;
        //        task.TaskStatusDetails = null;

        //        // Add task to the database
        //        db.Tasks.Add(task);
        //        db.SaveChanges();

        //        // Add a corresponding TaskStatusDetail
        //        TaskStatusDetail taskStatusDetail = new TaskStatusDetail
        //        {
        //            TaskID = task.TaskID,
        //            UserID = task.CreatedBy,
        //            TaskStatusID = 1,
        //            StatusUpdationDateTime = DateTime.Now,
        //            Description = "Task Created",
        //            TaskStatus = null,
        //            User = null
        //        };
        //        db.TaskStatusDetails.Add(taskStatusDetail);
        //        db.SaveChanges();

        //        return Ok(task);
        //    }
        //}
    }
}

        //    [HttpPost]
        //    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //    [Route("/api/createtask")]
        //    public IActionResult Post([FromBody] Tasks task)
        //    {
        //        task.Project = null;
        //        task.CreatedByUser = null;
        //        task.AssignedToUser = null;
        //        task.TaskPriority = null;
        //        task.TaskStatusDetails = null;
        //        task.CreatedOn = DateTime.Now;
        //        task.LastUpdatedOn = DateTime.Now;
        //        task.CurrentStatus = "Holding";
        //        task.CurrentTaskStatusID = 1;
        //        task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
        //        task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");
        //        // Assuming the User.Identity.Name represents the username, and you need the corresponding Guid.
        //        var user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
        //        if (user != null)
        //        {
        //            task.CreatedBy = user.Id; // Assign the user's Guid to CreatedBy
        //        }



//        db.Tasks.Add(task);
//        db.SaveChanges();

//        TaskStatusDetail taskStatusDetail = new TaskStatusDetail();
//        taskStatusDetail.TaskID = task.TaskID;
//        taskStatusDetail.UserID = task.CreatedBy;
//        taskStatusDetail.TaskStatusID = 1;
//        taskStatusDetail.StatusUpdationDateTime = DateTime.Now;
//        taskStatusDetail.TaskStatus = null;
//        taskStatusDetail.User = null;
//        taskStatusDetail.Description = "Task Created";
//        db.TaskStatusDetails.Add(taskStatusDetail);
//        db.SaveChanges();

//        Tasks existingTask = db.Tasks.Where(temp => temp.TaskID == task.TaskID).FirstOrDefault();
//        return Ok(existingTask);
//    }
//}
//}


//[HttpGet]
//[Route("/api/tasks")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//public IActionResult Get()
//{
//    try
//    {
//        string currentUserId = User.Identity.Name;
//        if (string.IsNullOrEmpty(currentUserId))
//        {
//            return Unauthorized("User is not authenticated.");
//        }

//        // Fetch task statuses from the database
//        List<TaskStatuses> taskStatuses = db.TaskStatusesTbl.ToList();

//        // Fetch tasks created by or assigned to the current user
//        List<Tasks> tasks = db.Tasks
//            .Include(task => task.CreatedByUser)
//            .Include(task => task.AssignedToUser)
//            .Include(task => task.Project).ThenInclude(project => project.ClientLocation)
//            .Include(task => task.TaskPriority)
//            .Include(task => task.TaskStatusDetails)
//            .Where(task => task.CreatedBy == new Guid(currentUserId) || task.AssignedTo == new Guid(currentUserId))
//            .OrderBy(task => task.TaskPriorityID)
//            .ThenByDescending(task => task.LastUpdatedOn)
//            .ToList();

//        // Format task data for response
//        foreach (var task in tasks)
//        {
//            task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
//            task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");
//            task.TaskStatusDetails = task.TaskStatusDetails.OrderByDescending(detail => detail.TaskStatusDetailID).ToList();
//            foreach (var detail in task.TaskStatusDetails)
//            {
//                detail.StatusUpdationDateTimeString = detail.StatusUpdationDateTime.ToString("dd/MM/yyyy");
//            }
//        }

//        // Group tasks by their current status
//        List<GroupedTask> groupedTasks = new List<GroupedTask>();
//        foreach (var status in taskStatuses)
//        {
//            var groupedTask = new GroupedTask
//            {
//                TaskStatusName = status.TaskStatusName,
//                Tasks = tasks
//                    .Where(task => task.CurrentStatus == status.TaskStatusName)
//                    .GroupBy(task => task.TaskID)  // Group by TaskID to avoid duplicates
//                    .Select(group => group.First())  // Select the first task from each group
//                    .ToList()
//            };

//            // Add to grouped tasks if there are tasks under this status
//     //Add to grouped tasks if there are tasks under this status
//        if (groupedTask.Tasks.Count > 0)
//            {
//                groupedTasks.Add(groupedTask);
//            }
//        }

//        return Ok(groupedTasks);
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, "Internal server error.");
//    }
//}



//[HttpGet]
//[Route("/api/tasks")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//public IActionResult Get()
//{
//    try
//    {
//        string currentUserId = User.Identity.Name;
//        if (string.IsNullOrEmpty(currentUserId))
//        {
//            return Unauthorized("User is not authenticated.");
//        }

//        _logger.LogInformation($"Current User ID: {currentUserId}");

//        // Fetch task statuses from the database
//        List<TaskStatuses> taskStatuses = db.TaskStatusesTbl.ToList();
//        _logger.LogInformation($"Fetched {taskStatuses.Count} task statuses.");

//        // Fetch tasks created by or assigned to the current user
//        List<Tasks> tasks = db.Tasks
//            .Include(task => task.CreatedByUser)
//            .Include(task => task.AssignedToUser)
//            .Include(task => task.Project).ThenInclude(project => project.ClientLocation)
//            .Include(task => task.TaskPriority)
//            .Include(task => task.TaskStatusDetails)
//            .Where(task => task.CreatedBy.ToString() == currentUserId || task.AssignedTo.ToString() == currentUserId)
//            .OrderBy(task => task.TaskPriorityID)
//            .ThenByDescending(task => task.LastUpdatedOn)
//            .ToList();

//        _logger.LogInformation($"Fetched {tasks.Count} tasks.");

//        if (!tasks.Any())
//        {
//            return NotFound("No tasks found for the current user.");
//        }

//        // Format task data for response
//        foreach (var task in tasks)
//        {
//            task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
//            task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");
//            task.TaskStatusDetails = task.TaskStatusDetails.OrderByDescending(detail => detail.TaskStatusDetailID).ToList();
//            foreach (var detail in task.TaskStatusDetails)
//            {
//                detail.StatusUpdationDateTimeString = detail.StatusUpdationDateTime.ToString("dd/MM/yyyy");
//            }
//        }

//        // Group tasks by their current status
//        List<GroupedTask> groupedTasks = new List<GroupedTask>();
//        foreach (var status in taskStatuses)
//        {
//            var groupedTask = new GroupedTask
//            {
//                TaskStatusName = status.TaskStatusName,
//                Tasks = tasks.Where(task => task.CurrentStatus == status.TaskStatusName).ToList()
//            };

//            if (groupedTask.Tasks.Any())
//            {
//                groupedTasks.Add(groupedTask);
//            }
//        }

//        return Ok(groupedTasks);
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error occurred while fetching tasks.");
//        return StatusCode(500, "Internal server error.");
//    }
//}
//[HttpGet]
//[Route("/api/tasks")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//public IActionResult Get()
//{
//    // Extract the current user ID from the authenticated user context
//    string currentUserId = User.Identity.Name;

//    // Initialize the response list for grouped tasks
//    List<GroupedTask> groupedTasks = new List<GroupedTask>();

//    // Fetch task statuses from the database
//    List<TaskStatuses> taskStatuses = db.TaskStatusesTbl.ToList();

//    // Fetch tasks created by or assigned to the current user
//    List<Tasks> tasks = db.Tasks
//        .Include(task => task.CreatedByUser)
//        .Include(task => task.AssignedToUser)
//        .Include(task => task.Project).ThenInclude(project => project.ClientLocation)
//        .Include(task => task.TaskPriority)
//        .Include(task => task.TaskStatusDetails)
//        .Where(task => task.CreatedBy.ToString() == currentUserId || task.AssignedTo.ToString() == currentUserId)
//        .OrderBy(task => task.TaskPriorityID)
//        .ThenByDescending(task => task.LastUpdatedOn)
//        .ToList();

//    // Format task data for response
//    foreach (var task in tasks)
//    {
//        // Convert datetime properties to formatted strings
//        task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
//        task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");

//        // Sort task status details by descending order of ID
//        task.TaskStatusDetails = task.TaskStatusDetails.OrderByDescending(detail => detail.TaskStatusDetailID).ToList();

//        // Convert status update datetime to formatted string
//        foreach (var detail in task.TaskStatusDetails)
//        {
//            detail.StatusUpdationDateTimeString = detail.StatusUpdationDateTime.ToString("dd/MM/yyyy");
//        }
//    }

//    // Group tasks by their current status
//    foreach (var status in taskStatuses)
//    {
//        GroupedTask groupedTask = new GroupedTask
//        {
//            TaskStatusName = status.TaskStatusName,
//            Tasks = tasks.Where(task => task.CurrentStatus == status.TaskStatusName).ToList()
//        };

//        // Add to grouped tasks if there are tasks under this status
//        if (groupedTask.Tasks.Count > 0)
//        {
//            groupedTasks.Add(groupedTask);
//        }
//    }

//    // Configure JSON serialization to handle circular references
//    var options = new JsonSerializerOptions
//    {
//        ReferenceHandler = ReferenceHandler.Preserve,
//        WriteIndented = true // Optional: Makes the JSON output more readable
//    };

//    // Serialize the grouped tasks with the configured options
//    string jsonResponse = JsonSerializer.Serialize(groupedTasks, options);

//    // Return the serialized JSON response
//    return Ok(jsonResponse);
//}
//***
//[HttpGet]
//[Route("/api/tasks")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//public IActionResult Get()
//{
//    // Extract the current user ID from the authenticated user context
//    string currentUserId = User.Identity.Name;

//    // Initialize the response list for grouped tasks
//    List<GroupedTask> groupedTasks = new List<GroupedTask>();

//    // Fetch task statuses from the database
//    List<TaskStatuses> taskStatuses = db.TaskStatusesTbl.ToList();

//    string currentUserId1 = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//    // Fetch tasks created by or assigned to the current user
//    List<Tasks> tasks = db.Tasks
//       .Include(task => task.CreatedByUser)
//       .Include(task => task.AssignedToUser)
//       .Include(task => task.Project).ThenInclude(project => project.ClientLocation)
//       .Include(task => task.TaskPriority)
//       .Include(task => task.TaskStatusDetails)
//    .Where(task => task.CreatedBy.ToString() == currentUserId || task.AssignedTo.ToString() == currentUserId)
//    .OrderBy(task => task.TaskPriorityID)
//    .ThenByDescending(task => task.LastUpdatedOn)
//    .ToList();

//    // Format task data for response
//    foreach (var task in tasks)
//    {
//        // Convert datetime properties to formatted strings
//        task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
//        task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");

//        // Sort task status details by descending order of ID
//        task.TaskStatusDetails = task.TaskStatusDetails.OrderByDescending(detail => detail.TaskStatusDetailID).ToList();

//        // Convert status update datetime to formatted string
//        foreach (var detail in task.TaskStatusDetails)
//        {
//            detail.StatusUpdationDateTimeString = detail.StatusUpdationDateTime.ToString("dd/MM/yyyy");
//        }
//    }

//    // Group tasks by their current status
//    foreach (var status in taskStatuses)
//    {
//        GroupedTask groupedTask = new GroupedTask
//        {
//            TaskStatusName = status.TaskStatusName,
//            Tasks = tasks.Where(task => task.CurrentStatus == status.TaskStatusName)
//                          .Select(task => new TaskDto
//                          {
//                              TaskID = task.TaskID,
//                              TaskName = task.TaskName,
//                              CreatedOnString = task.CreatedOnString,
//                              LastUpdatedOnString = task.LastUpdatedOnString,
//                              TaskStatusDetails = task.TaskStatusDetails.Select(detail => new TaskStatusDetailDto
//                              {
//                                  TaskStatusDetailID = detail.TaskStatusDetailID,
//                                  Description = detail.Description,
//                                  StatusUpdationDateTimeString = detail.StatusUpdationDateTimeString,
//                                  TaskStatusName = detail.TaskStatus.TaskStatusName
//                              }).ToList()
//                          })
//                          .ToList()
//        };

//        // Add to grouped tasks if there are tasks under this status
//        if (groupedTask.Tasks.Count > 0)
//        {
//            groupedTasks.Add(groupedTask);
//        }
//    }

//    // Return grouped tasks as response
//    return Ok(groupedTasks);
//}

//public class GroupedTask
//{
//    public string TaskStatusName { get; set; }
//    public List<TaskDto> Tasks { get; set; }
//}


//[HttpGet]
//[Route("/api/tasks")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//public IActionResult Get()
//{
//    // Extract the current user ID from the authenticated user context
//    string currentUserId = User.Identity.Name;

//    // Initialize the response list for grouped tasks
//    List<GroupedTask> groupedTasks = new List<GroupedTask>();

//    // Fetch task statuses from the database
//    List<TaskStatuses> taskStatuses = db.TaskStatusesTbl.ToList();

//    string currentUserId1 = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//    // Fetch tasks created by or assigned to the current user
//    List<Tasks> tasks = db.Tasks
//     .Include(task => task.CreatedByUser)
//     .Include(task => task.AssignedToUser)
//     .Include(task => task.Project).ThenInclude(project => project.ClientLocation)
//     .Include(task => task.TaskPriority)
//     .Include(task => task.TaskStatusDetails)
//    .Where(task => task.CreatedBy.ToString() == currentUserId || task.AssignedTo.ToString() == currentUserId)
//     .OrderBy(task => task.TaskPriorityID)
//     .ThenByDescending(task => task.LastUpdatedOn)
//    .ToList();

//    //List<Tasks> tasks = db.Tasks
//    //    .Include(task => task.CreatedByUser)
//    //    .Include(task => task.AssignedToUser)
//    //    .Include(task => task.Project).ThenInclude(project => project.ClientLocation)
//    //    .Include(task => task.TaskStatusDetails)
//    //    .Include(task => task.TaskPriority)
//    //    .Where(task => task.CreatedBy.ToString() == currentUserId || task.AssignedTo.ToString() == currentUserId)
//    //    .OrderBy(task => task.TaskPriorityID)
//    //    .ThenByDescending(task => task.LastUpdatedOn)
//    //    .ToList();

//    // Format task data for response
//    foreach (var task in tasks)
//    {
//        // Convert datetime properties to formatted strings
//        task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
//        task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");

//        // Sort task status details by descending order of ID
//        task.TaskStatusDetails = task.TaskStatusDetails.OrderByDescending(detail => detail.TaskStatusDetailID).ToList();

//        // Convert status update datetime to formatted string
//        foreach (var detail in task.TaskStatusDetails)
//        {
//            detail.StatusUpdationDateTimeString = detail.StatusUpdationDateTime.ToString("dd/MM/yyyy");
//        }
//    }

//    // Group tasks by their current status
//    foreach (var status in taskStatuses)
//    {
//        GroupedTask groupedTask = new GroupedTask
//        {
//            TaskStatusName = status.TaskStatusName,
//            Tasks = tasks.Where(task => task.CurrentStatus == status.TaskStatusName).ToList()
//        };

//        // Add to grouped tasks if there are tasks under this status
//        if (groupedTask.Tasks.Count > 0)
//        {
//            groupedTasks.Add(groupedTask);
//        }
//    }

//    // Return grouped tasks as response
//    return Ok(groupedTasks);
//}


//[HttpPost]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//[Route("/api/createtask")]
//public IActionResult Post([FromBody] Tasks task)
//{
//    // Assign default objects if fields are null
//    if (task.Project == null)
//    {
//        task.Project = new Project { ProjectName = "Default Project" };  // Example: replace with valid Project object
//    }

//    if (task.TaskPriority == null)
//    {
//        task.TaskPriority = new TaskPriority { TaskPriorityName = "Default Priority" };  // Example: replace with valid TaskPriority object
//    }

//    if (task.CreatedByUser == null)
//    {
//        task.CreatedByUser = new ApplicationUser { UserName = "Default User" };  // Example: replace with valid User object
//    }

//    // Fill in additional default values as necessary
//    task.CreatedOn = DateTime.Now;
//    task.LastUpdatedOn = DateTime.Now;
//    task.CurrentStatus = "Holding";
//    task.CurrentTaskStatusID = 1;
//    task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
//    task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");
//    task.CreatedBy = Guid.Parse(User.Identity.Name);

//    db.Tasks.Add(task);
//    db.SaveChanges();

//    // Create TaskStatusDetail
//    TaskStatusDetail taskStatusDetail = new TaskStatusDetail
//    {
//        TaskID = task.TaskID,
//        UserID = task.CreatedBy,
//        TaskStatusID = 1,
//        StatusUpdationDateTime = DateTime.Now,
//        TaskStatus = null,
//        User = null,
//        Description = "Task Created"
//    };
//    db.TaskStatusDetails.Add(taskStatusDetail);
//    db.SaveChanges();

//    Tasks existingTask = db.Tasks.Where(temp => temp.TaskID == task.TaskID).FirstOrDefault();
//    return Ok(existingTask);
//}
//#######
//[HttpPost]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//[Route("/api/createtask")]
//public IActionResult Post([FromBody] Tasks task)
//{
//    // Assign default values for nullable fields
//    task.CreatedOn = DateTime.Now;
//    task.LastUpdatedOn = DateTime.Now;
//    task.CurrentStatus ??= "Holding"; // Default to "Holding" if null
//    task.CreatedOnString ??= task.CreatedOn.ToString("dd/MM/yyyy");
//    task.LastUpdatedOnString ??= task.LastUpdatedOn.ToString("dd/MM/yyyy");

//    db.Tasks.Add(task);
//    db.SaveChanges();

//    // Create TaskStatusDetail
//    TaskStatusDetail taskStatusDetail = new TaskStatusDetail
//    {
//        TaskID = task.TaskID,
//        UserID =task.CreatedBy,
//        TaskStatusID = 1,
//        StatusUpdationDateTime = DateTime.Now,
//        Description = "Task Created"
//    };
//    db.TaskStatusDetails.Add(taskStatusDetail);
//    db.SaveChanges();

//    Tasks existingTask = db.Tasks.FirstOrDefault(temp => temp.TaskID == task.TaskID);
//    return Ok(existingTask);
//}

//[HttpPost]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//[Route("/api/createtask")]
//public IActionResult Post([FromBody] Tasks task)
//{
//    // Assign default objects if navigation properties are required
//    if (task.Project == null)
//    {
//        task.Project = db.Projects.Find(task.ProjectID); // Fetch from DB using ProjectID
//    }

//    if (task.TaskPriority == null)
//    {
//        task.TaskPriority = db.TaskPriorities.Find(task.TaskPriorityID); // Fetch from DB using TaskPriorityID
//    }

//    if (task.CreatedByUser == null)
//    {
//        task.CreatedByUser = db.Users.Find(task.CreatedBy); // Fetch from DB using CreatedBy
//    }

//    if (task.AssignedToUser == null)
//    {
//        task.AssignedToUser = db.Users.Find(task.AssignedTo); // Fetch from DB using AssignedTo
//    }

//    task.CreatedOn = DateTime.Now;
//    task.LastUpdatedOn = DateTime.Now;
//    task.CurrentStatus = "Holding";
//    task.CurrentTaskStatusID = 1;
//    task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
//    task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");

//    db.Tasks.Add(task);
//    db.SaveChanges();

//    // Create TaskStatusDetail
//    TaskStatusDetail taskStatusDetail = new TaskStatusDetail
//    {
//        TaskID = task.TaskID,
//        UserID = task.CreatedBy,
//        TaskStatusID = 1,
//        StatusUpdationDateTime = DateTime.Now,
//        TaskStatus = null,
//        User = null,
//        Description = "Task Created"
//    };
//    db.TaskStatusDetails.Add(taskStatusDetail);
//    db.SaveChanges();

//    Tasks existingTask = db.Tasks.Where(temp => temp.TaskID == task.TaskID).FirstOrDefault();
//    return Ok(existingTask);
//}

//[HttpPost]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//[Route("/api/createtask")]
//public IActionResult Post([FromBody] Tasks task)
//{
//    // Provide default values or handle null cases
//    if (task.Project == null) task.Project = "Default Project";  // Or use an actual Project object
//    if (task.TaskPriority == null) task.TaskPriority = "Default Priority";  // Or use an actual TaskPriority object
//    if (task.CreatedByUser == null) task.CreatedByUser = "Default User";  // Or use a user object

//    task.CreatedOn = DateTime.Now;
//    task.LastUpdatedOn = DateTime.Now;
//    task.CurrentStatus = "Holding";
//    task.CurrentTaskStatusID = 1;
//    task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
//    task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");
//    task.CreatedBy = Guid.Parse(User.Identity.Name);

//    db.Tasks.Add(task);
//    db.SaveChanges();

//    // Create TaskStatusDetail
//    TaskStatusDetail taskStatusDetail = new TaskStatusDetail
//    {
//        TaskID = task.TaskID,
//        UserID = task.CreatedBy,
//        TaskStatusID = 1,
//        StatusUpdationDateTime = DateTime.Now,
//        TaskStatus = null,
//        User = null,
//        Description = "Task Created"
//    };
//    db.TaskStatusDetails.Add(taskStatusDetail);
//    db.SaveChanges();

//    Tasks existingTask = db.Tasks.Where(temp => temp.TaskID == task.TaskID).FirstOrDefault();
//    return Ok(existingTask);
//}

//[HttpPost]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//[Route("/api/createtask")]
//public IActionResult Post([FromBody] Tasks task)
//{
//    task.Project = null;
//    task.CreatedByUser = null;
//    task.AssignedToUser = null;
//    task.TaskPriority = null;
//    task.TaskStatusDetails = null;
//    task.CreatedOn = DateTime.Now;
//    task.LastUpdatedOn = DateTime.Now;
//    task.CurrentStatus = "Holding";
//    task.CurrentTaskStatusID = 1;
//    task.CreatedOnString = task.CreatedOn.ToString("dd/MM/yyyy");
//    task.LastUpdatedOnString = task.LastUpdatedOn.ToString("dd/MM/yyyy");
//    task.CreatedBy =Guid.Parse( User.Identity.Name);

//    db.Tasks.Add(task);
//    db.SaveChanges();

//    TaskStatusDetail taskStatusDetail = new TaskStatusDetail();
//    taskStatusDetail.TaskID = task.TaskID;
//    taskStatusDetail.UserID = task.CreatedBy;
//    taskStatusDetail.TaskStatusID = 1;
//    taskStatusDetail.StatusUpdationDateTime = DateTime.Now;
//    taskStatusDetail.TaskStatus = null;
//    taskStatusDetail.User = null;
//    taskStatusDetail.Description = "Task Created";
//    db.TaskStatusDetails.Add(taskStatusDetail);
//    db.SaveChanges();

//    Tasks existingTask = db.Tasks.Where(temp => temp.TaskID == task.TaskID).FirstOrDefault();
//    return Ok(existingTask);
//}
//    }
//}
