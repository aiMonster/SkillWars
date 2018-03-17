using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Slack;
using Services.LoginService;
using Services.SendingService;
using SkillWars.Extensions;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.IdentityModel.Tokens;

namespace SkillWars
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)           
                .AddJsonFile("EmailSubjects.json", optional: true);
                //.AddEnvironmentVariables();
            Configuration = builder.Build();           
        }        
        
        public IConfigurationRoot Configuration { get; }
        
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.RequireHttpsMetadata = false;
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidIssuer = AuthOptions.ISSUER,

                       ValidateAudience = true,
                       ValidAudience = AuthOptions.AUDIENCE,

                       ValidateLifetime = true,

                       IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                       ValidateIssuerSigningKey = true
                   };
               });

            services.AddSwaggerGen(options =>
            {                
                options.SwaggerDoc("v1", new Info { Title = "SkillWars API", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey",
                });

                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "SkillWarsDoc.xml");

                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });            

            services.AddDbContext<SkillWarsContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //singleton services
            services.AddSingleton(_ => Configuration);            
            services.AddSingleton<ILoginService, LoginService>();
            services.AddSingleton<IHtmlGeneratorService, HtmlGeneratorService>();
            
            //transient services
            services.AddTransient<IEmailService, EmailService>();


            services.AddCors();
            services.AddMvc(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            return services.BuildServiceProvider();
        }
                
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime lifetime, IConfigurationRoot configuration, IHtmlGeneratorService htmlGeneratorService)
        {            
            SetUpLogger(env, loggerFactory, lifetime, configuration);
            app.UseExceptionHandlerMiddleware();
            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            htmlGeneratorService.SetPath(Path.Combine(env.ContentRootPath, "wwwroot/EmailHtmlForms"));

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseMvc();
        }

        private void SetUpLogger(IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory, IApplicationLifetime lifetime, IConfigurationRoot configuration)
        {
            var logPath = Path.Combine(hostingEnvironment.ContentRootPath, "Logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            var messagesTemplate =
                new List<string> { "Web API Started", "Web API Stopped" };

            Serilog.Core.Logger logger;

            if (!hostingEnvironment.IsDevelopment())
            {
                logger = new LoggerConfiguration()
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                        .WriteTo.RollingFile(@"Logs\Info-{Date}.log"))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                        .WriteTo.RollingFile(@"Logs\Debug-{Date}.log"))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                        .WriteTo.RollingFile(@"Logs\Warning-{Date}.log"))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                        .WriteTo.RollingFile(@"Logs\Error-{Date}.log")
                        .WriteTo.Slack(configuration.GetSection("SlackChannels")["Error"]))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
                        .WriteTo.RollingFile(@"Logs\Fatal-{Date}.log")
                        .WriteTo.Slack(configuration.GetSection("SlackChannels")["Fatal"]))                   
                    .CreateLogger();
            }
            else
            {
                logger = new LoggerConfiguration()
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                        .WriteTo.RollingFile(@"Logs\Info-{Date}.log"))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                        .WriteTo.RollingFile(@"Logs\Debug-{Date}.log"))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                        .WriteTo.RollingFile(@"Logs\Warning-{Date}.log"))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                        .WriteTo.RollingFile(@"Logs\Error-{Date}.log"))
                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
                        .WriteTo.RollingFile(@"Logs\Fatal-{Date}.log"))                   
                    .CreateLogger();
            }

            loggerFactory.AddSerilog(logger);

            lifetime.ApplicationStarted.Register(() => { logger.Information(messagesTemplate[0]); });
            lifetime.ApplicationStopping.Register(() => { logger.Information(messagesTemplate[1]); });           
        }
    }
}
