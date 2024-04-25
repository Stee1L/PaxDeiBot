using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using PaxDeiBot.WebApi.Client;

namespace PaxDeiBotApp;

public class DiscordBot
{
    private readonly IConfiguration _config;
    private readonly PaxDeiBotClient _apiClient;
    private readonly string token;
    private DiscordSocketClient _client;
    private readonly List<string> adminIds;

    public DiscordBot(IConfiguration config)
    {
        _config = config;
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
        // Console.WriteLine($"Получено сообщение от пользователя: {message.Author.Username} - ID:{message.Author.Id}");
        var msg = message.Content.Split(" ");

        if (msg.Length < 2 && msg[0] != "!help")
        {
            return Task.CompletedTask;
        }

        return msg[0] switch
        {
            "!addItem" => AddItemHandler(message.Channel, message.Author, msg),
            "!removeItem" => RemoveItemHandler(message.Channel, message.Author, msg),
            "!addChild" => AddChildHandler(message.Channel, message.Author, msg),
            "!removeChild" => RemoveChildHandler(message.Channel, message.Author, msg),
            "!getData" => GetFullModelDataHandler(message.Channel, msg),
            "!getTreeData" => GetTreeDataHandler(message.Channel, msg),
            "!getPrimalData" => GetPrimalDataHandler(message.Channel, msg),
            "!findByName" => FindByNameHandler(message.Channel, msg),
            "!help" => message.Channel.SendMessageAsync("Существующие команды:\n" +
                                                        "!addItem - Добавить предмет\n" +
                                                        "!addItem [Название предмета] [Описание предмета]\n" +
                                                        "!removeItem - Удалить предмет. Удаляется по идентификатору в системе.\n" +
                                                        "!removeItem [itemId]\n" +
                                                        "!addChild - Добавить в рецепт крафта предмета другой по идентификатору.\n" +
                                                        "!addChild [parentId] [childId] [количество]\n" +
                                                        "!removeChild - Удалить зависимость предметов\n" +
                                                        "!removeChild [itemId]\n" +
                                                        "!getData - Получить информацию о предмете, информацию о его сборке, а так же предметы, в крафтах которого он используется\n" +
                                                        "!getData [itemId]\n" +
                                                        "!getTreeData - Получить полный перечень ресурсов для крафта в древовидной форме.\n" +
                                                        "!getTreeData [itemId]\n" +
                                                        "!getPrimalData - Получить список ресурсов, необходимых для этого предмета. Применение:\n" +
                                                        "!getPrimalData [itemId]\n" +
                                                        "!findByName - поиск предмета по имени.\n" +
                                                        "!findByName [Название предмета]" +
                                                        "!help - путеводитель.\n" +
                                                        "**Получить информацию может каждый, но изменять её может только администратор системы!**"),
            _ => Task.CompletedTask
        };
    }

    private async Task RemoveChildHandler(ISocketMessageChannel channel, SocketUser author, string[] msg)
    {
        if (author.IsNotAdmin(_config))
        {
            await channel.SendMessageAsync("Указанное действие можно выполнить только администратору. " +
                                           "Если вы владелец бота, то перейдите к его настройкам.");
            return;
        }
        var isParentParsed = Guid.TryParse(msg[1], out var parent);
        var isChildParsed = Guid.TryParse(msg[2], out var child);
        
        if (!isParentParsed || !isChildParsed)
        {
            await channel.SendMessageAsync("Ошибка чтения команды. Попробуйте воспользоваться:" +
                                           "```\n" +
                                           $"!removeChild [parentId] [childId]" +
                                           "\n```\n" +
                                           $"например:\n" +
                                           $"!removeChild {Guid.NewGuid():N} {Guid.NewGuid():N}");
        }

        try
        {
            await _apiClient.ChildDELETEAsync(parent, child);
            await channel.SendMessageAsync("Успешно удалена зависимость.");
        }
        catch (Exception e)
        {
            await channel.SendMessageAsync("Ошибка исполнения команды:\n" +
                                           $"{e.Message}");
        }
    }

