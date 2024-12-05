using ApiForAngular.ApplicationDbContext;
using ApiForAngular.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiForAngular.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class TaskStatusesController : ControllerBase
    {
        private readonly TaskManagerDbContext db;

        public TaskStatusesController(TaskManagerDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        [Route("api/taskstatuses")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public List<TaskStatuses> Get()
        {
            List<TaskStatuses> taskStatuses = db.TaskStatusesTbl.ToList();
            return taskStatuses;
        }

        [HttpGet]
        [Route("api/taskstatuses/searchbytaskstatusid/{TaskStatusID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetByTaskStatusID(int TaskStatusID)
        {
            TaskStatuses taskStatus = db.TaskStatusesTbl.Where(temp => temp.TaskStatusID == TaskStatusID).FirstOrDefault();
            if (taskStatus != null)
            {
                return Ok(taskStatus);
            }
            else
                return NoContent();
        }

        [HttpPost]
        [Route("api/taskstatuses")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public TaskStatuses Post([FromBody] TaskStatuses taskStatus)
        {
            db.TaskStatusesTbl.Add(taskStatus);
            db.SaveChanges();

            TaskStatuses existingTaskStatus = db.TaskStatusesTbl.Where(temp => temp.TaskStatusID == taskStatus.TaskStatusID).FirstOrDefault();
            return taskStatus;
        }

        [HttpPut]
        [Route("api/taskstatuses")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public TaskStatuses Put([FromBody] TaskStatuses project)
        {
            TaskStatuses existingTaskStatus = db.TaskStatusesTbl.Where(temp => temp.TaskStatusID == project.TaskStatusID).FirstOrDefault();
            if (existingTaskStatus != null)
            {
                existingTaskStatus.TaskStatusName = project.TaskStatusName;
                db.SaveChanges();
                return existingTaskStatus;
            }
            else
            {
                return null;
            }
        }

        [HttpDelete]
        [Route("api/taskstatuses")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public int Delete(int TaskStatusID)
        {
            TaskStatuses existingTaskStatus = db.TaskStatusesTbl.Where(temp => temp.TaskStatusID == TaskStatusID).FirstOrDefault();
            if (existingTaskStatus != null)
            {
                db.TaskStatusesTbl.Remove(existingTaskStatus);
                db.SaveChanges();
                return TaskStatusID;
            }
            else
            {
                return -1;
            }
        }
    }
}
