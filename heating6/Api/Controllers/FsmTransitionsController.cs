using System;
using System.Linq;
using System.Threading.Tasks;

using Base.Helper;

using Core.Contracts;
using Core.DataTransferObjects;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

using Services;
using Services.Contracts;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class FsmTransitionsController : Controller
    {
        //private readonly SignInManager<IdentityUser> _signInManager;
        //private readonly UserManager<IdentityUser> _userManager;
        //private readonly ApiSettings _apiSettings;
        private IUnitOfWork UnitOfWork { get; init; }
        //private readonly IConfiguration _configuration;
        //private readonly ISerialCommunicationService _serialCommunicationService;

        public FsmTransitionsController(
            //SignInManager<IdentityUser> signInManager,
            //UserManager<IdentityUser> userManager,
            //IOptions<ApiSettings> options,
            IUnitOfWork unitOfWork
            //IConfiguration configuration,
            //ISerialCommunicationService serialCommunicationService
            )
        {
            //_userManager = userManager;
            //_signInManager = signInManager;
            //_apiSettings = options.Value;
            UnitOfWork = unitOfWork;
            //_configuration = configuration;
            //_serialCommunicationService = serialCommunicationService;
        }


        [HttpGet("{date}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByDate([FromRoute] DateTime date)
        {
            var fsmTransitions = await UnitOfWork.FsmTransitions.GetByDay(date);
            if (fsmTransitions == null)
            {
                return NotFound($"No fsmtransitions for  day {date.ToShortDateString()}");
            }
            return Ok(fsmTransitions);
        }


    }
}
