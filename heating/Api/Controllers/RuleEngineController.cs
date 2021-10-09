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

    }
}
