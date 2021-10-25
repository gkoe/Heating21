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
    public class MeasurementsController : Controller
    {
        //private readonly SignInManager<IdentityUser> _signInManager;
        //private readonly UserManager<IdentityUser> _userManager;
        //private readonly ApiSettings _apiSettings;
        private IUnitOfWork UnitOfWork { get; init; }
        //private readonly IConfiguration _configuration;
        //private readonly ISerialCommunicationService _serialCommunicationService;

        public MeasurementsController(
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


        [HttpGet("{actorName},{state}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Change(string actorName, int state)
        {
            Log.Information("Change Actor {actor} to state: {state}", actorName, state);
            await RuleEngine.Instance.SerialCommunicationService.SetActorAsync(actorName, state);
            //await Task.Run(() => _serialCommunicationService.Send($"heating/{actorName}/command:{state}"));
            return Ok(true);
        }


        [HttpGet("{sensorName},{date}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySensorAndDate([FromRoute] string sensorName, DateTime date)
        {
            var measurements = await UnitOfWork.Measurements.GetByDay(sensorName, date);
            if (measurements == null)
            {
                return NotFound($"No measurements for  sensor {sensorName} and day {date.ToShortDateString()}");
            }
            var measurementDtos = measurements
                .Select(m =>
                {
                    var dto = new MeasurementDto();
                    MiniMapper.CopyProperties(dto, m);
                    return dto;
                })
                .Take(100)
                .ToArray();
            return Ok(measurementDtos);
        }


    }
}
