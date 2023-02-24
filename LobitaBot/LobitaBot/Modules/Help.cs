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

        [Command("help")]
        [Summary("Display a list of available commands.")]
        public async Task HelpAsync()
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("LobitaBot Commands")
                .WithDescription("Character and series search commands use the %-symbol as a wildcard. " +
                    Environment.NewLine +
                    "Use the symbol anywhere in a search string to match any string of characters " +
                    "before, after or amidst the search string. Multiple %-symbols may be used." +
                    Environment.NewLine +
                    "Examples: %aber, hol%, me%llis, od%_%ga");
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

                        field.WithName($"{Constants.Prefix}{cmd.Name} {parameters}");
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
