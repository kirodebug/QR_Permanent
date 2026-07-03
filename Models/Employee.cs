using System;
using System.ComponentModel.DataAnnotations;

namespace DBPQRPermanent.Models
{
    public class Employee
    {
        [Key] public string EmpId { get; set; } = "";
        [Required] public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Department { get; set; } = "";
        public string Section { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string TelOffice { get; set; } = "";
        public string TelLocal { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string Website { get; set; } = "www.dbp.ph";
        public string Facebook { get; set; } = "fb.com/devbankphl";
        public string Organization { get; set; } = "Development Bank of the Philippines";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
