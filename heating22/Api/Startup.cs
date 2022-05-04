using System.Reflection;
using System.Text;

using Api.Middlewares;

using Base.Helper;
using Base.Persistence;

using Core.Contracts;

using IotServices.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Serialization;

using Persistence;

using Serilog;

using Services.Hubs;

namespace Api
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = ConfigurationHelper
                .GetConfiguration("DefaultConnection", "ConnectionStrings");
            //_ = services.AddDbContext<ApplicationDbContext>(options =>
            //            options.UseSqlServer(connectionString));
            _ = services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(connectionString));
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<BaseUnitOfWork, UnitOfWork>();
            services.AddScoped<CheckIfLoggedOutMiddleware>();

            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = true;
                config.Password.RequireUppercase = true;
                config.Password.RequireLowercase = true;
                config.Password.RequireNonAlphanumeric = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var secretKey = ConfigurationHelper.GetConfiguration("SecretKey", "APISettings");
            var issuer = ConfigurationHelper.GetConfiguration("ValidIssuer", "APISettings");
            var audience = ConfigurationHelper.GetConfiguration("ValidAudience", "APISettings");

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidAudience = audience,  // Zieldomäne
                        ValidIssuer = issuer,      // Aussteller
                        ClockSkew = TimeSpan.Zero  // keine Überprüfung von Zeitabweichungen
                    };
                });

            services.AddCors(o => o.AddPolicy("DefaultCors", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }));

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<AuthInitializer>();

            services.AddRouting(option => option.LowercaseUrls = true);
            services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null)
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "IotFullstack API",
                    Description = "An API for managing sensors and actors",
                    Contact = new OpenApiContact
                    {
                        Name = "HTBLA Leonding",
                        Url = new Uri("https://www.htl-leonding.at/")
                    }
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please Bearer and then token in the field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                   {
                     new OpenApiSecurityScheme
                     {
                       Reference = new OpenApiReference
                       {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                       }
                      },
                      Array.Empty<string>()
                    }
                });
            });
            services.AddSignalR();
            services.AddHostedService<IotService>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="authInitializer"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AuthInitializer authInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseCors("DefaultCors");
            app.UseSerilogRequestLogging(configure =>
            {
                configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} ({UserId}) responded {StatusCode} in {Elapsed:0.0000}ms";
            }); // We want to log all HTTP requests
            app.UseRouting();

            app.UseMiddleware<CheckIfLoggedOutMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            authInitializer.Initalize();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<SignalRHub>("/measurementshub");
            });
        }
    }
}
