using Discord.Commands;
using LobitaBot.Utils;
using System.Threading.Tasks;

namespace LobitaBot.Modules
{
    public class OpenAI : ModuleBase<SocketCommandContext>
    {
        [Command("talk")]
        [Summary("Talk to Lobita!")]
        public async Task RandomAsync([Remainder] string prompt)
        {
            await Context.Channel.SendMessageAsync(ProcessExecutor.ExecuteProcess(Literals.GptPath, $"--prompt \"{prompt}\""));
        }
    }
}
