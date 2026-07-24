using DBPQRPermanent.Data;
using DBPQRPermanent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DBPQRPermanent.Controllers
{
    [ApiController]
    [Route("api/employees")]
    [Produces("application/json")]
    public class EmployeesApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EmployeesApiController(AppDbContext db)
        {
            _db = db;
        }

        private static string GenerateToken()
        {
            var bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }

        /// <summary>Get all employees</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _db.Employees
                .Select(e => new {
                    e.EmpId, e.Name, e.Title, e.Department, e.Section,
                    e.Mobile, e.TelOffice, e.TelLocal, e.Email,
                    e.Address, e.Website, e.Organization,
                    e.CreatedAt, e.UpdatedAt
                })
                .ToListAsync();
            return Ok(employees);
        }

        /// <summary>Get one employee by Employee ID</summary>
        [HttpGet("{empId}")]
        public async Task<IActionResult> GetOne(string empId)
        {
            var e = await _db.Employees.FindAsync(empId.ToUpper());
            if (e == null) return NotFound(new { message = $"Employee '{empId}' not found." });
            return Ok(e);
        }

        /// <summary>Create a new employee (auto-generates their QR token)</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.EmpId) || string.IsNullOrWhiteSpace(employee.Name))
                return BadRequest(new { message = "EmpId and Name are required." });

            employee.EmpId = employee.EmpId.Trim().ToUpper();

            if (await _db.Employees.AnyAsync(e => e.EmpId == employee.EmpId))
                return Conflict(new { message = $"Employee '{employee.EmpId}' already exists." });

            employee.CreatedAt = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;

            _db.Employees.Add(employee);

            // Auto-generate permanent QR token for this employee
            var qr = new QRCode
            {
                EmpId = employee.EmpId,
                Token = GenerateToken(),
                GeneratedAt = DateTime.UtcNow
            };
            _db.QRCodes.Add(qr);

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { empId = employee.EmpId },
                new { employee, qrToken = qr.Token });
        }

        /// <summary>Update an existing employee's info</summary>
        [HttpPut("{empId}")]
        public async Task<IActionResult> Update(string empId, [FromBody] Employee updated)
        {
            empId = empId.Trim().ToUpper();
            var existing = await _db.Employees.FindAsync(empId);
            if (existing == null) return NotFound(new { message = $"Employee '{empId}' not found." });

            // Update fields — EmpId never changes
            existing.Name         = updated.Name ?? existing.Name;
            existing.Title        = updated.Title ?? existing.Title;
            existing.Department   = updated.Department ?? existing.Department;
            existing.Section      = updated.Section ?? existing.Section;
            existing.Mobile       = updated.Mobile ?? existing.Mobile;
            existing.TelOffice    = updated.TelOffice ?? existing.TelOffice;
            existing.TelLocal     = updated.TelLocal ?? existing.TelLocal;
            existing.Email        = updated.Email ?? existing.Email;
            existing.Address      = updated.Address ?? existing.Address;
            existing.Website      = updated.Website ?? existing.Website;
            existing.Facebook     = updated.Facebook ?? existing.Facebook;
            existing.Organization = updated.Organization ?? existing.Organization;
            existing.UpdatedAt    = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(existing);
        }

        /// <summary>Delete an employee and their QR token</summary>
        [HttpDelete("{empId}")]
        public async Task<IActionResult> Delete(string empId)
        {
            empId = empId.Trim().ToUpper();
            var employee = await _db.Employees.FindAsync(empId);
            if (employee == null) return NotFound(new { message = $"Employee '{empId}' not found." });

            // Remove QR record too
            var qr = _db.QRCodes.FirstOrDefault(q => q.EmpId == empId);
            if (qr != null) _db.QRCodes.Remove(qr);

            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Employee '{empId}' deleted." });
        }

        /// <summary>Get the QR token info for an employee</summary>
        [HttpGet("{empId}/qr")]
        public async Task<IActionResult> GetQR(string empId)
        {
            empId = empId.Trim().ToUpper();
            var qr = await _db.QRCodes.FirstOrDefaultAsync(q => q.EmpId == empId);
            if (qr == null) return NotFound(new { message = "No QR token found for this employee." });

            return Ok(new {
                qr.EmpId,
                qr.Token,
                qr.GeneratedAt,
                contactUrl = $"/contact/{qr.Token}"
            });
        }

        /// <summary>Regenerate QR token for an employee (old QR becomes invalid)</summary>
        [HttpPost("{empId}/qr/regenerate")]
        public async Task<IActionResult> RegenerateQR(string empId)
        {
            empId = empId.Trim().ToUpper();
            var employee = await _db.Employees.FindAsync(empId);
            if (employee == null) return NotFound(new { message = $"Employee '{empId}' not found." });

            var qr = await _db.QRCodes.FirstOrDefaultAsync(q => q.EmpId == empId);
            if (qr == null)
            {
                qr = new QRCode { EmpId = empId, Token = GenerateToken(), GeneratedAt = DateTime.UtcNow };
                _db.QRCodes.Add(qr);
            }
            else
            {
                qr.Token = GenerateToken();
                qr.GeneratedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "QR token regenerated.", qr.Token, contactUrl = $"/contact/{qr.Token}" });
        }
    }
}
