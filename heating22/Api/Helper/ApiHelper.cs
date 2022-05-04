
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Helper
{
    /// <summary>
    /// Helpermethods for API
    /// </summary>
    public static class ApiHelper
    {
        /// Von r.stropek dndLight
        /// <summary>
        /// Gets the URL for retrieving room details.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id">Id of the room for which we want the Url</param>
        /// <param name="url">URL helper (you get get it from the <see cref="ControllerBase.Url"/> property)</param>
        /// <param name="request">Current HTTP request (you get get it from the <see cref="ControllerBase.Request"/> property)</param>
        /// <returns>
        /// Url for the room details (e.g. https://localhost:7243/api/rooms/4)
        /// </returns>
        /// <remarks>
        /// Note that this implementation is correct. However, it is not simple to understand.
        /// In exams, it is ok if you use a simple string building mechanism with string constants.
        /// In real life however, you should use an algorithm similar to this one.
        /// </remarks>
        public static string GetEntityUrl(string name, int id, IUrlHelper url, HttpRequest request)
        {
            if (url == null || request == null)  // UnitTests
            {
                return "";
            }
            var uri = url.RouteUrl(
                name,
                new { id },
                request.Scheme,
                request.Host.ToString());
            return uri ?? "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetEntityUrl(string name, string id, IUrlHelper url, HttpRequest request)
        {
            if (url == null || request == null)  // UnitTests
            {
                return "";
            }
            var uri = url.RouteUrl(
                name,
                new { id },
                request.Scheme,
                request.Host.ToString());
            return uri ?? "";
        }
    }
}
