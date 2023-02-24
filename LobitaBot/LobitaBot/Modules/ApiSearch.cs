using Discord.Commands;
using LobitaBot.Utils;
using System.Threading.Tasks;

namespace LobitaBot.Modules
{
    public class ApiSearch : ModuleBase<SocketCommandContext>
    {
        [Command("random")]
        [Summary("Rolls a random image (safe).")]
        public async Task RandomAsync([Remainder] string tags)
        {
            var generator = new RandomPostGenerator();
            var embed = generator.RandomPost(tags);

            if (embed == null)
            {
                await Context.Channel.SendMessageAsync("No results found.");
            }

            var toSend = await Context.Channel.SendMessageAsync(embed: embed);
            await toSend.AddReactionAsync(Constants.RerollRandom);
        }
    }
}
