using Microsoft.Extensions.Configuration;
using PaxDeiBotApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var settings = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "appsettings.json"))
            .Build();

        var bot = new DiscordBot(settings);

        await bot.MainAsync();
    }
}