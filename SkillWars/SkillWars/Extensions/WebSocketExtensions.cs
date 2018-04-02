using Common.Interfaces.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Services.WebSockets;
using Services.WebSockets.Handlers;
using SkillWars.WebSockets.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SkillWars.Extensions
{
    public static class WebSocketExtensions
    {
        public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app, PathString path, WebSocketHandler handler)
        {
            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketManagerMiddleware>(handler));
        }

        public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
        {
            services.AddTransient<WebSocketConnectionManager>();


            //services.AddSingleton<WebSocketHandler>();
            services.AddSingleton<LobbieHandler>();            
           
            //foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            //{
            //    if (type.GetTypeInfo().BaseType == typeof(WebSocketHandler))
            //    {
            //        services.AddSingleton(type);
            //    }
            //}

            return services;
        }
    }
}
