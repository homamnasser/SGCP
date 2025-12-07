using SGCP.Helper;
using SGCP.Models;

namespace SGCP.Context
{
    public class Seed
    {
        private readonly DataContext _context;
        public Seed(DataContext context)
        {
            _context = context;
        }

        public void SeedDataContext()
        {
            if (!_context.Roles.Any())
            {
                var roles = new List<Role>()
                {
                    new Role { Name = "Admin" },
                    new Role { Name = "Employee" },
                    new Role { Name = "User" }
                };

                _context.Roles.AddRange(roles);
                _context.SaveChanges();
            }

            if (!_context.Users.Any())
            {
                var adminRole = _context.Roles.FirstOrDefault(r => r.Name == "Admin");

                if (adminRole != null)
                {
                    var adminUser = new User
                    {
                        Name = "Admin",
                        Email = "admin@system.local",
                        Password = PasswordHashHandler.HashPassword("Admin@123"),
                        RoleId = adminRole.Id,
                    };

                    _context.Users.Add(adminUser);
                    _context.SaveChanges();
                }
                else
                {
                    throw new Exception("Admin role not found. Cannot seed admin user.");
                }

            }
        }
    }
}
