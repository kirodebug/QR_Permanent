using DBPQRPermanent.Data;
using DBPQRPermanent.Models;
using DBPQRPermanent.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DBPQRPermanent.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly QRService _qrService;

        public HomeController(AppDbContext db, QRService qrService)
        {
            _db = db;
            _qrService = qrService;
        }

        [HttpGet("/")]
        public IActionResult Index() => View("Welcome");

        [HttpGet("/generate")]
        public IActionResult Generate() => View("Generate");

        [HttpPost("/generate")]
        [ValidateAntiForgeryToken]
        public IActionResult GeneratePost(string empId)
        {
            if (string.IsNullOrWhiteSpace(empId))
            {
                TempData["Error"] = "Please enter your Employee ID.";
                return View("Generate");
            }

            empId = empId.Trim().ToUpper();
            var employee = _db.Employees.FirstOrDefault(e => e.EmpId == empId);
            if (employee == null)
            {
                TempData["Error"] = "Employee ID not found. Please contact your administrator.";
                return View("Generate");
            }

            if (!_db.QRCodes.Any(q => q.EmpId == empId))
            {
                _db.QRCodes.Add(new QRCode
                {
                    EmpId = empId,
                    QRImageUrl = $"/contact/{empId}",
                    GeneratedAt = DateTime.UtcNow
                });
                _db.SaveChanges();
            }

            return RedirectToAction("QRScreen", new { empId });
        }

        [HttpGet("/qr/{empId}")]
        public IActionResult QRScreen(string empId)
        {
            empId = empId.Trim().ToUpper();
            var employee = _db.Employees.FirstOrDefault(e => e.EmpId == empId);
            if (employee == null) return View("NotFound");

            if (!_db.QRCodes.Any(q => q.EmpId == empId))
            {
                _db.QRCodes.Add(new QRCode { EmpId = empId, QRImageUrl = $"/contact/{empId}", GeneratedAt = DateTime.UtcNow });
                _db.SaveChanges();
            }

            ViewBag.Employee = employee;
            return View("QRScreen");
        }

        [HttpGet("/qr-image/{empId}")]
        public IActionResult QRImage(string empId)
        {
            empId = empId.Trim().ToUpper();
            var employee = _db.Employees.FirstOrDefault(e => e.EmpId == empId);
            if (employee == null) return NotFound();

            string contactUrl = $"{Request.Scheme}://{Request.Host}/contact/{empId}";
            byte[] qrBytes = _qrService.GenerateQRCode(contactUrl);
            return File(qrBytes, "image/png");
        }

        // Scan QR → phone saves contact directly
        [HttpGet("/contact/{empId}")]
        public IActionResult Contact(string empId)
        {
            empId = empId.Trim().ToUpper();
            var e = _db.Employees.FirstOrDefault(emp => emp.EmpId == empId);
            if (e == null) return NotFound();

            string lastName = e.Name.Contains(" ") ? e.Name.Substring(e.Name.LastIndexOf(' ') + 1) : "";
            string firstName = e.Name.Contains(" ") ? e.Name.Substring(0, e.Name.IndexOf(' ')) : e.Name;
            string telFull = e.TelOffice + (!string.IsNullOrWhiteSpace(e.TelLocal) ? $" loc. {e.TelLocal}" : "");

            var sb = new StringBuilder();
            sb.Append("BEGIN:VCARD\r\n");
            sb.Append("VERSION:3.0\r\n");
            sb.Append($"FN:{e.Name}\r\n");
            sb.Append($"N:{lastName};{firstName};;;\r\n");
            sb.Append($"ORG:{e.Organization};{e.Department};{e.Section}\r\n");
            sb.Append($"TITLE:{e.Title}\r\n");
            if (!string.IsNullOrWhiteSpace(e.Mobile))
                sb.Append($"TEL;TYPE=CELL:{e.Mobile}\r\n");
            if (!string.IsNullOrWhiteSpace(e.TelOffice))
                sb.Append($"TEL;TYPE=WORK,VOICE:{telFull}\r\n");
            if (!string.IsNullOrWhiteSpace(e.Email))
                sb.Append($"EMAIL;TYPE=WORK:{e.Email}\r\n");
            if (!string.IsNullOrWhiteSpace(e.Address))
                sb.Append($"ADR;TYPE=WORK:;;{e.Address};;;;\r\n");
            if (!string.IsNullOrWhiteSpace(e.Website))
                sb.Append($"URL:{e.Website}\r\n");
            if (!string.IsNullOrWhiteSpace(e.Facebook))
                sb.Append($"X-SOCIALPROFILE;type=facebook:{e.Facebook}\r\n");
            sb.Append($"NOTE:Employee ID: {e.EmpId}\r\n");
            sb.Append("END:VCARD\r\n");

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            string filename = e.Name.Replace(" ", "-") + ".vcf";
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{filename}\"");
            return File(bytes, "text/vcard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
