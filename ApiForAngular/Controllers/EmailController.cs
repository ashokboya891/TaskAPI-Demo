﻿using ApiForAngular.DTO;
using ApiForAngular.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiForAngular.Controllers
{
    //[AllowAnonymous]
    //[Route("api/[controller]")]
    //[ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        //[HttpPost]
        //public IActionResult SendEmail(EmailDto request)
        //{
        //    _emailService.SendEmail(request);
        //    return Ok();
        //}
    }
}
