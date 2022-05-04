using Microsoft.AspNetCore.Mvc;
using Core.Contracts;
using Base.Helper;
using Api.Helper;
using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Serilog;
using Core.Validations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Base.Entities;
using Microsoft.AspNetCore.Authorization;
using Base.Validations;
using Base.DataTransferObjects;

namespace Api.Controllers.Auth
{
    #region DTOs
    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="Email"></param>
    ///// <param name="Name"></param>
    ///// <param name="Password"></param>
    //public record RegisterDto(
    //    [Required, EmailAddress] string Email,
    //    [Required] string Name,
    //    [Required, CheckPasswordRules] string Password);


    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="Id"></param>
    ///// <param name="Email"></param>
    ///// <param name="Name"></param>
    ///// <param name="OldPassword"></param>
    ///// <param name="NewPassword"></param>
    //public record UpdateDto(
    //    [Required] string Id,
    //    [Required, EmailAddress] string Email,
    //    [Required] string Name,
    //    [CheckPasswordRules] string? OldPassword,
    //    [CheckPasswordRules] string? NewPassword
    //    );

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="Email"></param>
    ///// <param name="Password"></param>
    //public record LoginRequestDto(
    //    [Required] string Email,
    //    [Required, CheckPasswordRules] string Password);

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="AuthToken"></param>
    ///// <param name="Email"></param>
    //public record LoginResponseDto(
    //    string AuthToken,
    //    string Email);

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="Id"></param>
    ///// <param name="Email"></param>
    ///// <param name="Name"></param>
    ///// <param name="RoleName"></param>
    //public record UserGetDto(
    //    string Id,
    //    string Email,
    //    string Name,
    //    string? RoleName
    //    );
    #endregion


    /// <summary>
    /// Controller for managing users
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// MS Identity Usermanager
        /// </summary>
        public UserManager<IdentityUser> UserManager { get; }

        /// <summary>
        /// MS Identity Rolemanager
        /// </summary>
        public RoleManager<IdentityRole> RoleManager { get; }

        /// <summary>
        /// MS Identity Signinmanager
        /// </summary>
        public SignInManager<IdentityUser> SigninManager { get; }


        /// <summary>
        /// Constructor with DI
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="userManager"></param>
        /// <param name="roleManager"></param>
        /// <param name="signinManager"></param>
        public UsersController(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signinManager)
        {
            UnitOfWork = unitOfWork;
            UserManager = userManager;
            RoleManager = roleManager;
            SigninManager = signinManager;
        }

