using System.Threading.Tasks;

using Base.DataTransferObjects;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Serilog;

using Services;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class RuleEngineController : Controller
    {
        public RuleEngineController()
        {
        }

        [HttpGet("{on}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SetManualOperation([FromRoute] int on)
        {
            Log.Information("Change manual operation to: {on}");
            RuleEngine.Instance.SetManualOperation(on > 0);
            await RuleEngine.Instance.RaspberryIoService.ResetEspAsync();
            return Ok(true);
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetFsmStates()
        {
            Log.Information("RuleEngineController;GetFsmStates");
            string[] states =
            {
                RuleEngine.Instance.OilBurner.Fsm.ActState.StateEnum.ToString(),
                RuleEngine.Instance.HotWater.Fsm.ActState.StateEnum.ToString(),
                RuleEngine.Instance.HeatingCircuit.Fsm.ActState.StateEnum.ToString()
            };
            return Ok(states);
        }

        [HttpGet("{floor}/{tenthOfDegree}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SetTargetTemperature([FromRoute] int floor, double tenthOfDegree)
        {
            Log.Information($"SetTargetTemperature; Floor: {floor}, Temperature: {tenthOfDegree}");
            if (floor == 1)
            {
                //RuleEngine.Instance.HeatingCircuit.TargetTemperature = tenthOfDegree/10.0;
                await RuleEngine.Instance.SetTargetTemperatureAsync(tenthOfDegree / 10.0);
            }
            //await RuleEngine.Instance.RaspberryIoService.ResetEspAsync();
            return Ok();
        }

        [HttpGet("{floor}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetTargetTemperature([FromRoute] int floor)
        {
            Log.Information($"GetTargetTemperature; Floor: {floor}");
            //await RuleEngine.Instance.RaspberryIoService.ResetEspAsync();
            var tenthOfDegree = (int)(RuleEngine.Instance.HeatingCircuit.TargetTemperature * 10);
            return Ok(tenthOfDegree);
        }




    }
}
