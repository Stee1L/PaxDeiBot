// See https://aka.ms/new-console-template for more information

using PaxDeiBotApp;

var bot = new DiscordBot();
bot.MainAsync()
    .GetAwaiter()
    .GetResult();