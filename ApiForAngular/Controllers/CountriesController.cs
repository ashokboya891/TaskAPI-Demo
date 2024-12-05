using ApiForAngular.ApplicationDbContext;
using ApiForAngular.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiForAngular.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly TaskManagerDbContext _db;

        public CountriesController(TaskManagerDbContext db)
        {
            this._db = db;
        }
        [HttpGet]
        [Route("api/countries")]
        public IActionResult GetCountries()
        {
            List<Country> countries = this._db.Countries.OrderBy(temp => temp.CountryName).ToList();
            return Ok(countries);
        }
        [HttpGet]
        [Route("api/countries/searchbycountryid/{CountryID}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetByCountryID(int CountryID)
        {
            Country country = _db.Countries.Where(temp => temp.CountryID == CountryID).FirstOrDefault();
            if (country != null)
            {
                return Ok(country);
            }
            else
                return NoContent();
        }

        [HttpPost]
        [Route("api/countries")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public Country Post([FromBody] Country country)
        {
            _db.Countries.Add(country);
            _db.SaveChanges();

            Country existingCountry = _db.Countries.Where(temp => temp.CountryID == country.CountryID).FirstOrDefault();
            return existingCountry;
        }

        [HttpPut]
        [Route("api/countries")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public Country Put([FromBody] Country country)
        {
            Country existingCountry = _db.Countries.Where(temp => temp.CountryID == country.CountryID).FirstOrDefault();
            if (existingCountry != null)
            {
                existingCountry.CountryName = country.CountryName;
                _db.SaveChanges();
                return existingCountry;
            }
            else
            {
                return null;
            }
        }

        [HttpDelete]
        [Route("api/countries")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public int Delete(int CountryID)
        {
            Country existingCountry = _db.Countries.Where(temp => temp.CountryID == CountryID).FirstOrDefault();
            if (existingCountry != null)
            {
                _db.Countries.Remove(existingCountry);
                _db.SaveChanges();
                return CountryID;
            }
            else
            {
                return -1;
            }
        }    
    }
}
