using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Serialization;

using Persistence;
using Core.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using Base.Contracts.Persistence;
using Base.Helper;
using Services.Contracts;
using Services;
using Services.Hubs;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSignalR();
            var appSettingsSection = Configuration.GetSection("ConnectionStrings");
            var dbFileName = appSettingsSection["DbFileName"];
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={dbFileName}"));

            services.AddScoped<IUnitOfWork, UnitOfWork>();      // Anwendungsschicht
            services.AddScoped<IBaseUnitOfWork, UnitOfWork>();  // Auth-Schicht kennt IUnitOfWork nicht
            services.AddScoped<DbInitializer>();

            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = true;
                config.Password.RequireUppercase = true;
                config.Password.RequireLowercase = true;
                config.Password.RequireNonAlphanumeric = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();
                //.AddDefaultTokenProviders()
                //.AddDefaultUI();

            appSettingsSection = Configuration.GetSection("AuthSettings");
            var key = Encoding.ASCII.GetBytes(appSettingsSection["SecretKey"]);
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
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidAudience = appSettingsSection["ValidAudience"],  // Zieldomäne
                        ValidIssuer = appSettingsSection["ValidIssuer"],      // Aussteller
                        ClockSkew = TimeSpan.Zero  // keine Überprüfung von Zeitabweichungen
                    };
                });

            services.AddCors(o => o.AddPolicy("DefaultCors", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }));

            services.AddRouting(option => option.LowercaseUrls = true);
            services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null)
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Htl.net", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please Bearer and then token in the field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
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

            //services.AddSingleton<IHttpCommunicationService, HttpCommunicationService>();
            //services.AddSingleton<ISerialCommunicationService, SerialCommunicationService>();
            //services.AddSingleton<IRaspberryIoService, RaspberryIoService>();
            //services.AddSingleton<IStateService, StateService>();
            services.AddHostedService<RuleEngine>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
            }

            dbInitializer.Initalize();  // legt, falls erforderlich Defaultuser und Rollen an

            app.UseHttpsRedirection();
            app.UseCors("DefaultCors");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MeasurementsHub>("/measurementshub");
            });
        }
    }
}
