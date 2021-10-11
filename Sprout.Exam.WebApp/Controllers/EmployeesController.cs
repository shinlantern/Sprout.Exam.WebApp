using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Sprout.Exam.Business.DataTransferObjects;
//using Sprout.Exam.Common.Enums;
using Sprout.Exam.WebApp.Models;
using Microsoft.EntityFrameworkCore;
using Sprout.Exam.Common.ViewModels;

namespace Sprout.Exam.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private SproutExamDbContext db = new SproutExamDbContext();
        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var dbResult = await Task.FromResult(db.Employees.Where(a => !a.IsDeleted).ToList());
            IEnumerable<EmployeeDto> employees_data = dbResult.Select(a => new EmployeeDto {
                Birthdate = a.Birthdate.ToString("yyyy-MM-dd"),
                FullName = a.FullName,
                Id = a.Id,
                Tin = a.Tin,
                TypeId = a.EmployeeTypeId
            });
            return Ok(employees_data);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var dbResult = await Task.FromResult(db.Employees.FirstOrDefault(a => a.Id == id));
                EmployeeDto employee_data = new EmployeeDto()
                {
                    Birthdate = dbResult.Birthdate.ToString("yyyy-MM-dd"),
                    FullName = dbResult.FullName,
                    Id = dbResult.Id,
                    Tin = dbResult.Tin,
                    TypeId = dbResult.EmployeeTypeId
                };
                return Ok(employee_data);
            }
            catch (Exception e)
            {
                return NotFound(id);
            }
        }

        /// <summary>
        /// Refactor this method to go through proper layers and update changes to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(EditEmployeeDto input)
        {
            try
            {
                var item = await Task.FromResult(db.Employees.FirstOrDefault(m => m.Id == input.Id));
                if (item == null) return NotFound();
                item.FullName = input.FullName;
                item.Tin = input.Tin;
                item.Birthdate = Convert.ToDateTime(input.Birthdate.ToString("yyyy-MM-dd"));
                item.EmployeeTypeId = input.TypeId;

                await Task.FromResult(db.Entry(item).State = EntityState.Modified);
                await db.SaveChangesAsync();

                return Ok(item);
            }
            catch (Exception e)
            {
                return NotFound(input.Id);
            }
        }

        /// <summary>
        /// Refactor this method to go through proper layers and insert employees to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CreateEmployeeDto input)
        {
            Employee new_employee = new Employee()
            {
                Birthdate = input.Birthdate,
                EmployeeTypeId = input.TypeId,
                FullName = input.FullName,
                IsDeleted = false,
                Tin = input.Tin
            };

            await Task.FromResult(db.Employees.Add(new_employee));
            await db.SaveChangesAsync();

            return Created($"/api/employees/{new_employee.Id}", new_employee.Id);
        }


        /// <summary>
        /// Refactor this method to go through proper layers and perform soft deletion of an employee to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dbResult = await Task.FromResult(db.Employees.FirstOrDefault(a => a.Id == id));
                if (dbResult == null) return NotFound();

                dbResult.IsDeleted = true;
                await Task.FromResult(db.Entry(dbResult).State = EntityState.Modified);
                await db.SaveChangesAsync();
                return Ok(id);
            }
            catch (Exception e)
            {
                return NotFound(id);
            }
        }

        /// <summary>
        /// Refactor this method to go through proper layers and use Factory pattern
        /// </summary>
        /// <param name="id"></param>
        /// <param name="absentDays"></param>
        /// <param name="workedDays"></param>
        /// <returns></returns>
        /// 
        [HttpPost("{id}/calculate")]
        public async Task<IActionResult> Calculate(int id, [FromBody] SalaryCalculateViewModel data)
        {
            var dbResult = await Task.FromResult(db.Employees.FirstOrDefault(a => a.Id == id));
            if (dbResult == null) return NotFound();
            var type = (Sprout.Exam.Common.Enums.EmployeeType)dbResult.EmployeeTypeId;
            return type switch
            {
                Sprout.Exam.Common.Enums.EmployeeType.Regular =>
                    //create computation for regular.
                    Ok(Math.Round((20000 - ((20000 / 22) * (double)data.absentDays) - (20000 * 0.12)), 2)),
                Sprout.Exam.Common.Enums.EmployeeType.Contractual =>
                    //create computation for contractual.
                    Ok(Math.Round(500 * data.workedDays, 2)),
                _ => NotFound("Employee Type not found")
            };

        }

    }
}
