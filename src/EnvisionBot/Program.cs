using EnvisionBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using TwitterSharp.Client;

namespace EnvisionBot;

public static class Program
{
    private static IHostBuilder ConfigureServices(IHostBuilder hostBuilder)
    {
        hostBuilder
            .AddDiscordService(services => services.GetRequiredService<IConfiguration>().GetSection("Discord").GetValue<string>("token"))
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(new TwitterClient(context.Configuration.GetSection("Twitter").GetValue<string>("token")));
                services.AddSingleton<IHostedService, TwitterRepostService>();
                services.Configure<DiscordGatewayClientOptions>(options =>
                {
                    options.Intents |=
                        GatewayIntents.MessageContents |
                        GatewayIntents.GuildMessages;
                });
            });

        return hostBuilder;
    }

    public static async Task Main()
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder().UseConsoleLifetime();

        hostBuilder.ConfigureAppConfiguration(configuration =>
        {
            configuration.SetBasePath(Directory.GetCurrentDirectory());
            configuration.AddJsonFile("appSettings.json", true, true);
        });

        ConfigureServices(hostBuilder);

        IHost host = hostBuilder.Build();

        await host.RunAsync();
    }
}
