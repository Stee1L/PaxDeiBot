using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaxDeiBot.WebApi.Client;
using PaxDeiBotApp.Models;

namespace PaxDeiBotApp;

public class DiscordBot
{
    private readonly PaxDeiBotClient _apiClient;
    private DiscordSocketClient _client;

    public DiscordBot(IConfiguration config)
    {
        _apiClient = new PaxDeiBotClient(config["ApiURL"], new HttpClient());
    }

    const string token = "MTIzMjAxNTQ2NzM3MjA4OTQzNw.Gsw5gV.IoZQuNAdifhOJ86rnDHHWTm3an3cfDfkUD1Y8w";
    public async Task MainAsync()
    {
        var config = new DiscordSocketConfig
        {
            AlwaysDownloadUsers = false,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        
        _client = new DiscordSocketClient(config);
        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceivedAsync;
        
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }
    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }
    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Content.Contains("!getData"))
        {
            var itemName = message.Content.Replace("!getData ", "");
            if (string.IsNullOrEmpty(itemName)) await message.Channel.SendMessageAsync("Укажите название предмета после команды !getData");
            // var shortItemApi = "http://localhost:5000/api/v1/Items/components";
            // var shortItemData = _apiClient.ComponentsAsync();
            // if (string.IsNullOrEmpty(shortItemData))
            // {
            //     await message.Channel.SendMessageAsync("Не удалось получить список предметов!");
            //     return;
            // }

            var itemId = Guid.Parse(itemName);
            
            var items = await _apiClient.ComponentsAsync();
            var selectedItem =
                items.FirstOrDefault(item => item.Id == itemId);
    
            if (selectedItem == null)
            {
                await message.Channel.SendMessageAsync($"Предмет с названием {itemName} не найден!");
                return;
            }
    
            // var fullItemApi = $"http://localhost:5000/api/v1/Items/{selectedItem.Id}";
            // var fullItemData = await GetDataFromWebApi(fullItemApi);
            //
            // if (string.IsNullOrEmpty(fullItemData))
            // {
            //     await message.Channel.SendMessageAsync($"Не удалось получить данные по предмету {itemName}");
            //     return;
            // }
            
            var fullItem = await _apiClient.CountAsync(selectedItem.Id, 1);
            
            var messageText = $"**{fullItem.Name}**\n" + 
                                    $"{fullItem.Description}\n";
    
            if (fullItem.Components.Any())
            {
                messageText += "Components:\n" + CompleteTree(messageText, 0, fullItem, 1);
                
            }
    
            await message.Channel.SendMessageAsync(messageText);
        }
    }

    public string CompleteTree(string message, int tabs, TreeItemViewModel model, int depth)
    {
        var msg = string.Empty;
        for (var i = 0; i < tabs; i++)
            msg += "\t";

        msg += $"|-{model.Name} - {model.Description} (Count: {model.Count})\n";

        // if (depth<=0)
        //     return msg;

        foreach (var component in model.Components)
            msg += CompleteTree(msg, tabs + 1, component, depth - 1);
        
        return msg;
    }

}