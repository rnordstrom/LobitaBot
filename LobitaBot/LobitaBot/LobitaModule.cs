using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class LobitaModule : ModuleBase<SocketCommandContext>
    {
        private string dataDirectory = Path.Join(Directory.GetCurrentDirectory(), "data");
        private string[] cmdHandles = new string[] 
        {
            "lysithea",
            "holo",
            "fenrir",
            "myuri",
            "ryouko",
            "nagatoro",
            "velvet"
        };

        private string ChooseImgPath(string cmdHandle)
        {
            DirectoryInfo di = new DirectoryInfo(Path.Join(dataDirectory, cmdHandle));
            FileInfo[] fileInfos = di.GetFiles();
            Random rand = new Random();
            int chosen = rand.Next(0, fileInfos.Length - 1);

            return Path.Join(di.FullName, fileInfos[chosen].Name);
        }

        private string ChooseImgPath()
        {
            Random rand = new Random();
            int chosenDir = rand.Next(0, cmdHandles.Length - 1);

            return ChooseImgPath(cmdHandles[chosenDir]);
        }

        [Command("help")]
        [Summary("Display the list of commands.")]
        public async Task HelpAsync()
        {
            string help = "---Commands---\n" +
                "- oka.lysithea\n" +
                "- oka.holo\n" +
                "- oka.fenrir\n" +
                "- oka.myuri\n" +
                "- oka.ryouko\n" +
                "- oka.nagatoro\n" +
                "- oka.velvet\n" +
                "- oka.mita";

            await ReplyAsync(help);
        }

        [Command("lysithea")]
        [Summary("Displays a random image of Lysithea.")]
        public async Task LysitheaAsync()
        {
            string path = ChooseImgPath(cmdHandles[0]);

            await Context.Channel.SendFileAsync(path);
        }

        [Command("holo")]
        [Summary("Displays a random image of Holo.")]
        public async Task HoloAsync()
        {
            string path = ChooseImgPath(cmdHandles[1]);

            await Context.Channel.SendFileAsync(path);
        }

        [Command("fenrir")]
        [Summary("Displays a random image of Fenrir.")]
        public async Task FenrirAsync()
        {
            string path = ChooseImgPath(cmdHandles[2]);

            await Context.Channel.SendFileAsync(path);
        }

        [Command("myuri")]
        [Summary("Displays a random image of Myuri.")]
        public async Task MyuriAsync()
        {
            string path = ChooseImgPath(cmdHandles[3]);

            await Context.Channel.SendFileAsync(path);
        }

        [Command("ryouko")]
        [Summary("Displays a random image of Ryouko.")]
        public async Task RyoukoAsync()
        {
            string path = ChooseImgPath(cmdHandles[4]);

            await Context.Channel.SendFileAsync(path);
        }

        [Command("nagatoro")]
        [Summary("Displays a random image of Nagatoro.")]
        public async Task NagatoroAsync()
        {
            string path = ChooseImgPath(cmdHandles[5]);

            await Context.Channel.SendFileAsync(path);
        }

        [Command("velvet")]
        [Summary("Displays a random image of Velvet.")]
        public async Task VelvetAsync()
        {
            string path = ChooseImgPath(cmdHandles[6]);

            await Context.Channel.SendFileAsync(path);
        }

        [Command("mita")]
        [Summary("Displays a random image of any character.")]
        public async Task MitaAsync()
        {
            string path = ChooseImgPath();

            await Context.Channel.SendFileAsync(path);
        }
    }
}
