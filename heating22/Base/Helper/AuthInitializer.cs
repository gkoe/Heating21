
using Base.Contracts.Persistence;
using Base.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;

namespace Base.Helper
{
    public class AuthInitializer
    {
        private UserManager<IdentityUser> UserManager { get; set; }
        private RoleManager<IdentityRole> RoleManager { get; set; }

        public AuthInitializer(UserManager<IdentityUser> userManager,
                             RoleManager<IdentityRole> roleManager)
        {
            RoleManager = roleManager;
            UserManager = userManager;
        }

        public void Initalize()
        {
            if (RoleManager.Roles.Any(x => x.Name == MagicStrings.Role_Admin)) return;

            RoleManager.CreateAsync(new IdentityRole(MagicStrings.Role_Admin)).GetAwaiter().GetResult();
            RoleManager.CreateAsync(new IdentityRole(MagicStrings.Role_User)).GetAwaiter().GetResult();
            RoleManager.CreateAsync(new IdentityRole(MagicStrings.Role_Guest)).GetAwaiter().GetResult();

            UserManager.CreateAsync(new ApplicationUser
            {
                Name = "Admin",
                UserName = "admin@htl.at",
                Email = "admin@htl.at",
                EmailConfirmed = true
            }, "Admin123*").GetAwaiter().GetResult();
            UserManager.CreateAsync(new ApplicationUser
            {
                Name = "User",
                UserName = "user@htl.at",
                Email = "user@htl.at",
                EmailConfirmed = true
            }, "User123*").GetAwaiter().GetResult();

            var user = UserManager.FindByEmailAsync("admin@htl.at").GetAwaiter().GetResult();
            UserManager.AddToRoleAsync(user, MagicStrings.Role_Admin).GetAwaiter().GetResult();
            user = UserManager.FindByEmailAsync("user@htl.at").GetAwaiter().GetResult();
            UserManager.AddToRoleAsync(user, MagicStrings.Role_User).GetAwaiter().GetResult();
        }
    }
}
