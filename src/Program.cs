using System.CommandLine;
using ConfigToJson;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Steeltoe.Extensions.Configuration.ConfigServer;

namespace SpringScrap;
/// <summary>
/// This is a .NET tool to fetch configuration from Spring Cloud Config Server and save it as a JSON file.
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        var command = new RootCommand();
        var serviceOption = new Option<string>("--service")
        {
            IsRequired = true,
            Description = "Service name to get configuration"
        };
        serviceOption.AddAlias("-s");
        
        var environmentOption = new Option<string>("--environment")
        {
            IsRequired = true,
            Description = "Environment name to get configuration"
        };
        environmentOption.AddAlias("-e");
        
        var outputOption = new Option<string>("--output", getDefaultValue: () => "appsettings.spring.json")
        {
            Description = "Name of the Output JSON file."
        };
        outputOption.AddAlias("-o");
        
        var springUriOption = new Option<string>("--spring-uri", getDefaultValue: () => "")
        {
            Description = "URI of the spring cloud service. If not provided, it will use the SPRING_SCRAP_URI environment variable."
        };
        springUriOption.AddAlias("-u");
        
        command.Add(serviceOption);
        command.Add(environmentOption);
        command.Add(outputOption);
        command.Add(springUriOption);
        command.SetHandler(
            GetConfigurationFile, 
            serviceOption, 
            environmentOption, 
            outputOption,
            springUriOption);
        await command.InvokeAsync(args);
    }

    private static void GetConfigurationFile(
        string service, string environment, string outputFileName, string springUri)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables(prefix: "SPRING_SCRAP_");
        var defaultConfiguration = configurationBuilder.Build();
        var environmentSprintUri = defaultConfiguration["URI"];
    if (string.IsNullOrEmpty(defaultConfiguration["spring:cloud:config:uri"]) && 
            string.IsNullOrEmpty(springUri) &&
            string.IsNullOrEmpty(environmentSprintUri))
        {
            Console.WriteLine("Spring Cloud URI is missing, please provide it using the --spring-uri option (-u) or set the SPRING_SCRAP_URI environment variable.");
            Console.WriteLine("In order to set the SPRING_SCRAP_URI environment variable, you can use the following commands:");
            Console.WriteLine("Windows PoweShell: $env:SPRING_SCRAP_URI=http://spring:8080");
            Console.WriteLine("Windows CMD: set SPRING_SCRAP_URI=http://spring:8080");
            Console.WriteLine("UNIX: export SPRING_SCRAP_URI=http://spring:8080");
            return;
        }

        var customConfigs = new List<KeyValuePair<string, string?>>
        {
            new("spring:application:name", service),
            new("spring:cloud:config:env", environment)
        };
        var configServerUri = string.IsNullOrEmpty(springUri)
            ? environmentSprintUri
            : springUri;
        if (!string.IsNullOrEmpty(configServerUri))
        {
            customConfigs.Add(new KeyValuePair<string, string?>("spring:cloud:config:uri", configServerUri));
        }

        configurationBuilder.AddInMemoryCollection(customConfigs).AddConfigServer();
            
        var configurationJson = configurationBuilder.Build().ToJToken();
        RemoveProperty(configurationJson, "spring");
        RemoveProperty(configurationJson, "Spring");
        using var streamWriter = new StreamWriter(Path.Combine(outputFileName), true);
        streamWriter.WriteLine(configurationJson);
        Console.WriteLine(new string('=', 50));
        Console.WriteLine($"Configuration saved to {outputFileName}.");
        Console.WriteLine(new string('=', 50));
    }
    
    private static void RemoveProperty(JToken jsonObject, string name)
    {
        jsonObject.SelectToken(name)?.Parent.Remove();
    }
}