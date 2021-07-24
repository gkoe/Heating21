using System;
using System.Linq;
using System.Text;

using Api.Helper;
using Api.Middlewares;
using Api.Services;

using Common.Persistence;

using Core.Contracts;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Serialization;

using Persistence;

using Services;
using Services.Contracts;
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

            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //services.AddDbContext<ApplicationDbContext>();
            //services.AddScoped<ApplicationDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<CommonUnitOfWork, UnitOfWork>();
            services.AddScoped<CheckIfLoggedOutMiddleware>();
            services.AddSingleton<IHttpCommunicationService, HttpCommunicationService>();
            services.AddSingleton<ISerialCommunicationService, SerialCommunicationService>();
            services.AddSingleton<IRaspberryIoService, RaspberryIoService>();
            services.AddSingleton<IStateService,StateService>();
            services.AddHostedService<RuleEngine>();

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()
                .AddDefaultUI();
            appSettingsSection = Configuration.GetSection("APISettings");
            services.Configure<ApiSettings>(appSettingsSection);

            services.Configure<MailJetSettings>(Configuration.GetSection("MailJetSettings"));

            var apiSettings = appSettingsSection.Get<ApiSettings>();
            var key = Encoding.ASCII.GetBytes(apiSettings.SecretKey);

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
                        ValidAudience = apiSettings.ValidAudience,
                        ValidIssuer = apiSettings.ValidIssuer,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<DbInitializer>();

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Template", Version = "v1" });
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


            //services.AddControllers();
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbInitializer dbInitializer)
        {
            app.UseResponseCompression();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseCors("DefaultCors");
            //app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseMiddleware<CheckIfLoggedOutMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            dbInitializer.Initalize();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MeasurementsHub>("/measurementshub");
            });


            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseSwagger();
            //    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
            //}

            //app.UseHttpsRedirection();

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
