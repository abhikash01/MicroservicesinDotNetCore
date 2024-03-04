using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace OcelotApiGw
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddOcelot().AddCacheManager(settings => settings.WithDictionaryHandle());
			var app = builder.Build();
			app.Run();
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration((hostingContext, config) =>
			{
				config.AddJsonFile($"ocelot.{hostingContext.HostingEnvironment.EnvironmentName}.json",true, true);
			})
			.ConfigureLogging((hostingContext, loggingbuilder) => {
				loggingbuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
				loggingbuilder.AddConsole();
				loggingbuilder.AddDebug();
			});
		 
	}
}
