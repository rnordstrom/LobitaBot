using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;

        public Help(CommandService cs)
        {
            _commandService = cs;
        }

        [Command("help")]
        [Summary("Display a list of available commands.")]
        public async Task HelpAsync()
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("LobitaBot Commands");
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();

            foreach (var module in _commandService.Modules)
            {
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);

                    if (result.IsSuccess)
                    {
                        EmbedFieldBuilder field = new EmbedFieldBuilder();
                        string parameters = string.Empty;

                        foreach (ParameterInfo pi in cmd.Parameters)
                        {
                            parameters += $"{pi.Name} ";
                        }

                        field.WithName($"{Literals.Prefix}{cmd.Name} {parameters}");
                        field.WithValue(cmd.Summary);
                        fields.Add(field);
                    }
                }

            }

            builder.WithFields(fields);

            await ReplyAsync(embed: builder.Build());
        }
    }
}
