using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sprout.Exam.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sprout.Exam.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeTypeController : Controller
    {
        private SproutExamDbContext db = new SproutExamDbContext();

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var dbResult = await Task.FromResult(db.EmployeeType.ToList());
            return Ok(dbResult.Select(a => new {
                value = a.Id,
                text = a.TypeName
            }));
        }
    }
}
