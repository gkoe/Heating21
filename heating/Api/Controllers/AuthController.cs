using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Base.DataTransferObjects;
using Base.Entities;
using Base.Helper;

using Core.Contracts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Serilog;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AuthController : Controller
    {
        private SignInManager<IdentityUser> SignInManager { get; }
        private UserManager<IdentityUser> UserManager { get; }
        private IUnitOfWork UnitOfWork { get; }
        public IConfiguration Configuration { get; }

        public AuthController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            UserManager = userManager;
            Configuration = configuration;
            SignInManager = signInManager;
            UnitOfWork = unitOfWork;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto userRequestDto)
        {
            if (userRequestDto == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new ApplicationUser
            {
                UserName = userRequestDto.Email,
                Email = userRequestDto.Email,
                Name = userRequestDto.Name,
                PhoneNumber = userRequestDto.PhoneNo,
                EmailConfirmed = true
            };

            var result = await UserManager.CreateAsync(user, userRequestDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new ApiResponseDto
                { Errors = errors, IsSuccessful = false });
            }
            return Created("", new ApiResponseDto { IsSuccessful = true });
        }

        [HttpPost]
        [Authorize(Roles = MagicStrings.Role_Admin)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterByAdmin([FromBody] UserDetailsDto userRequestDto)
        {
            if (userRequestDto == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var userByMail = await UnitOfWork.ApplicationUsers.FindByEmailAsync(userRequestDto.Email);
            if (userByMail != null)
            {
                return BadRequest("User with email already exists");
            }

            var user = new ApplicationUser
            {
                UserName = userRequestDto.Email,
                Email = userRequestDto.Email,
                Name = userRequestDto.Name,
                PhoneNumber = userRequestDto.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await UserManager.CreateAsync(user, userRequestDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new ApiResponseDto
                { Errors = errors, IsSuccessful = false });
            }
            if (!string.IsNullOrEmpty(userRequestDto.Role))
            {
                await UserManager.AddToRoleAsync(user, userRequestDto.Role);
            }
            return Created("", new ApiResponseDto { IsSuccessful = true });
        }

        [HttpPost]
        [Authorize(Roles = MagicStrings.Role_Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpsertByAdmin([FromBody] UserDetailsDto upsertUserDto)
        {
            if (upsertUserDto == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            ApplicationUser dbUser;
            if (upsertUserDto.Id != null)  // Update
            {
                dbUser = await UserManager.FindByIdAsync(upsertUserDto.Id) as ApplicationUser;
                if (dbUser == null)
                {
                    return NotFound();
                }
                if (upsertUserDto.Email != dbUser.Email)
                {
                    if (await UserManager.FindByEmailAsync(upsertUserDto.Email) is ApplicationUser otherUser)
                    {
                        var errors = new string[] { "Other user with mailadress already exists" };
                        return BadRequest(new ApiResponseDto
                        { Errors = errors, IsSuccessful = false });
                    }
                }
                dbUser.Email = upsertUserDto.Email;
                dbUser.PhoneNumber = upsertUserDto.PhoneNumber;
                dbUser.Name = upsertUserDto.Name;
                var dbUserRoles = await UserManager.GetRolesAsync(dbUser);
                if ((upsertUserDto.Role == null || upsertUserDto.Role.Length == 0) &&
                    (dbUserRoles != null || dbUserRoles.Count == 0))  // Rolle ist zu löschen
                {
                    await UserManager.RemoveFromRolesAsync(dbUser, dbUserRoles);
                }
                else if (RoleChanged(upsertUserDto.Role, dbUserRoles))
                {
                    if (dbUserRoles.Count > 0)
                    {
                        await UserManager.RemoveFromRolesAsync(dbUser, dbUserRoles);  // alte Rolle löschen
                    }
                    await UserManager.AddToRoleAsync(dbUser, upsertUserDto.Role);  // neue Rolle zuordnen
                }
                var result = await UserManager.UpdateAsync(dbUser);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new ApiResponseDto
                    { Errors = errors, IsSuccessful = false });
                }

                if (!string.IsNullOrEmpty(upsertUserDto.NewPassword))
                {
                    var token = await UserManager.GeneratePasswordResetTokenAsync(dbUser);
                    result = await UserManager.ResetPasswordAsync(dbUser, token, upsertUserDto.NewPassword);
                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description);
                        return BadRequest(new ApiResponseDto
                        { Errors = errors, IsSuccessful = false });
                    }
                }

                return Ok(new ApiResponseDto { IsSuccessful = true });
            }
            else  // Insert
            {
                var user = new ApplicationUser
                {
                    UserName = upsertUserDto.Email,
                    Email = upsertUserDto.Email,
                    Name = upsertUserDto.Name,
                    PhoneNumber = upsertUserDto.PhoneNumber,
                    EmailConfirmed = true
                };

                var newUser = await UserManager.CreateAsync(user, upsertUserDto.NewPassword);
                if (upsertUserDto.Role != null && upsertUserDto.Role.Length > 0)
                {
                    await UserManager.AddToRoleAsync(user, upsertUserDto.Role);
                }

                if (!newUser.Succeeded)
                {
                    var errors = newUser.Errors.Select(e => e.Description);
                    return BadRequest(new ApiResponseDto
                    { Errors = errors, IsSuccessful = false });
                }
                return Created("", new ApiResponseDto { IsSuccessful = true });
            }

        }


        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditUser([FromBody] UserDetailsDto editUserDto)
        {
            if (editUserDto == null || !ModelState.IsValid || editUserDto.Id == null)
            {
                return BadRequest();
            }

            ApplicationUser dbUser;
            dbUser = await UserManager.FindByIdAsync(editUserDto.Id) as ApplicationUser;
            if (dbUser == null)
            {
                return NotFound();
            }
            if (editUserDto.Email != dbUser.Email)
            {
                if (await UserManager.FindByEmailAsync(editUserDto.Email) is ApplicationUser otherUser)
                {
                    var errors = new string[] { "Other user with mailadress already exists" };
                    return BadRequest(new ApiResponseDto
                    { Errors = errors, IsSuccessful = false });
                }
            }
            dbUser.Email = editUserDto.Email;
            dbUser.PhoneNumber = editUserDto.PhoneNumber;
            dbUser.Name = editUserDto.Name;
            var result = await UserManager.UpdateAsync(dbUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new ApiResponseDto
                { Errors = errors, IsSuccessful = false });
            }

            if (!string.IsNullOrEmpty(editUserDto.NewPassword))
            {
                result = await UserManager.ChangePasswordAsync(dbUser, editUserDto.OldPassword, editUserDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new ApiResponseDto
                    { Errors = errors, IsSuccessful = false });
                }
            }
            return Ok(new ApiResponseDto { IsSuccessful = true });
        }


        private static bool RoleChanged(string newRole, IList<string> oldRoles)
        {
            if (oldRoles.Count == 0)
            {
                return true;
            }
            return newRole != oldRoles[0];
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto signInDto)
        {
            var result = await SignInManager.PasswordSignInAsync(signInDto.UserName,
                signInDto.Password, false, false);
            if (result.Succeeded)
            {
                if ((await UserManager.FindByNameAsync(signInDto.UserName)) is not ApplicationUser user)
                {
                    return Unauthorized(new LoginResponseDto
                    {
                        IsSuccessful = false,
                        ErrorMessage = "Invalid Authentication"
                    });
                }

                //everything is valid and we need to login the user

                var signinCredentials = GetSigningCredentials();
                var claims = await GetClaims(user);

                var appSettingsSection = Configuration.GetSection("AuthSettings");
                var tokenOptions = new JwtSecurityToken(
                    audience: appSettingsSection["ValidAudience"],
                    issuer: appSettingsSection["ValidIssuer"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: signinCredentials);

                var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                Session session = new()
                {
                    ApplicationUserId = user.Id,
                    Login = DateTime.Now
                };
                await UnitOfWork.Sessions.AddAsync(session);
                await UnitOfWork.SaveChangesAsync();

                var loginResponseDto = new LoginResponseDto
                {
                    IsSuccessful = true,
                    Token = token,
                    User = new UserDto
                    {
                        Name = user.Name,
                        Id = user.Id,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    }
                };

                return Ok(loginResponseDto);
            }
            else
            {
                return Unauthorized(new LoginResponseDto
                {
                    IsSuccessful = false,
                    ErrorMessage = "Invalid Authentication"
                });
            }
        }

        private SigningCredentials GetSigningCredentials()
        {
            var appSettingsSection = Configuration.GetSection("AuthSettings");
            var key = Encoding.ASCII.GetBytes(appSettingsSection["SecretKey"]);
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims(IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Email),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("Id",user.Id),

            };
            var roles = await UserManager.GetRolesAsync(await UserManager.FindByEmailAsync(user.Email));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }

        [HttpGet("{applicationUserId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromRoute] string applicationUserId)
        {
            var user = await UnitOfWork.ApplicationUsers.GetByUserIdAsync(applicationUserId);
            if (user == null)
            {
                return NotFound($"User with ID: {applicationUserId} doesn't exist");
            }
            var session = await UnitOfWork.Sessions.GetLastByUserAsync(applicationUserId);
            if (session != null)
            {
                session.Logout = DateTime.UtcNow;
            }
            else
            {
                Log.Error($"AuthController;Logout(); User {user.Email} has no cookie-session in db");
            }
            await UnitOfWork.SaveChangesAsync();
            await SignInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var users = await UnitOfWork.ApplicationUsers.GetWithRolesAndLastLogin();
            return Ok(users);
        }

        [HttpGet("{userId}")]
        [Authorize()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(string userId)
        {
            var user = await UnitOfWork.ApplicationUsers.GetUserDto(userId);
            return Ok(user);
        }


        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete([FromBody] string userId)
        {

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            var result = await UserManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return NoContent();
            }
            return Unauthorized(new ApiResponseDto
            {
                IsSuccessful = false,
                Errors = new string[] { "Delete failed" }
            });
        }



    }
}
