
using Common.Helper;
using Common.Persistence;
using Common.Persistence.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;

namespace Api.Services
{
    public class DbInitializer
    {
        private readonly CommonUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public DbInitializer(CommonUnitOfWork unitOfWork, 
                             UserManager<IdentityUser> userManager,
                             RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initalize()
        {
            try
            {
                if (_unitOfWork.BaseApplicationDbContext.Database.GetPendingMigrations().Any())
                {
                    _unitOfWork.BaseApplicationDbContext.Database.Migrate();
                }
            }
            catch (Exception)
            {
                throw;
            }

            if (_roleManager.Roles.Any(x => x.Name == MagicStrings.Role_Admin)) return;

            _roleManager.CreateAsync(new IdentityRole(MagicStrings.Role_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(MagicStrings.Role_User)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(MagicStrings.Role_Guest)).GetAwaiter().GetResult();

            _userManager.CreateAsync(new ApplicationUser
            {
                Name = "admin",
                UserName = "admin@htl.at",
                Email = "admin@htl.at",
                EmailConfirmed = true
            }, "Admin123*").GetAwaiter().GetResult();
            _userManager.CreateAsync(new ApplicationUser
            {
                Name = "user",
                UserName = "user@htl.at",
                Email = "user@htl.at",
                EmailConfirmed = true
            }, "User123*").GetAwaiter().GetResult();
            _userManager.CreateAsync(new ApplicationUser
            {
                Name = "guest",
                UserName = "guest@htl.at",
                Email = "guest@htl.at",
                EmailConfirmed = true
            }, "Guest123*").GetAwaiter().GetResult();
            _userManager.CreateAsync(new ApplicationUser
            {
                Name = "norole",
                UserName = "norole@htl.at",
                Email = "norole@htl.at",
                EmailConfirmed = true
            }, "Norole123*").GetAwaiter().GetResult();

            var user = _userManager.FindByEmailAsync("admin@htl.at").GetAwaiter().GetResult();
            //IdentityUser user = _unitOfWork.Users.FirstOrDefault(u => u.Email == "admin@htl.at") as IdentityUser;
            _userManager.AddToRoleAsync(user, MagicStrings.Role_Admin).GetAwaiter().GetResult();
            user = _userManager.FindByEmailAsync("user@htl.at").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(user, MagicStrings.Role_User).GetAwaiter().GetResult();
            user = _userManager.FindByEmailAsync("guest@htl.at").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(user, MagicStrings.Role_Guest).GetAwaiter().GetResult();
        }
    }
}
