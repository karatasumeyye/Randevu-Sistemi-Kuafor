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

        public EmployeeLeaveApiController(SalonDbContext context)
        {
            _context = context;
        }

        // GET: api/EmployeeLeaveApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeLeave>>> GetEmployeeLeaves()
        {
            return await _context.EmployeeLeaves.Include(e => e.Employee).ToListAsync();
        }

        // POST: api/EmployeeLeaveApi
        [HttpPost]
        public async Task<ActionResult<EmployeeLeave>> PostEmployeeLeave(EmployeeLeave leave)
        {
            _context.EmployeeLeaves.Add(leave);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmployeeLeave", new { id = leave.LeaveId }, leave);
        }

    }
}