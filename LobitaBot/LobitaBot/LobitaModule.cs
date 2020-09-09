using Discord.Commands;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class LobitaModule : ModuleBase<SocketCommandContext>
    {
        [Command("lysithea")]
        [Summary("Displays a random image of Lysithea.")]
        public async Task LysitheaAsync()
        {
            await ReplyAsync("I am Lysithea.");
        }

        [Command("holo")]
        [Summary("Displays a random image of Holo.")]
        public async Task HoloAsync()
        {
            await ReplyAsync("I am Holo.");
        }

        [Command("fenrir")]
        [Summary("Displays a random image of Fenrir.")]
        public async Task FenrirAsync()
        {
            await ReplyAsync("I am Fenrir.");
        }

        [Command("myuri")]
        [Summary("Displays a random image of Myuri.")]
        public async Task MyuriAsync()
        {
            await ReplyAsync("I am Myuri.");
        }

        [Command("ryouko")]
        [Summary("Displays a random image of Ryouko.")]
        public async Task RyoukoAsync()
        {
            await ReplyAsync("I am Ryouko.");
        }

        [Command("nagatoro")]
        [Summary("Displays a random image of Nagatoro.")]
        public async Task NagatoroAsync()
        {
            await ReplyAsync("I am Nagatoro.");
        }

        [Command("velvet")]
        [Summary("Displays a random image of Velvet.")]
        public async Task VelvetAsync()
        {
            await ReplyAsync("I am Velvet.");
        }

        [Command("mita")]
        [Summary("Displays a random image of any character.")]
        public async Task MitaAsync()
        {
            await ReplyAsync("Who am I?");
        }
    }
}
