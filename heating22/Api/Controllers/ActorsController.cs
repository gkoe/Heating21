using System.Threading.Tasks;

using IotServices.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

using Services;
using Services.Contracts;

namespace Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class ActorsController : Controller
    {


        /// <summary>
        /// Aktor wird auf den gewünschten Zustand gesetzt.
        /// </summary>
        /// <param name="actorName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <response code="404">user with id not found</response>
        [HttpGet("{actorName},{state}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Change(string actorName, int state)
        {
            Log.Information("Change Actor {actor} to state: {state}", actorName, state);
            bool ok = await StateService.Instance.SetActorAsync(actorName, state);
            if (!ok)
            {
                return BadRequest($"Couldn't change actor {actorName} to state {state}");
            }
            return Ok(true);
        }

    }
}
