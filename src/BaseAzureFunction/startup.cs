using System;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

//Tells the core process that we want this to run as the DI startup on function usages
[assembly: FunctionsStartup(typeof(BaseAzureFunction.Startup))]

namespace BaseAzureFunction
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
        }

        private IConfiguration Configuration { get; set; }
        
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            //rather than use context.EnvironmentName, we use our own custom environment variable so we can have lots of different configurations not just
            //development, staging and production. This value can be set in the azure functions configuration setup if you want different envs
            string environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            
            var context = builder.GetContext();

            //Adds the basic appsettings
            var configurationBuilder = builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false);

            //If the environment has been supplied and exists then loads the extra settings
            if (!string.IsNullOrWhiteSpace(environment))
            {
                if (!File.Exists(Path.Combine(context.ApplicationRootPath, $"appsettings.{environment}.json")))
                {
                    throw new ArgumentException($"The environment variable is set to {environment} but no appsettings.{environment}.json exists");
                }
                
                configurationBuilder = builder.ConfigurationBuilder
                    .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{environment}.json"), optional: true, reloadOnChange: false);
            }

            configurationBuilder.AddEnvironmentVariables();
            
            //Sets the local parameter so we can use it in our DI configuration for connection strings etc
            Configuration = configurationBuilder.Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //IConfiguration gets added automatically, so if you need to override it you can do it like this
            //builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));
            
            //builder.Services.AddSingleton<IExampleService, ExampleService>();
        }
    }
}