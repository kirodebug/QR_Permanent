using DBPQRPermanent.Data;
using DBPQRPermanent.Models;
using System;
using System.Linq;

namespace DBPQRPermanent.Services
{
    public class DatabaseSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (db.Employees.Any()) return;

            db.Employees.AddRange(
                new Employee
                {
                    EmpId = "0000001-DBP",
                    Name = "Patricia T. Roque",
                    Title = "Vice President",
                    Department = "IT Operations Group",
                    Section = "ICT Sector",
                    Mobile = "+63 917-6380349",
                    TelOffice = "+632 8818-9511",
                    TelLocal = "3210",
                    Email = "ptroque@dbp.ph",
                    Address = "Sen. Gil J. Puyat Avenue corner Makati Avenue, Makati City",
                    Website = "www.dbp.ph",
                    Facebook = "fb.com/devbankphl",
                    Organization = "Development Bank of the Philippines",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Employee
                {
                    EmpId = "0000002-DBP",
                    Name = "Juan Dela Cruz",
                    Title = "Branch Manager",
                    Department = "Makati Branch",
                    Mobile = "+639001234567",
                    TelOffice = "+632 8818-0000",
                    TelLocal = "1234",
                    Email = "jdelacruz@dbp.ph",
                    Address = "DBP Head Office, Makati City",
                    Organization = "Development Bank of the Philippines",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            db.SaveChanges();
        }
    }
}
