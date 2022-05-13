using System.Threading.Tasks;

using Base.DataTransferObjects;

using Core.DataTransferObjects;

using IotServices.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Serilog;

using Services;
using Services.ControlComponents;

namespace Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class RuleEngineController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        public RuleEngineController()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="on"></param>
        /// <returns></returns>
        [HttpGet("{on}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SetManualOperation([FromRoute] int on)
        {
            Log.Information("Change manual operation to: {on}");
            RuleEngine.Instance.SetManualOperation(on > 0);
            EspHttpCommunicationService.Instance.RestartEsp();  
            //await RaspberryIoService.Instance.ResetEspAsync();
            return Ok(true);
        }

        /// <summary>
        /// Sendet die aktuellen Werte aus dem Stateservice.
        /// </summary>
        /// <returns>aktuelle Messwerte der Sensoren und Aktoren</returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(MeasurementDto[]),  StatusCodes.Status200OK)]
        public IActionResult GetSensorAndActorValues()
        {
            Log.Information("RuleEngineController;GetSensorValues");
            var sensorValues = StateService.Instance.GetSensorAndActorValues();
            return Ok(sensorValues);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetFsmStates()
        {
            Log.Information("RuleEngineController;GetFsmStates");
            string[] states =
            {
                OilBurner.Instance.Fsm.ActState.StateEnum.ToString(),
                HotWater.Instance.Fsm.ActState.StateEnum.ToString(),
                HeatingCircuit.Instance.Fsm.ActState.StateEnum.ToString()
            };
            return Ok(states);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="tenthOfDegree"></param>
        /// <returns></returns>
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        [HttpGet("{floor}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetTargetTemperature([FromRoute] int floor)
        {
            Log.Information($"GetTargetTemperature; Floor: {floor}");
            //await RuleEngine.Instance.RaspberryIoService.ResetEspAsync();
            var tenthOfDegree = (int)(HeatingCircuit.Instance.TargetTemperature * 10);
            return Ok(tenthOfDegree);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetOilBurnerTargetTemperature()
        {
            Log.Information($"GetOilBurnerTargetTemperature");
            var tenthOfDegree = (int)(OilBurner.TargetTemperature * 10);
            return Ok(tenthOfDegree);
        }

    }
}
