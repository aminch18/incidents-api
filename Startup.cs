using Incidents_Api;
using Incidents_Api.DataAccess;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Incidents_Api
{
	class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			var services = builder?.Services;
			var configuration = AddCustomConfiguration(services);
			ConfigureServices(configuration, services);
		}

		private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
		{
			string connectionString = configuration.GetValue<string>("CosmosDbConnectionString");
			string dataBaseName = configuration.GetValue<string>("CosmosDataBaseName");
			string containerName = configuration.GetValue<string>("ContainerName");
			var cosmosClient = new CosmosClient(connectionString);
			services.AddSingleton<IRepository>(new Repository(cosmosClient, dataBaseName, containerName));
		}

		private IConfiguration AddCustomConfiguration(IServiceCollection services)
		{
			var configuration = new ConfigurationBuilder()
			  .SetBasePath(Environment.CurrentDirectory)
			  .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
			  .AddEnvironmentVariables()
			  .Build();

			services.AddSingleton<IConfiguration>(configuration);
			return configuration;
		}
	}
}
