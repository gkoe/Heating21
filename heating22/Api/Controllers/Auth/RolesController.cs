using Microsoft.AspNetCore.Mvc;
using Api.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Base.Helper;

namespace Api.Controllers.Auth
{
    #region DTOs
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name"></param>
    public record RolePostDto([Required, MinLength(4)] string Name);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    public record RolePutDto([Required] string Id, [Required, MinLength(4)] string Name);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    public record RoleGetDto(string Id, string Name);
    #endregion

    /// <summary>
    /// Controller to manage roles
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        /// <summary>
        /// MS Identity rolemanager
        /// </summary>
        public RoleManager<IdentityRole> RoleManager { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="roleManager"></param>
        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            RoleManager = roleManager;
        }

        /// <summary>
        /// GET: api/AuthUsers
        /// </summary>
        /// <returns>All valid roles</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var roles = await RoleManager.Roles.ToListAsync();
            var roleDtos = roles.Select(ar => new RoleGetDto( ar.Id, ar.Name)).ToArray();
            return Ok(roleDtos);
        }

        /// <summary>
        /// Get a single AuthRole by id
        /// </summary>
        /// <param name="id">ID of the AuthRole to get</param>
        /// <response code="404">role with id not found</response>
        /// <returns>role with id</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoleGetDto), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null) { return NotFound(); }

            return Ok(new RoleGetDto(role.Id, role.Name));
        }

        /// <summary>
        /// Neue Rolle anlegen. Muss eindeutig sein
        /// </summary>
        /// <param name="rolePostDto"></param>
        /// <response code="400">adding role causes error</response>
        /// <returns>angelegte neue Rolle</returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Authorize(Roles = MagicStrings.Role_Admin)]
        [HttpPost(Name = nameof(Post))]
        public async Task<IActionResult> Post(RolePostDto rolePostDto)
        {
            if (rolePostDto.Name.Trim().Length == 0)
            {
                return BadRequest("Empty RoleName not allowed");
            }
            if (await RoleManager.RoleExistsAsync(rolePostDto.Name))
            {
                return BadRequest($"Role {rolePostDto.Name} already exists!");
            }
            var identityResult = await RoleManager.CreateAsync(new IdentityRole(rolePostDto.Name));
            if (!identityResult.Succeeded)
            {
                var errorMessages = identityResult.Errors.Select(e => e.Description).ToArray();
                return BadRequest($"{string.Join(',', errorMessages)}");
            }
            var authRole = await RoleManager.FindByNameAsync(rolePostDto.Name);
            return Created(ApiHelper.GetEntityUrl(nameof(GetById), authRole.Id, Url, Request),
                new RoleGetDto(authRole.Id, authRole.Name));
        }

        /// <summary>
        /// Rolle updaten. Darf noch nicht bestehen
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rolePutDto"></param>
        /// <response code="400">updating role causes error</response>
        /// <returns>angelegte neue Rolle</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = MagicStrings.Role_Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, RolePutDto rolePutDto)
        {
            if (rolePutDto.Name.Trim().Length == 0)
            {
                return BadRequest("Empty RoleName not allowed");
            }
            var authRole = await RoleManager.FindByIdAsync(rolePutDto.Id);
            if (authRole == null) { return NotFound(); }
            if (rolePutDto.Id != id) { return BadRequest("ObjectId is different from id"); }
            authRole.Name = rolePutDto.Name;

            var identityResult = await RoleManager.UpdateAsync(authRole);
            if (!identityResult.Succeeded)
            {
                var errorMessages = identityResult.Errors.Select(e => e.Description).ToArray();
                return BadRequest($"{string.Join(',', errorMessages)}");
            }
            return Ok(new RoleGetDto(authRole.Id, authRole.Name));
        }

        /// <summary>
        /// Deletes the authRole with the given id 
        /// </summary>
        /// <param name="id">ID of the user to delete</param>
        /// <response code="400">deleting role causes error</response>
        /// <response code="404">role not found</response>
        /// <response code="204">role deleted</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = MagicStrings.Role_Admin)]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null) { return NotFound(); }
            var identityResult = await RoleManager.DeleteAsync(role);
            if (!identityResult.Succeeded)
            {
                var errorMessages = identityResult.Errors.Select(e => e.Description).ToArray();
                return BadRequest($"{string.Join(',', errorMessages)}");
            }
            return NoContent();
        }


    }
}
