﻿using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistance
{
	public class OrderContextSeed
	{
		public static async Task SeedAsync(OrderContext context, ILogger<OrderContextSeed> logger)
		{
			if(!context.Orders.Any())
			{
				context.Orders.AddRange(GetPreconfiguredOrders());
				await context.SaveChangesAsync();
				logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
			}
		}

		private static IEnumerable<Order> GetPreconfiguredOrders()
		{
			return new List<Order>
			{
				new Order(){ UserName = "swn", FirstName = "Adam", LastName = "Smith", EmailAddress = "Adam@gmail.com", AddressLine = " NY Road", Country = "India", TotalPrice = 10000}
			};
		} 
	}
}
