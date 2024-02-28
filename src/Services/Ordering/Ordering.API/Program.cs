using Ordering.Application;
using Ordering.Infrastructure;
using System.Linq;
using System;
using System.Threading.Tasks;
using Ordering.API.Extensions;
using Ordering.Infrastructure.Persistance;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using MediatR;
using Ordering.Infrastructure.Persistence;
using MassTransit;
using EventBus.Messages.Events;
using EventBus.Messages.Common;
using Ordering.API.EventBusConsumer;

namespace Ordering.API
{
    public class Program
    {
	 
		public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
			builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
			builder.Services.AddApplicationServices();
			builder.Services.AddInfrastructureServices(builder.Configuration);

			// Mass Transit Rabbit MQ Configuration
			builder.Services.AddMassTransit(config =>
			{
                config.AddConsumer<BasketCheckoutConsumer>();
				config.UsingRabbitMq((ctx, cfg) =>
				{
					cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
                    cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>
                    {
                        c.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
                    });
				});
			});

            // General Configuration
            builder.Services.AddAutoMapper(typeof(Program));


			var app = builder.Build();
            app.MigrateDatabase<OrderContext>((context, services) =>
            {
                var logger = services.GetService<ILogger<OrderContextSeed>>();
                OrderContextSeed.SeedAsync(context, logger).Wait();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();
            app.Run();
        }
    }
}