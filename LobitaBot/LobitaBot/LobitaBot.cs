using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace LobitaBot
{
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

            Console.WriteLine("Installing command handler...");

            CommandHandler cmdHandler = new CommandHandler(client, cmdService);
            await cmdHandler.InstallCommandsAsync();

            var token = Environment.GetEnvironmentVariable("token");

            Console.WriteLine("Preparing to log in...");

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            Console.WriteLine("Started successfully!");

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }
    }
}
