
using Base.Contracts.Persistence;
using Base.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;

namespace Base.Helper
{
    public class DbInitializer
    {
        private IBaseUnitOfWork UnitOfWork { get; set; }
        private UserManager<IdentityUser> UserManager { get; set; }
        private RoleManager<IdentityRole> RoleManager { get; set; }


        public DbInitializer(IBaseUnitOfWork unitOfWork,
                             UserManager<IdentityUser> userManager,
                             RoleManager<IdentityRole> roleManager)
        {
            UnitOfWork = unitOfWork;
            RoleManager = roleManager;
            UserManager = userManager;
        }

        public void Initalize()
        {
            try
            {
                if (UnitOfWork.BaseApplicationDbContext.Database.GetPendingMigrations().Any())
                {
                    UnitOfWork.BaseApplicationDbContext.Database.Migrate();
                }
            }
            catch (Exception)
            {
                throw;
            }

            if (RoleManager.Roles.Any(x => x.Name == MagicStrings.Role_Admin)) return;

            RoleManager.CreateAsync(new IdentityRole(MagicStrings.Role_Admin)).GetAwaiter().GetResult();
            RoleManager.CreateAsync(new IdentityRole(MagicStrings.Role_User)).GetAwaiter().GetResult();
            RoleManager.CreateAsync(new IdentityRole(MagicStrings.Role_Guest)).GetAwaiter().GetResult();

            UserManager.CreateAsync(new ApplicationUser
            {
                Name = "Gerald",
                UserName = "gerald.koeck@aon.at",
                Email = "gerald.koeck@aon.at",
                EmailConfirmed = true
            }, "Gerald123*").GetAwaiter().GetResult();
            UserManager.CreateAsync(new ApplicationUser
            {
                Name = "Sieglinde",
                UserName = "sieglinde.koeck@aon.at",
                Email = "sieglinde.koeck@aon.at",
                EmailConfirmed = true
            }, "Sieglinde123*").GetAwaiter().GetResult();

            var user = UserManager.FindByEmailAsync("gerald.koeck@aon.at").GetAwaiter().GetResult();
            //IdentityUser user = _unitOfWork.Users.FirstOrDefault(u => u.Email == "admin@htl.at") as IdentityUser;
            UserManager.AddToRoleAsync(user, MagicStrings.Role_Admin).GetAwaiter().GetResult();
            user = UserManager.FindByEmailAsync("sieglinde.koeck@aon.at").GetAwaiter().GetResult();
            UserManager.AddToRoleAsync(user, MagicStrings.Role_User).GetAwaiter().GetResult();
        }
    }
}
