using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LobitaBot.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class Constants
    {
        public const string Prefix = "oka.";
        public static Emoji RerollRandom = new Emoji("🔄");
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
            var generator = new RandomPostGenerator(); 
            client.ReactionAdded += generator.ReactionAdded_Event;

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
