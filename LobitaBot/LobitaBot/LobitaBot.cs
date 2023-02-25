using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LobitaBot.Reactions;
using System;
using System.Threading.Tasks;
using System.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace LobitaBot
{
    public class Constants
    {
        public const string Prefix = "oka.";
        public static Emoji RerollRandom = new Emoji("🔄");
        public const string PostsUrlBase = "https://danbooru.donmai.us/posts/random.xml?tags=rating:general";
        public const string RandomImageTitle = "Random Image";
        public static string ApiUser = ConfigurationManager.AppSettings.Get("API-USER");
        public static string ApiKey;
    }

    class LobitaBot
    {
        private DiscordSocketClient socketClient;
        private CommandService cmdService;

        public static void Main(string[] args)
            => new LobitaBot().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            socketClient = new DiscordSocketClient();
            cmdService = new CommandService();
            socketClient.Log += Log;
            cmdService.Log += Log;
            socketClient.ReactionAdded += ReactionRegistry.ReactionAdded_Event;

            CommandHandler cmdHandler = new CommandHandler(socketClient, cmdService);
            await cmdHandler.InstallCommandsAsync();

            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = Azure.Core.RetryMode.Exponential
                 }
            };

            var client = new SecretClient(new Uri("https://lobitakeys.vault.azure.net/"), new DefaultAzureCredential(), options);
            KeyVaultSecret tokenSecret = client.GetSecret("token");
            KeyVaultSecret apiSecret = client.GetSecret("API-KEY");
            var token = tokenSecret.Value;
            Constants.ApiKey = apiSecret.Value;

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
