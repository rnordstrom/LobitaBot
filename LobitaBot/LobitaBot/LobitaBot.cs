using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LobitaBot.Reactions;
using System;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class Constants
    {
        public const string Prefix = "oka.";
        public static Emoji RerollRandom = new Emoji("🔄");
        public const string PostsUrlBase = "https://danbooru.donmai.us/posts/random.xml?tags=rating:safe";
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
            client.ReactionAdded += ReactionRegistry.ReactionAdded_Event;

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
