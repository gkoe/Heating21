using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Api.Helper;

using Common.Helper;

using Core.Contracts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Serilog;

using Services.Contracts;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class ActorsController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApiSettings _apiSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ISerialCommunicationService _serialCommunicationService;

        public ActorsController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IOptions<ApiSettings> options,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ISerialCommunicationService serialCommunicationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _apiSettings = options.Value;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _serialCommunicationService = serialCommunicationService;
        }


        [HttpGet("{actorName},{state}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Change(string actorName, int state)
        {
            Log.Error("Change Actor {actor} to state: {state}", actorName, state);
            await _serialCommunicationService.SetActorAsync(actorName, state);
            //await Task.Run(() => _serialCommunicationService.Send($"heating/{actorName}/command:{state}"));
            return Ok(true);
        }

    }
}
