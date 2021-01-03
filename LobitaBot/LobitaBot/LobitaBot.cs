using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class Constants
    {
        public const string Prefix = "oka.";
        public const string FooterText = "Requested by ";
        public static string BaseAddress = Environment.GetEnvironmentVariable("PUBLIC_IP");
        public static string WorkingDirectory = Directory.GetCurrentDirectory();
    }

    class LobitaBot
    {
        private DiscordSocketClient client;
        private CommandService cmdService;

        public static void Main(string[] args)
            => new LobitaBot().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            cmdService = new CommandService();
            client.Log += Log;
            cmdService.Log += Log;

            CommandHandler cmdHandler = new CommandHandler(client, cmdService);
            await cmdHandler.InstallCommandsAsync();

            var token = Environment.GetEnvironmentVariable("token");

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }
    }
}
