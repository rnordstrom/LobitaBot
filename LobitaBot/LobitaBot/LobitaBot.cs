using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LobitaBot.Reactions;
using System;
using System.Threading.Tasks;
using System.Configuration;
using LobitaBot.Services;

namespace LobitaBot
{
    public class Literals
    {
        public const string Prefix = "oka.";
        public static Emoji RerollRandom = new Emoji("🔄");
        public const string UrlBase = "https://danbooru.donmai.us/";
        public const string PostsBase = "posts/random.xml?tags=-rating:explicit -rating:mature -rating:questionable";
        public const string RandomImageTitle = "Random Image";
        public const string NotAvailable = "n/a";
        public static string ApiUser = ConfigurationManager.AppSettings.Get("API-USER");
        public static string ApiKey;
        public static string GptPath = ConfigurationManager.AppSettings.Get("GPT-PATH");
    }

    class LobitaBot
    {
        private DiscordSocketClient socketClient;
        private CommandService cmdService;

        public static void Main(string[] args)
            => new LobitaBot().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var token = Environment.GetEnvironmentVariable("token");
            Literals.ApiKey = Environment.GetEnvironmentVariable("API_KEY");

            HttpXmlService.Initialize();

            socketClient = new DiscordSocketClient();
            cmdService = new CommandService();
            socketClient.Log += Log;
            cmdService.Log += Log;
            socketClient.ReactionAdded += ReactionRegistry.ReactionAdded_Event;

            CommandHandler cmdHandler = new CommandHandler(socketClient, cmdService);
            await cmdHandler.InstallCommandsAsync();
            await socketClient.LoginAsync(TokenType.Bot, token);
            await socketClient.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }
    }
}
