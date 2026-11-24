using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Data
{
    // Creates the database (if needed), seeds roles, a default admin account,
    // and a handful of lookup/sample records so the app is usable immediately
    // after first run.
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Roles
            string[] roles = { "Admin", "Receptionist", "Doctor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Default admin account
            const string adminEmail = "admin@clinic.com";
            const string adminPassword = "Admin@12345";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Sample specialties
            if (!context.Specialties.Any())
            {
                context.Specialties.AddRange(
                    new Specialty { Name = "General Medicine", Description = "Primary care and general checkups." },
                    new Specialty { Name = "Dentistry", Description = "Dental care and oral health." },
                    new Specialty { Name = "Pediatrics", Description = "Medical care for infants, children and adolescents." },
                    new Specialty { Name = "Dermatology", Description = "Skin, hair and nail conditions." }
                );
                await context.SaveChangesAsync();
            }

            // 4. Sample doctor so Appointments/Visits can be tested immediately
            if (!context.Doctors.Any())
            {
                var generalMedicine = context.Specialties.First(s => s.Name == "General Medicine");
                context.Doctors.Add(new Doctor
                {
                    FullName = "Dr. Sarah Johnson",
                    Email = "sarah.johnson@clinic.com",
                    PhoneNumber = "0100-000-0000",
                    SpecialtyId = generalMedicine.Id
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
