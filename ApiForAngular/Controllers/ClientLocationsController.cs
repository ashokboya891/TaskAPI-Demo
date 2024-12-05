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
    public class ClientLocationsController : ControllerBase
    {
        private readonly TaskManagerDbContext _context;
        public ClientLocationsController(TaskManagerDbContext context) 
        {
            _context = context;
        }
        [HttpGet]
        [Route("api/clientlocations")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Get()
        {
            List<ClientLocations> clientLocations = _context.ClientLocations.ToList();
            return Ok(clientLocations);
        }

        [HttpGet]
        [Route("api/clientlocations/searchbyclientlocationid/{ClientLocationID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetByClientLocationID(int ClientLocationID)
        {
            ClientLocations clientLocation = _context.ClientLocations.Where(temp => temp.ClientLocationID == ClientLocationID).FirstOrDefault();
            if (clientLocation != null)
            {
                return Ok(clientLocation);
            }
            else
                return NoContent();
        }

        [HttpPost]
        [Route("api/clientlocations")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ClientLocations Post([FromBody] ClientLocations clientLocation)
        {
            _context.ClientLocations.Add(clientLocation);
            _context.SaveChanges();

            ClientLocations existingClientLocation = _context.ClientLocations.Where(temp => temp.ClientLocationID == clientLocation.ClientLocationID).FirstOrDefault();
            return clientLocation;
        }

        [HttpPut]
        [Route("api/clientlocations")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ClientLocations Put([FromBody] ClientLocations project)
        {
            ClientLocations existingClientLocation = _context.ClientLocations.Where(temp => temp.ClientLocationID == project.ClientLocationID).FirstOrDefault();
            if (existingClientLocation != null)
            {
                existingClientLocation.ClientLocationName = project.ClientLocationName;
                _context.SaveChanges();
                return existingClientLocation;
            }
            else
            {
                return null;
            }
        }

        [HttpDelete]
        [Route("api/clientlocations")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public int Delete(int ClientLocationID)
        {
            ClientLocations existingClientLocation = _context.ClientLocations.Where(temp => temp.ClientLocationID == ClientLocationID).FirstOrDefault();
            if (existingClientLocation != null)
            {
                _context.ClientLocations.Remove(existingClientLocation);
                _context.SaveChanges();
                return ClientLocationID;
            }
            else
            {
                return -1;
            }
        }

    }
}