        /// <summary>
        /// Liefert alle Benutzer zurück, wenn im AuthorizationHeader
        /// ein gültiger Token mitgegeben wurde
        /// </summary>
        /// <response code="401">unauthorized</response>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserDetailsDto[]), StatusCodes.Status200OK)]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            //var userDtos = await UserManager.Users.ToListAsync();
            var result = new List<UserDetailsDto>();
            var users = await UnitOfWork.ApplicationUsers.GetWithRolesAndLastLogin();
            foreach (var user in users)
            {
                //var applicationUser = (ApplicationUser)user;
                //string roleName = await GetRoleNameAsync(applicationUser);
                result.Add(new UserDetailsDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    RoleName = user.RoleName,
                    PhoneNumber = user.PhoneNumber,
                    LastLogin = user.LastLogin,
                });
            }
            return Ok(result.ToArray());
        }

        private async Task<string> GetRoleNameAsync(ApplicationUser user)
        {
            var roles = await UserManager.GetRolesAsync(user);
            var name = roles?.FirstOrDefault() == null ? "" : roles[0];
            return name;
        }

        async Task<UserGetDto> GetUserDto(ApplicationUser user)
        {
            var roles = await UserManager.GetRolesAsync(user);
            var roleName = roles?.FirstOrDefault() == null ? "" : roles[0];
            return new UserGetDto(user.Id, user.Email, user.UserName, roleName, user.PhoneNumber);
        }

        /// <summary>
        /// Get a single AuthUser by id
        /// </summary>
        /// <response code="404">user with id not found</response>
        /// <param name="id">ID of the AuthUser to get</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserGetDto), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            var user = (ApplicationUser)(await UserManager.FindByIdAsync(id));
            if (user == null) { return NotFound(); }
            var roleName = await GetRoleNameAsync(user);
            return Ok(new UserGetDto(user.Id, user.Email, user.Name, roleName, user.PhoneNumber));
        }

        /// <summary>
        /// Neuen User anlegen. Muss eindeutig sein
        /// </summary>
        /// <param name="newAuthUser"></param>
        /// <response code="400">register user causes error</response>
        /// <returns>angelegte neuer Benutzer</returns>
        [ProducesResponseType(typeof(UserGetDto), StatusCodes.Status201Created)]
        [HttpPost(Name = nameof(Register))]
        public async Task<IActionResult> Register(RegisterRequestDto newAuthUser)
        {
            var user = new ApplicationUser
            {
                UserName = newAuthUser.Email,
                Email = newAuthUser.Email,
                Name = newAuthUser.Name,
                EmailConfirmed = true
            };
            var result = await UserManager.CreateAsync(user, newAuthUser.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(string.Join(',', errors));
            }
            return Created(ApiHelper.GetEntityUrl(nameof(GetById), user.Id, Url, Request),
                new UserGetDto(user.Id, user.Email, user.Name, "", user.PhoneNumber));
        }

        /// <summary>
        /// Benutzer meldet sich an.
        /// Gibt es den Benutzer nicht, oder stimmt das Passwort nicht, wird 401 zurückgegeben.
        /// Sonst wird ein JWT erzeugt und zurückgegeben.
        /// </summary>
        /// <param name="loginRequestDto"></param>
        /// <response code="401">unauthorized</response>
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [HttpPost()]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var result = await SigninManager.PasswordSignInAsync(loginRequestDto.Email,
                loginRequestDto.Password, false, false);
            if (result.Succeeded)
            {
                if ((await UserManager.FindByNameAsync(loginRequestDto.Email)) is not ApplicationUser user)
                {
                    return Unauthorized("Invalid Authentication");
                }
                var signinCredentials = AuthUtils.GetSigningCredentials();
                var claims = await AuthUtils.GetClaims(user, UserManager);
                var issuer = ConfigurationHelper.GetConfiguration("ValidIssuer", "APISettings");
                var audience = ConfigurationHelper.GetConfiguration("ValidAudience", "APISettings");
                var tokenOptions = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
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

                var loginResponseDto = new LoginResponseDto(token, user.Email);
                return Ok(loginResponseDto);
            }
            else
            {
                return Unauthorized("Invalid Authentication");
            }
        }

        /// <summary>
        /// logout user
        /// </summary>
        /// <param name="id"></param>
        /// <response code="401">unauthorized</response>
        /// <response code="200">user logged out</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Logout([FromRoute] string id)
        {
            if ((await UserManager.FindByIdAsync(id)) is not ApplicationUser)
            {
                return Unauthorized("Unautorized");
            }
            var dbUser = await UnitOfWork.ApplicationUsers.GetByUserIdAsync(id);
            if (dbUser == null)
            {
                return NotFound($"User with ID: {id} doesn't exist");
            }
            var session = await UnitOfWork.Sessions.GetLastByUserAsync(id);
            if (session != null)
            {
                session.Logout = DateTime.UtcNow;
            }
            else
            {
                Log.Error($"Logout(); User {dbUser.Email} has no cookie-session in db");
            }
            await UnitOfWork.SaveChangesAsync();
            await SigninManager.SignOutAsync();
            return Ok();
        }

        /// <summary>
        /// Insert or update user
        /// </summary>
        /// <param name="upsertDto"></param>
        /// <response code="400">insert or update user causes error</response>
        /// <response code="404">userid for update not found</response>
        /// <returns>inserted or updated user</returns>
        [HttpPut]
        [Authorize(Roles = MagicStrings.Role_Admin)]
        [ProducesResponseType(typeof(UserGetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UserGetDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> UpsertByAdmin([FromBody] UpsertRequestDto upsertDto)
        {
            if (upsertDto == null)
            {
                return BadRequest("No data in body");
            }
            ApplicationUser dbUser;
            if (!string.IsNullOrEmpty(upsertDto.Id))  // Update
            {
                dbUser = (ApplicationUser)await UserManager.FindByIdAsync(upsertDto.Id);
                if (dbUser == null)
                {
                    return NotFound();
                }
                if (upsertDto.Email != dbUser.Email)
                {
                    if (await UserManager.FindByEmailAsync(upsertDto.Email) is ApplicationUser otherUser)
                    {
                        return BadRequest("Other user with mailadress already exists");
                    }
                }
                dbUser.Email = upsertDto.Email;
                dbUser.Name = upsertDto.Name;
                dbUser.PhoneNumber = upsertDto.PhoneNumber;

                if (!string.IsNullOrEmpty(upsertDto.RoleName) &&
                    !(await RoleManager.RoleExistsAsync(upsertDto.RoleName)))  // Rolle ist nicht leer, existiert aber nicht
                {
                    return BadRequest($"role {upsertDto.RoleName} doesn't ");
                }
                var dbUserRoles = await UserManager.GetRolesAsync(dbUser);
                if ((upsertDto.RoleName == null || upsertDto.RoleName.Length == 0) &&
                    (dbUserRoles != null || dbUserRoles?.Count == 0))  // Rolle ist zu löschen
                {
                    await UserManager.RemoveFromRolesAsync(dbUser, dbUserRoles);
                }
                else if (dbUserRoles != null && upsertDto?.RoleName != null && AuthUtils.RoleChanged(upsertDto.RoleName, dbUserRoles))
                {
                    if (dbUserRoles.Count > 0)
                    {
                        await UserManager.RemoveFromRolesAsync(dbUser, dbUserRoles);  // alte Rolle löschen
                    }
                    await UserManager.AddToRoleAsync(dbUser, upsertDto.RoleName);  // neue Rolle zuordnen
                }
                var result = await UserManager.UpdateAsync(dbUser);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(string.Join(',', errors));
                }

                if (!string.IsNullOrEmpty(upsertDto?.NewPassword))
                {
                    var token = await UserManager.GeneratePasswordResetTokenAsync(dbUser);
                    result = await UserManager.ResetPasswordAsync(dbUser, token, upsertDto.NewPassword);
                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description);
                        return BadRequest(string.Join(',', errors));
                    }
                }

                return Ok(new UserGetDto(dbUser.Id, dbUser.Email, dbUser.Name, upsertDto?.RoleName ?? "", dbUser.PhoneNumber));
            }
            else  // Insert
            {
                var user = new ApplicationUser
                {
                    UserName = upsertDto.Email,
                    Email = upsertDto.Email,
                    Name = upsertDto.Name,
                    EmailConfirmed = true,
                    PhoneNumber= upsertDto.PhoneNumber,
                };

                var newUser = await UserManager.CreateAsync(user, upsertDto.NewPassword);
                if (upsertDto.RoleName != null && upsertDto.RoleName.Length > 0)
                {
                    await UserManager.AddToRoleAsync(user, upsertDto.RoleName);
                }

                if (!newUser.Succeeded)
                {
                    var errors = newUser.Errors.Select(e => e.Description);
                    return BadRequest(string.Join(',', errors));
                }
                return Created("", new UserGetDto(user.Id, user.Email, user.Name, upsertDto?.RoleName ?? "", user.PhoneNumber));
            }
        }


        /// <summary>
        /// user updates his data
        /// </summary>
        /// <param name="updateRequestDto"></param>
        /// <response code="401">user is unauthorized</response>
        /// <response code="400">update user causes error</response>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(UserGetDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] UserPutDto updateRequestDto)
        {
            var userId = AuthUtils.GetLoggedInUserId(User);  // wer ist aktuell eingeloggt
            if (updateRequestDto.Id != userId)
            {
                return BadRequest("you are not allowed to update another user");
            }
            ApplicationUser user;
            user = (ApplicationUser)await UserManager.FindByIdAsync(updateRequestDto.Id);
            if (user == null)
            {
                return NotFound();
            }
            if (updateRequestDto.Email != user.Email)
            {
                if (await UserManager.FindByEmailAsync(updateRequestDto.Email) is ApplicationUser otherUser)
                {
                    return BadRequest($"Other user with mailadress {updateRequestDto.Email} already exists");
                }
            }
            user.Email = updateRequestDto.Email;
            user.Name = updateRequestDto.Name;
            user.PhoneNumber = updateRequestDto.PhoneNumber;
            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(string.Join(',', errors));
            }

            if (!string.IsNullOrEmpty(updateRequestDto.NewPassword))
            {
                result = await UserManager.ChangePasswordAsync(user, updateRequestDto.OldPassword, updateRequestDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(string.Join(',', errors));
                }
            }
            var roles = await UserManager.GetRolesAsync(user);
            var roleName = roles?.FirstOrDefault() == null ? "" : roles[0];

            return Ok(new UserGetDto(user.Id, user.Email, user.Name, roleName, user.PhoneNumber));
        }


        /// <summary>
        /// Deletes the authUser with the given id 
        /// </summary>
        /// <response code="400">deleting user causes error</response>
        /// <response code="404">user with id not found</response>
        /// <response code="204">user deleted</response>
        /// <param name="userId">ID of the user to delete</param>
        [HttpDelete("{userId}")]
        [Authorize(Roles = MagicStrings.Role_Admin)]
        public async Task<ActionResult> Delete(string userId)
        {
            var authUser = await UserManager.FindByIdAsync(userId);
            if (authUser == null) { return NotFound(); }
            try
            {
                await UnitOfWork.Sessions.RemoveAllByUserAsync(userId);
                await UnitOfWork.SaveChangesAsync();
                var identityResult = await UserManager.DeleteAsync(authUser);
                if (!identityResult.Succeeded)
                {
                    var errorMessages = identityResult.Errors.Select(e => e.Description).ToArray();
                    return BadRequest($"{string.Join(',', errorMessages)}");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Cannot delete user, error: {ex.Message}");
            }
        }

    }
}
