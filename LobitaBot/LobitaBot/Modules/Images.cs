using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LobitaBot.Modules
{
    [DontAutoLoad]
    public class Images : ModuleBase<SocketCommandContext>
    {
        private string imagesDirectory = "images";
        private string[] imageHandles = new string[]
        {
            "lysithea",
            "holo",
            "fenrir",
            "myuri",
            "ryouko",
            "nagatoro",
            "velvet",
            "hololive"
        };

        [Command("lysithea")]
        [Summary("Displays a random image of Lysithea.")]
        public async Task LysitheaAsync()
        {
            Embed e = CreateImageEmbedFor("Lysithea", imageHandles[0]);

            await ReplyAsync(embed: e);
        }

        [Command("holo")]
        [Summary("Displays a random image of Holo.")]
        public async Task HoloAsync()
        {
            Embed e = CreateImageEmbedFor("Holo", imageHandles[1]);

            await ReplyAsync(embed: e);
        }

        [Command("fenrir")]
        [Summary("Displays a random image of Fenrir.")]
        public async Task FenrirAsync()
        {
            Embed e = CreateImageEmbedFor("Fenrir", imageHandles[2]);

            await ReplyAsync(embed: e);
        }

        [Command("myuri")]
        [Summary("Displays a random image of Myuri.")]
        public async Task MyuriAsync()
        {
            Embed e = CreateImageEmbedFor("Myuri", imageHandles[3]);

            await ReplyAsync(embed: e);
        }

        [Command("ryouko")]
        [Summary("Displays a random image of Ryouko.")]
        public async Task RyoukoAsync()
        {
            Embed e = CreateImageEmbedFor("Ryouko", imageHandles[4]);

            await ReplyAsync(embed: e);
        }

        [Command("nagatoro")]
        [Summary("Displays a random image of Nagatoro.")]
        public async Task NagatoroAsync()
        {
            Embed e = CreateImageEmbedFor("Nagatoro", imageHandles[5]);

            await ReplyAsync(embed: e);
        }

        [Command("velvet")]
        [Summary("Displays a random image of Velvet.")]
        public async Task VelvetAsync()
        {
            Embed e = CreateImageEmbedFor("Velvet", imageHandles[6]);

            await ReplyAsync(embed: e);
        }

        [Command("hololive")]
        [Summary("Displays a random Hololive image.")]
        public async Task HololiveAsync()
        {
            Embed e = CreateImageEmbedFor("Hololive", imageHandles[7]);

            await ReplyAsync(embed: e);
        }

        [Command("mita")]
        [Summary("Displays a random image of any character.")]
        public async Task MitaAsync()
        {
            var embed = new EmbedBuilder()
            {
                ImageUrl = BuildRandomImageLink()
            };

            embed.WithTitle("Okamita")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = Constants.FooterText + Context.User.Username)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        private string BuildRandomImageLinkFor(string imageHandle)
        {
            DirectoryInfo di = new DirectoryInfo(Path.Join(Constants.WorkingDirectory, imagesDirectory, imageHandle));
            FileInfo[] files = di.GetFiles();
            Random rand = new Random();
            int chosen = rand.Next(0, files.Length - 1);

            return $"http://{Constants.BaseAddress}/{imagesDirectory}/{imageHandle}/{files[chosen].Name}" + ParameterUtils.GenerateUniqueParam();
        }

        private string BuildRandomImageLink()
        {
            Random rand = new Random();
            int chosenDir = rand.Next(0, imageHandles.Length - 1);

            return BuildRandomImageLinkFor(imageHandles[chosenDir]);
        }

        private Embed CreateImageEmbedFor(string cmdName, string cmdHandle)
        {
            var embed = new EmbedBuilder()
            {
                ImageUrl = BuildRandomImageLinkFor(cmdHandle)
            };

            embed.WithTitle(cmdName)
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = Constants.FooterText + Context.User.Username)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            return embed.Build();
        }
    }
}
