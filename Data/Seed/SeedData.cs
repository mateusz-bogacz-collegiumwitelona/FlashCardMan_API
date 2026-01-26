using Data.Context;
using Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Data.Seed
{
    public class SeedData
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public SeedData(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole<Guid>> roleManager
            )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await SeedRoleAsync();
            await SeedUsersAsync();
        }

        private async Task SeedRoleAsync()
        {
            string[] roles =
            {
                "User"
            };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            string userName = "User";
            string userEmail = "user@example.pl";
            string userPassword = "Password123!";

            if (await _userManager.FindByEmailAsync(userEmail) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userName,
                    NormalizedUserName = userName.ToUpper(),
                    Email = userEmail,
                    NormalizedEmail = userEmail.ToUpper(),
                    EmailConfirmed = true,
                    DateTime = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    SecurityStamp = Guid.NewGuid().ToString()
                };


                var isSaved = await _userManager.CreateAsync(user, userPassword);

                if (isSaved.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                else
                { 
                    throw new Exception("Seed User failed");
                }
            }
        }
    }
}
