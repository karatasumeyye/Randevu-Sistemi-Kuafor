using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

namespace Randevu_Sistemi_Kuafor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeLeaveApiController : ControllerBase
    {
        private readonly SalonDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeLeaveApiController(SalonDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: api/EmployeeLeaveApi
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<EmployeeLeave>>> GetEmployeeLeaves()
        //{
        //    return await _context.EmployeeLeaves.Include(e => e.Employee).ToListAsync();
        //}


        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeLeave>>> GetEmployeeLeavesByEmployeeId([FromQuery] int employeeId)
        {
            var leaves = await _context.EmployeeLeaves
                                        .Where(l => l.EmployeeId == employeeId)
                                        .ToListAsync();
            if (leaves == null || !leaves.Any())
            {
                return NotFound("No leaves found for this employee.");
            }

            return Ok(leaves);
        }


        // POST: api/EmployeeLeaveApi

        [HttpPost]
        public async Task<ActionResult<EmployeeLeave>> PostEmployeeLeave(EmployeeLeave leave)
        {
            try
            {


                _context.EmployeeLeaves.Add(leave);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetEmployeeLeave", new { id = leave.LeaveId }, leave);
            }
            catch (Exception ex)
            {
                // Hata mesajını logla ve kullanıcıya göster

                return StatusCode(500, "Internal server error occurred.");
            }

        }


        // GET: api/EmployeeLeaveApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeLeave>> GetEmployeeLeave(int id)
        {
            var leave = await _context.EmployeeLeaves.FindAsync(id);

            if (leave == null)
            {
                return NotFound();
            }

            return leave;
        }



        // PUT: api/EmployeeLeaveApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployeeLeave(int id, EmployeeLeave leave)
        {
            if (id != leave.LeaveId)
            {
                return BadRequest();
            }

            // EmployeeId zaten leave modelinin içinde yer alıyor
            _context.Entry(leave).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeLeaveExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/EmployeeLeaveApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeLeave(int id)
        {
            var leave = await _context.EmployeeLeaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            _context.EmployeeLeaves.Remove(leave);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeLeaveExists(int id)
        {
            return _context.EmployeeLeaves.Any(e => e.LeaveId == id);
        }

    }
}