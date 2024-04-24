using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using PaxDeiBot.WebApi.Client;

namespace PaxDeiBotApp;

public class DiscordBot
{
    private readonly PaxDeiBotClient _apiClient;
    private readonly string token;
    private DiscordSocketClient _client;

    public DiscordBot(IConfiguration config)
    {
        _apiClient = new PaxDeiBotClient(config["ApiURL"], new HttpClient());
        token = config["Token"];
    }
    
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

    private Task MessageReceivedAsync(SocketMessage message)
    {
        var msg = message.Content.Split(" ");

        if (msg.Length < 2 && msg[0] != "!help")
        {
            return Task.CompletedTask;
        }

        return msg[0] switch
        {
            "!getData" => GetFullModelDataHandler(message.Channel, msg),
            "!getTreeData" => GetTreeDataHandler(message.Channel, msg),
            "!getPrimalData" => GetPrimalDataHandler(message.Channel, msg),
            "!findByName" => FindByNameHandler(message.Channel, msg),
            _ => Task.CompletedTask
        };
    }

    private async Task GetPrimalDataHandler(ISocketMessageChannel messageChannel, string[] msg)
    {
        if (!Guid.TryParse(msg[1], out var itemId))
            return;

        var item = await _apiClient.ItemsGETAsync(itemId);
        
        var primalComponents = await _apiClient.PrimalAsync(itemId, 1);

        var message = primalComponents.Aggregate($"Для предмета **{item.Name}** необходимо добыть:\n",
            (s, model) => s + $"{model.Name} ({model.Count} шт.)\n");

        await messageChannel.SendMessageAsync(message);
    }

    private async Task GetFullModelDataHandler(ISocketMessageChannel messageChannel, string[] msg)
    {
        if (!Guid.TryParse(msg[1], out var itemId))
            return;

        var item = await _apiClient.DepthAsync(itemId, 0);

        var message = $"**{item.Name}**\n" 
                      + (item.Components.Any() ? item.Components.Aggregate("Состоит из:\n", (s, model) => s + $"{model.Name}, ", s => s[..^2]) + "\n" : string.Empty)
                      + (item.NeedForParents.Any() ? item.NeedForParents.Aggregate("Необходим для:\n", (s, model) => s + $"{model.Name}, ", s => s[..^2]) : string.Empty);
        await messageChannel.SendMessageAsync(message);
    }

    private async Task FindByNameHandler(ISocketMessageChannel messageChannel, string[] messageContent)
    {
        var items = await _apiClient.PageAsync();
        var models = items.Where(f => f.Name.Contains(messageContent[1], StringComparison.OrdinalIgnoreCase));
        await messageChannel.SendMessageAsync(models.Aggregate(string.Empty, (s, model) => s+=$"{model.Name} = {model.Id:N}\n"));
    }

    public async Task GetTreeDataHandler(ISocketMessageChannel messageChannel, string[] messageContent)
    {

        if (!Guid.TryParse(messageContent[1], out var itemId))
        {
            await messageChannel.SendMessageAsync("Указанный параметр не является идентификатором\n" +
                                                  "Попробуйте отправить сообщение в формате !getData <GUID>.");
            return;
        }

        var fullItem = await _apiClient.CountAsync(itemId, 1);

        var messageText = $"**{fullItem.Name}**\n" +
                          $"{fullItem.Description}\n";

        if (fullItem.Components.Any())
            messageText += "Components:\n" + CompleteTree(0, fullItem, 1);

        await messageChannel.SendMessageAsync(messageText);
    }

    public string CompleteTree(int tabs, TreeItemViewModel model, int depth)
    {
        var msg = string.Empty;
        for (var i = 0; i < tabs; i++)
            msg += "\t";

        msg += $"\u255a[{model.Name}]({model.Id}) - {model.Description} (Count: {model.Count})\n";

        // if (depth<=0)
        //     return msg;

        foreach (var component in model.Components)
            msg += CompleteTree(tabs + 1, component, depth - 1);

        return msg;
    }
}