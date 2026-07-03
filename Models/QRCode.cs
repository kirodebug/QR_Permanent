using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBPQRPermanent.Models
{
    public class QRCode
    {
        [Key] public int Id { get; set; }
        [Required] public string EmpId { get; set; } = "";
        public string QRImageUrl { get; set; } = "";
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("EmpId")] public Employee Employee { get; set; }
    }
}
