using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using PaxDeiBotApp.Models;

namespace PaxDeiBotApp;

public class DiscordBot
{
    private DiscordSocketClient _client;
    const string token = "MTIzMjAxNTQ2NzM3MjA4OTQzNw.GoD7n8.OsUpdoBcaAeWDw2vzL3EwjYfUYa5kEJfCZj9Co";
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
            var shortItemApi = "http://localhost:5000/api/v1/Items/components";
            var shortItemData = await GetDataFromWebApi(shortItemApi);
            if (string.IsNullOrEmpty(shortItemData))
            {
                await message.Channel.SendMessageAsync("Не удалось получить список предметов!");
                return;
            }
    
            var items = JsonConvert.DeserializeObject<List<ItemShortModel>>(shortItemData);
            var selectedItem =
                items.FirstOrDefault(item => item.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
    
            if (selectedItem == null)
            {
                await message.Channel.SendMessageAsync($"Предмет с названием {itemName} не найден!");
                return;
            }
    
            var fullItemApi = $"http://localhost:5000/api/v1/Items/{selectedItem.Id}";
            var fullItemData = await GetDataFromWebApi(fullItemApi);
    
            if (string.IsNullOrEmpty(fullItemData))
            {
                await message.Channel.SendMessageAsync($"Не удалось получить данные по предмету {itemName}");
                return;
            }
            
            var fullItem = JsonConvert.DeserializeObject<ItemFullModel>(fullItemData) ?? throw new Exception("Data not found");
            
            var messageText = $"**{fullItem.Name}**\n" + 
                                    $"{fullItem.Description}\n";
    
            if (fullItem.Components.Any())
            {
                messageText += "Components:\n";
                foreach (var component in fullItem.Components)
                {
                    messageText += $"- {component.Name} ()\n";
                }
            }
    
            await message.Channel.SendMessageAsync(messageText);
        }
    }
    private async Task<string> GetDataFromWebApi(string apiUrl)
    {
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            else
            {
                return "Произошла ошибка при получении данных из Web Api";
            }
        }
        catch (Exception e)
        {
            return $"Ошибка: {e.Message}";
        }
    }
}