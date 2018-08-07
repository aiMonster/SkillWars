using Common.Interfaces.Services;
using DataAccessLayer.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Slack;
using Services.AccountService;
using Services.LobbieService;
using Services.LoginService;
using Services.PaymentService;
using Services.SendingService;
using Services.TimeredFunctionsService;
using SkillWars.Extensions;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using Services.WebSockets.Handlers;
using Common.DTO.Sockets;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using YxRadio.Gallery.API.Swagger;
using Services.ImageStorageManager;
using Services.FacebookSubscriber;

namespace SkillWars
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("EmailSubjects.json", optional: true)
                .AddJsonFile("PhoneSms.json", optional: true);
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
                options.OperationFilter<UploadImageOperation>();
                options.OperationFilter<DownloadImageOperation>();
                //var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                //var xmlPath = Path.Combine(basePath, "SkillWarsDoc.xml");
                var path = String.Format(@"{0}\SkillWarsDoc.xml", AppDomain.CurrentDomain.BaseDirectory);
                if (File.Exists(path))
                    options.IncludeXmlComments(path);


            });            

            services.AddDbContext<SkillWarsContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            
            //singleton services
            services.AddSingleton(_ => Configuration);           
            services.AddSingleton<ILoginService, LoginService>();
            services.AddSingleton<IHtmlGeneratorService, HtmlGeneratorService>();
            services.AddSingleton<ITimeredFunctionsService, TimeredFunctionsService>();
            services.AddSingleton<IPaymentService, PaymentService>();
            services.AddSingleton<IFacebookSubscriber, FacebookSubscriberBot>();

            //transient services
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ISmsService, SmsService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<ILobbieService, LobbieService>();

            services.AddTransient<IImageStorageManager, ImageStorageManager>(serviceProvider =>
            {
                var env = serviceProvider.GetService<IHostingEnvironment>();
                var path = Path.Combine(env.ContentRootPath, Configuration["ImagesPath"]);
                return new ImageStorageManager(path);
            });


            services.AddCors();
            services.AddWebSocketManager();

            //services.AddMvc();
            services.AddMvc(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            return services.BuildServiceProvider();
        }
                
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime lifetime, IConfigurationRoot configuration, IHtmlGeneratorService htmlGeneratorService, ITimeredFunctionsService timeredFunctionsService, IServiceProvider serviceProvider, IFacebookSubscriber facebookSubscriber)
        {
            var multimediaPath = Path.Combine(env.ContentRootPath, Configuration["ImagesPath"]);
            if (!Directory.Exists(multimediaPath))
            {
                Directory.CreateDirectory(multimediaPath);
            }

            SetUpLogger(env, loggerFactory, lifetime, configuration);
            app.UseExceptionHandlerMiddleware();
            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            htmlGeneratorService.SetPath(Path.Combine(env.ContentRootPath, "wwwroot/EmailHtmlForms"));
            await timeredFunctionsService.Setup();

            facebookSubscriber.Setup(Path.Combine(env.ContentRootPath, "wwwroot"));

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            var wsOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(wsOptions);
            app.MapWebSocketManager("/lobbie", serviceProvider.GetService<LobbieHandler>());
            app.UseMvc();
        }

        private void SetUpLogger(IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory, IApplicationLifetime lifetime, IConfigurationRoot configuration)
        {
            var logPath = Path.Combine(hostingEnvironment.ContentRootPath, "Logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            var multiPath = Path.Combine(hostingEnvironment.ContentRootPath, "Multimedia");
            if (!Directory.Exists(multiPath))
            {
                Directory.CreateDirectory(multiPath);
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
                        .WriteTo.RollingFile(@"Logs\Fatal-{Date}.log")
                        .WriteTo.Slack(configuration.GetSection("SlackChannels")["Fatal"])) 
                    .CreateLogger();
            }

            loggerFactory.AddSerilog(logger);

            lifetime.ApplicationStarted.Register(() => { logger.Information(messagesTemplate[0]); });
            lifetime.ApplicationStopping.Register(() => { logger.Information(messagesTemplate[1]); });           
        }
    }
   
}
