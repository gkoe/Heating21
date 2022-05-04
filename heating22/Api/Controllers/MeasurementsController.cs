using Api.Helper;

using Base.Helper;

using Core.Contracts;
using Core.DataTransferObjects;

using IotServices.DataTransferObjects;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Controller to manage measurements
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MeasurementsController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        public IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public MeasurementsController(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        /// <summary>
        /// get all measurements ordered by time desc
        /// optional filtered by itemname
        /// </summary>
        /// <response code="200">Die Abfrage war erfolgreich.</response>
        [HttpGet]
        [ProducesResponseType(typeof(Core.DataTransferObjects.MeasurementDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery]string? itemname)
        {
            var measurements = await UnitOfWork.Measurements.GetFilteredAsync(itemname);
            return Ok(measurements);
        }

        /// <summary>
        /// get all measurements by itemm and date
        /// ordered by time
        /// </summary>
        /// <response code="404">Sensor not found</response>
        [ProducesResponseType(typeof(IotServices.DataTransferObjects.MeasurementTimeValue[]), StatusCodes.Status200OK)]
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
                .Select(m => new MeasurementTimeValueDto(m.Time, m.Value))
                //.Take(100)
                .ToArray();
            return Ok(measurementDtos);
        }


    }
}
