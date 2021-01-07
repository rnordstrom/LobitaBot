using Discord;
using Discord.Commands;
using System.IO;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class Video : ModuleBase<SocketCommandContext>
    {
        private readonly VideoService _videoService;
        private string videosDirectory = "videos";
        private string titleText = "Click here to play video...";
        private string[] videoHandles = new string[]
        {
            "OP",
            "ED"
        };

        public Video(VideoService vs)
        {
            _videoService = vs;
        }

        [Command("op")]
        [Summary("Displays a random anime opening theme.")]
        public async Task OpAsync()
        {
            string path = BuildRandomVideoLinkFor(videoHandles[0]);

            var embed = new EmbedBuilder()
            {
                Url = path,
                ImageUrl = $"http://{Constants.BaseAddress}/images/OP_img.png" + ParameterUtils.GenerateUniqueParam()
            };

            embed.WithTitle(titleText)
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = Constants.FooterText + Context.User.Username)
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("ed")]
        [Summary("Displays a random anime opening theme.")]
        public async Task EdAsync()
        {
            string path = BuildRandomVideoLinkFor(videoHandles[1]);

            var embed = new EmbedBuilder()
            {
                Url = path,
                ImageUrl = $"http://{Constants.BaseAddress}/images/ED_img.png" + ParameterUtils.GenerateUniqueParam()
            };

            embed.WithTitle(titleText)
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = Constants.FooterText + Context.User.Username)
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        private string BuildRandomVideoLinkFor(string videoHandle)
        {
            DirectoryInfo di = new DirectoryInfo(Path.Join(Constants.WorkingDirectory, videosDirectory, videoHandle));
            FileInfo[] files = di.GetFiles();

            if (_videoService.RollIndex < 0)
            {
                _videoService.RollIndex = files.Length - 1;
            }

            return $"http://{Constants.BaseAddress}/{videosDirectory}/{videoHandle}/{files[_videoService.RollIndex--].Name}" + ParameterUtils.GenerateUniqueParam();
        }
    }
}
