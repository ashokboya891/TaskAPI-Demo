using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiForAngular.ApplicationDbContext;
using ApiForAngular.Models;
using Microsoft.AspNetCore.Authorization;
using ApiForAngular.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ApiForAngular.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;

        public ProjectsController(TaskManagerDbContext context)
        {
            _context = context;
        }

        [HttpGet("Search")]
        public async Task<List<Project>>  Search(string searchBy, string searchText)
        {
           List<Project> result = null;
            if (searchBy == "ProjectID")
            {
               result=await _context.Projects.Where(t=>t.ProjectID.ToString().Contains(searchText)).ToListAsync();
            }
            else if(searchBy == "ProjectName")
            {
                result = await  _context.Projects.Where(t => t.ProjectName.ToString().Contains(searchText)).ToListAsync();

            }
            else if (searchBy == "DateOfStart")
            {
                result = await   _context.Projects.Where(t => t.DateOfStart.ToString().Contains(searchText)).ToListAsync();

            }
            else if (searchBy == "TeamSize")
            {
                result = await _context.Projects.Where(t => t.TeamSize.ToString().Contains(searchText)).ToListAsync();

            }
            else
            {
                return await _context.Projects.ToListAsync();
            }
            return result;
        }
        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            System.Threading.Thread.Sleep(1000);
            List<Project> projects = _context.Projects.Include("ClientLocation").ToList();

            List<ProjectViewModel> projectsViewModel = new List<ProjectViewModel>();
            foreach (var project in projects)
            {
                projectsViewModel.Add(new ProjectViewModel() { ProjectID = project.ProjectID, ProjectName = project.ProjectName, TeamSize = project.TeamSize, DateOfStart = project.DateOfStart.ToString("dd/MM/yyyy"), Active = project.Active, ClientLocation = project.ClientLocation, ClientLocationID = project.ClientLocationID, Status = project.Status });
            }
            return Ok(projectsViewModel);
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }
        [HttpGet]
        [Route("api/projects/searchbyprojectid/{ProjectID}")]
        public IActionResult GetProjectByProject(int ProjectID)
        {
                var project = _context.Projects
                    .Include("ClientLocation")
                    .FirstOrDefault(temp => temp.ProjectID == ProjectID);

                if (project == null)
                {
                    return NotFound(); // Return 404 if the project doesn't exist
                }

                var projectViewModel = new ProjectViewModel()
                {
                    ProjectID = project.ProjectID,
                    ProjectName = project.ProjectName,
                    TeamSize = project.TeamSize,
                    DateOfStart = project.DateOfStart.ToString("dd/MM/yyyy"),
                    Active = project.Active,
                    ClientLocation = project.ClientLocation,
                    ClientLocationID = project.ClientLocationID,
                    Status = project.Status
                };

                return Ok(projectViewModel);
            }

            //Project project = _context.Projects.Include("ClientLocation").Where(temp => temp.ProjectID == ProjectID).FirstOrDefault();

            //ProjectViewModel projectViewModel = new ProjectViewModel() { ProjectID = project.ProjectID, ProjectName = project.ProjectName, TeamSize = project.TeamSize, DateOfStart = project.DateOfStart.ToString("dd/MM/yyyy"), Active = project.Active, ClientLocation = project.ClientLocation, ClientLocationID = project.ClientLocationID, Status = project.Status };
            //return Ok(projectViewModel);
        

        // PUT: api/Projects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
          
                if (id != project.ProjectID)
                {
                    return BadRequest();
                }

                _context.Entry(project).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Retrieve the updated project
                var updatedProject = await _context.Projects.FindAsync(id);
                if (updatedProject == null)
                {
                    return NotFound();
                }

                return Ok(updatedProject); // Return the updated project
            }

            //    if (id != project.ProjectID)
            //    {
            //        return BadRequest();
            //    }

            //    _context.Entry(project).State = EntityState.Modified;

            //    try
            //    {
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!ProjectExists(id))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }

            //    return NoContent();
            //}

            // POST: api/Projects
            // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
            [HttpPost]
            public async Task<ActionResult<Project>> PostProject(Project project)
            {
            project.ClientLocation = null;
            _context.Projects.Add(project);
            _context.SaveChanges();

            Project existingProject = _context.Projects.Include("ClientLocation").Where(temp => temp.ProjectID == project.ProjectID).FirstOrDefault();
            ProjectViewModel projectViewModel = new ProjectViewModel() { ProjectID = existingProject.ProjectID, ProjectName = existingProject.ProjectName, TeamSize = existingProject.TeamSize, DateOfStart = existingProject.DateOfStart.ToString("dd/MM/yyyy"), Active = existingProject.Active, ClientLocation = existingProject.ClientLocation, ClientLocationID = existingProject.ClientLocationID, Status = existingProject.Status };

            return Ok(projectViewModel);

            //return CreatedAtAction("GetProject", new { id = project.ProjectID }, project);
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectID == id);
        }
    }
}
