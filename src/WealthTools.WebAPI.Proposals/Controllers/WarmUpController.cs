using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace WealthTools.WebAPI.Proposals.Controllers
{
    [ApiVersion("1.0")]
    public class WarmUpController : Controller
    {
        [HttpGet("/")]
        [MapToApiVersion("1.0")]
        public IActionResult PreLoadAppDataHere()
        {
            System.IO.File.AppendAllText(@"C:\temp\WriteText.txt", "Test");
            return Ok();
        }
    }
}