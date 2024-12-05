using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
namespace ApiForAngular.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RouterLoggerController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public RouterLoggerController(IWebHostEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        [Route("api/routerlogger")]
        public async Task<IActionResult> Index()
        {
            string logMessage = null;
            using (StreamReader streamReader = new StreamReader(Request.Body, Encoding.ASCII))
            {
                logMessage =  streamReader.ReadToEnd() + "\n";
            }
            string filePath = this._hostingEnvironment.ContentRootPath + "\\RouterLogger.txt";
            System.IO.File.AppendAllText(filePath, logMessage);
            return Ok();
        }
    }
}
