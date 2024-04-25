using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace PaxDeiBotApp;

public static class DiscordUserExtensions
{
    public static bool IsAdmin(this SocketUser user, IConfiguration cfg)
    {
        var admins = JsonConvert.DeserializeObject<List<string>>(cfg["Admins"]);
        return admins.Any(f => f == user.Username);
    }

    public static bool IsNotAdmin(this SocketUser user, IConfiguration cfg) => !user.IsAdmin(cfg);
}