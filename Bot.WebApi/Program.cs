using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PaxDeiBot.Data;

namespace PaxDeiBot;

public class Program
{
    public static Task Main(string[] args) => 
        WebApplication
            .CreateBuilder(args)
            .SetupBuilder()
            .Build()
            .SetupApplication()
            .InitializeMigrations()
            .RunAsync();
}