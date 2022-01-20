using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

using Services;
using Services.Contracts;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class MaintenanceController : Controller
    {
        //private readonly SignInManager<IdentityUser> _signInManager;
        //private readonly UserManager<IdentityUser> _userManager;
        //private readonly ApiSettings _apiSettings;
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly IConfiguration _configuration;
        //private readonly ISerialCommunicationService _serialCommunicationService;
        //public IRaspberryIoService RaspberryIoService { get; }

        //public MaintenanceController(
        //    SignInManager<IdentityUser> signInManager,
        //    UserManager<IdentityUser> userManager,
        //    IOptions<ApiSettings> options,
        //    IUnitOfWork unitOfWork,
        //    IConfiguration configuration,
        //    ISerialCommunicationService serialCommunicationService,
        //    IRaspberryIoService raspberryIoService
        //    )
        //{
        //    _userManager = userManager;
        //    _signInManager = signInManager;
        //    _apiSettings = options.Value;
        //    _unitOfWork = unitOfWork;
        //    _configuration = configuration;
        //    _serialCommunicationService = serialCommunicationService;
        //    RaspberryIoService = raspberryIoService;
        //}


        [HttpGet()]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RestartFsmsAsync()
        {
            Log.Information("ESP reseted");
            await RuleEngine.Instance.RaspberryIoService.ResetEspAsync();
            await Task.Delay(10000);  // warten, dass ESP konstant läuft
            RuleEngine.Instance.StartFiniteStateMachines();
            return Ok();
        }

    }
}
