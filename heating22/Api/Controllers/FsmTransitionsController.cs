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
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize]
    public class FsmTransitionsController : Controller
    {
        private IUnitOfWork UnitOfWork { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public FsmTransitionsController(
            IUnitOfWork unitOfWork
            )
        {
            UnitOfWork = unitOfWork;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
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
