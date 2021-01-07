using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;

        public Help(VideoService vs, CommandService cs)
        {
            _commandService = cs;
        }

        [Command("help")]
        [Summary("Display a list of available commands.")]
        public async Task HelpAsync()
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("LobitaBot Commands");

            string description;

            foreach (var module in _commandService.Modules)
            {
                description = string.Empty;

                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);

                    if (result.IsSuccess)
                    {
                        description += $"`{Constants.Prefix}{cmd.Name}`\n";
                    }
                }

                if (!string.IsNullOrEmpty(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync(embed: builder.Build());
        }
    }
}
