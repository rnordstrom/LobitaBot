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

        public Help(VideoService vs, CommandService cs)
        {
            _commandService = cs;
        }

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

                        switch (cmd.Name)
                        {
                            case "character":
                                field.WithName($"{Constants.Prefix}{cmd.Name} character_name");
                                field.WithValue("Rolls a random image of **character_name**." +
                                    Environment.NewLine +
                                    "Lists all alternatives if a conclusive match is not found.");
                                break;
                            case "series":
                                field.WithName($"{Constants.Prefix}{cmd.Name} series_name");
                                field.WithValue("Rolls a random image from series **series_name**." +
                                    Environment.NewLine +
                                    "Lists all alternatives if a conclusive match is not found.");
                                break;
                            case "with_character":
                                field.WithName($"{Constants.Prefix}{cmd.Name} character_name");
                                field.WithValue("Lists series with character **character_name**.");
                                break;
                            case "in_series":
                                field.WithName($"{Constants.Prefix}{cmd.Name} series_name");
                                field.WithValue("Lists characters from series **series_name**.");
                                break;
                            case "random":
                                field.WithName($"{Constants.Prefix}{cmd.Name}");
                                field.WithValue("Rolls a random character image.");
                                break;
                            case "avatar":
                                field.WithName($"{Constants.Prefix}{cmd.Name} [user_mention]");
                                field.WithValue("Displays a user's avatar, or your own if a user @-mention is not provided.");
                                break;
                            case "help":
                                continue;
                        }

                        fields.Add(field);
                    }
                }

            }

            builder.WithFields(fields);

            await ReplyAsync(embed: builder.Build());
        }
    }
}