    private async Task AddChildHandler(ISocketMessageChannel channel, SocketUser author, string[] msg)
    {
        if (author.IsNotAdmin(_config))
        {
            await channel.SendMessageAsync("Указанное действие можно выполнить только администратору. " +
                                           "Если вы владелец бота, то перейдите к его настройкам.");
            return;
        }

        var isParentParsed = Guid.TryParse(msg[1], out var parent);
        var isChildParsed = Guid.TryParse(msg[2], out var child);
        var isCountParsed = long.TryParse(msg[3], out var count);
        
        if (!isParentParsed || !isChildParsed || !isCountParsed)
        {
            await channel.SendMessageAsync("Ошибка чтения команды. Попробуйте воспользоваться:" +
                                           "```\n" +
                                           $"!addChild [parentId] [childId] [количество]" +
                                           "\n```\n" +
                                           $"например:\n" +
                                           $"!addChild {Guid.NewGuid():N} {Guid.NewGuid():N} {Random.Shared.Next(1, 100)}");
        }

        try
        {
            await _apiClient.ChildPOSTAsync(parent, child, count);
            await channel.SendMessageAsync("Успешно добавлено.");
        }
        catch (Exception e)
        {
            await channel.SendMessageAsync("Ошибка исполнения команды:\n" +
                                           $"{e.Message}");
        }
    }

    private async Task RemoveItemHandler(ISocketMessageChannel channel, SocketUser author, string[] msg)
    {
        if (author.IsNotAdmin(_config))
        {
            await channel.SendMessageAsync("Указанное действие можно выполнить только администратору. " +
                                           "Если вы владелец бота, то перейдите к его настройкам.");
            return;
        }

        if (!Guid.TryParse(msg[1], out var id))
        {
            await channel.SendMessageAsync("Ошибка в команде удаления");
            return;
        }

        try
        {
            await _apiClient.ItemsDELETEAsync(id);
            await channel.SendMessageAsync("Удаление завершено успешно.");
        }
        catch (Exception e)
        {
            await channel.SendMessageAsync("Произошла ошибка во время исполнения запроса к серверу:\n" +
                                           $"{e.Message}");
        }
    }

    private async Task AddItemHandler(ISocketMessageChannel channel, SocketUser author, string[] msg)
    {
        if (author.IsNotAdmin(_config))
        {
            await channel.SendMessageAsync("Указанное действие можно выполнить только администратору. " +
                                           "Если вы владелец бота, то перейдите к его настройкам.");
            return;
        }

        try
        {
            var id = await _apiClient.ItemsPOSTAsync(new ItemInputModel()
            {
                Name = msg[1],
                Description = msg[2]
            });

            await channel.SendMessageAsync($"Предмет с названием **{msg[1]}** добавлен, его ID - \"{id:N}\".");
        }
        catch (Exception e)
        {
            await channel.SendMessageAsync("Произошла ошибка во время исполнения запроса к серверу:\n" +
                                           $"{e.Message}");
        }
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
                      + (item.Components.Any()
                          ? item.Components.Aggregate("Состоит из:\n", (s, model) => s + $"{model.Name}, ",
                              s => s[..^2]) + "\n"
                          : string.Empty)
                      + (item.NeedForParents.Any()
                          ? item.NeedForParents.Aggregate("Необходим для:\n", (s, model) => s + $"{model.Name}, ",
                              s => s[..^2])
                          : string.Empty);
        await messageChannel.SendMessageAsync(message);
    }

    private async Task FindByNameHandler(ISocketMessageChannel messageChannel, string[] messageContent)
    {
        var items = await _apiClient.PageAsync();
        var models = items.Where(f => f.Name.Contains(messageContent[1], StringComparison.OrdinalIgnoreCase));
        await messageChannel.SendMessageAsync(models.Aggregate(string.Empty,
            (s, model) => s += $"{model.Name} = {model.Id:N}\n"));
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