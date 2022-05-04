using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Radzen;

using Serilog;
using Serilog.Core;

using Wasm.Services;
using Wasm.Services.Contracts;

namespace Wasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, MyAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthenticationApiService, AuthenticationApiService>();
            builder.Services.AddScoped<IApiService, ApiService>();
            builder.Services.AddScoped<DialogService>();
            builder.Services.AddScoped<NotificationService>();
            //builder.Services.AddScoped<TooltipService>();
            //builder.Services.AddScoped<ContextMenuService>();
            builder.Services.AddScoped<UtilityServices>();
            var baseAddress = builder.Configuration.GetValue<string>("BaseAPIUrl");
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress =
                        new Uri(baseAddress)
            });

            //var levelSwitch = new LoggingLevelSwitch();
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.ControlledBy(levelSwitch)
            //    .Enrich.WithProperty("InstanceId", Guid.NewGuid().ToString("n"))
            //    .CreateLogger();
            //builder.Logging.AddSerilog();

            var x = builder.Build();
            _ = new JwtPayload();
            await x.RunAsync();
        }
    }
}
