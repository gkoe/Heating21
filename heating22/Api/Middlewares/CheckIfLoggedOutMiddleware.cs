using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

using Core.Contracts;

using Microsoft.AspNetCore.Http;

namespace Api.Middlewares
{
    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/extensibility?view=aspnetcore-5.0

    /// <summary>
    /// Middleware factory based, da IUnitOfWork injiziert werden muss.
    /// Kontrolliert, ob der angemeldete Benutzer blockiert wurde und löscht
    /// bei Blokade die Userdaten aus dem Context.
    /// Muss nach der Authentication (User muss bekannt sein) und vor der Authorization
    /// in die Pipeline eingefügt werden.
    /// </summary>
    public class CheckIfLoggedOutMiddleware : IMiddleware
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork"></param>
        public CheckIfLoggedOutMiddleware(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader))
            {
                var jwtString = authHeader.ToString().Split(' ')[1];
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwt = jwtHandler.ReadJwtToken(jwtString);
                var mail = jwt.Claims.FirstOrDefault(c => c.Type.Contains("emailaddress"))?.Value;
                var user = await _unitOfWork.ApplicationUsers.FindByEmailAsync(mail);
                var session = await _unitOfWork.Sessions.GetLastByUserAsync(user.Id);
                if (session == null || session.Logout != null)
                {
                    context.Request.Headers["Authorization"] = "";
                }
            }
            // Call the next delegate/ middleware in the pipeline
            await next(context);
        }
    }
}