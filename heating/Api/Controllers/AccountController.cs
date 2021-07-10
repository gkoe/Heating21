using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Api.Helper;
using Common.DataTransferObjects;
using Common.Helper;
using Common.Persistence.Entities;
using Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApiSettings _apiSettings;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IOptions<ApiSettings> options,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _apiSettings = options.Value;
            _unitOfWork = unitOfWork;
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

            var result = await _userManager.CreateAsync(user, userRequestDto.Password);

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

            var userByMail = await _unitOfWork.ApplicationUsers.FindByEmailAsync(userRequestDto.Email);
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

            var result = await _userManager.CreateAsync(user, userRequestDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new ApiResponseDto
                { Errors = errors, IsSuccessful = false });
            }
            if (!string.IsNullOrEmpty(userRequestDto.Role))
            {
                await _userManager.AddToRoleAsync(user, userRequestDto.Role);
            }


            //var roleResult = await _userManager.AddToRoleAsync(user, MagicStrings.Role_Retailer);
            //if (!roleResult.Succeeded)
            //{
            //    var errors = result.Errors.Select(e => e.Description);
            //    return BadRequest(new SignUpResponseDto
            //    { Errors = errors, IsSignupSuccessful = false });
            //}
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
                dbUser = await _userManager.FindByIdAsync(upsertUserDto.Id) as ApplicationUser;
                if (dbUser == null)
                {
                    return NotFound();
                }
                if (upsertUserDto.Email != dbUser.Email)
                {
                    if (await _userManager.FindByEmailAsync(upsertUserDto.Email) is ApplicationUser otherUser)
                    {
                        var errors = new string[] { "Other user with mailadress already exists" };
                        return BadRequest(new ApiResponseDto
                        { Errors = errors, IsSuccessful = false });
                    }
                }
                dbUser.Email = upsertUserDto.Email;
                dbUser.PhoneNumber = upsertUserDto.PhoneNumber;
                dbUser.Name = upsertUserDto.Name;
                var dbUserRoles = await _userManager.GetRolesAsync(dbUser);
                if ((upsertUserDto.Role == null || upsertUserDto.Role.Length == 0) &&
                    (dbUserRoles != null || dbUserRoles.Count == 0))  // Rolle ist zu löschen
                {
                    await _userManager.RemoveFromRolesAsync(dbUser, dbUserRoles);
                }
                else if (RoleChanged(upsertUserDto.Role, dbUserRoles))
                {
                    if (dbUserRoles.Count > 0)
                    {
                        await _userManager.RemoveFromRolesAsync(dbUser, dbUserRoles);  // alte Rolle löschen
                    }
                    await _userManager.AddToRoleAsync(dbUser, upsertUserDto.Role);  // neue Rolle zuordnen
                }
                var result = await _userManager.UpdateAsync(dbUser);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new ApiResponseDto
                    { Errors = errors, IsSuccessful = false });
                }

                if (!string.IsNullOrEmpty(upsertUserDto.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(dbUser);
                    result = await _userManager.ResetPasswordAsync(dbUser, token, upsertUserDto.NewPassword);
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

                var newUser = await _userManager.CreateAsync(user, upsertUserDto.NewPassword);
                if (upsertUserDto.Role != null && upsertUserDto.Role.Length > 0)
                {
                    await _userManager.AddToRoleAsync(user, upsertUserDto.Role);
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
            dbUser = await _userManager.FindByIdAsync(editUserDto.Id) as ApplicationUser;
            if (dbUser == null)
            {
                return NotFound();
            }
            if (editUserDto.Email != dbUser.Email)
            {
                if (await _userManager.FindByEmailAsync(editUserDto.Email) is ApplicationUser otherUser)
                {
                    var errors = new string[] { "Other user with mailadress already exists" };
                    return BadRequest(new ApiResponseDto
                    { Errors = errors, IsSuccessful = false });
                }
            }
            dbUser.Email = editUserDto.Email;
            dbUser.PhoneNumber = editUserDto.PhoneNumber;
            dbUser.Name = editUserDto.Name;
            var result = await _userManager.UpdateAsync(dbUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new ApiResponseDto
                { Errors = errors, IsSuccessful = false });
            }

            if (!string.IsNullOrEmpty(editUserDto.NewPassword))
            {
                result = await _userManager.ChangePasswordAsync(dbUser, editUserDto.OldPassword, editUserDto.NewPassword);
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
            var result = await _signInManager.PasswordSignInAsync(signInDto.UserName,
                signInDto.Password, false, false);
            if (result.Succeeded)
            {
                if ((await _userManager.FindByNameAsync(signInDto.UserName)) is not ApplicationUser user)
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

                var tokenOptions = new JwtSecurityToken(
                    issuer: _apiSettings.ValidIssuer,
                    audience: _apiSettings.ValidAudience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: signinCredentials);

                var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                Session session = new()
                {
                    ApplicationUserId = user.Id,
                    Login = DateTime.Now
                };
                await _unitOfWork.Sessions.AddAsync(session);
                await _unitOfWork.SaveChangesAsync();

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
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSettings.SecretKey));
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
            var roles = await _userManager.GetRolesAsync(await _userManager.FindByEmailAsync(user.Email));

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
            var user = await _unitOfWork.ApplicationUsers.GetByUserIdAsync(applicationUserId);
            if (user == null)
            {
                return NotFound($"User with ID: {applicationUserId} doesn't exist");
            }
            var session = await _unitOfWork.Sessions.GetLastByUserAsync(applicationUserId);
            if (session != null)
            {
                session.Logout = DateTime.UtcNow;
            }
            else
            {
                Log.Error($"Logout(); User {user.Email} has no cookie-session in db");
            }
            await _unitOfWork.SaveChangesAsync();
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var users = await _unitOfWork.ApplicationUsers.GetWithRolesAndLastLogin();
            return Ok(users);
        }

        [HttpGet("{userId}")]
        [Authorize()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(string userId)
        {
            var user = await _unitOfWork.ApplicationUsers.GetUserDto(userId);
            return Ok(user);
        }


        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete([FromBody] string userId)
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
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
