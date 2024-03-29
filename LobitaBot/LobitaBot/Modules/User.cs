﻿using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class User : ModuleBase<SocketCommandContext>
    {
        [Command("avatar")]
        [Summary("Displays a user's avatar, or your own if a user @-mention is not provided.")]
        public async Task AvatarAsync(string userID = null)
        {
            var users = Context.Message.MentionedUsers;
            EmbedBuilder embed = null;

            if (!string.IsNullOrEmpty(userID))
            {
                if (users.Count() == 1)
                {
                    var user = users.ElementAt(0);

                    embed = new EmbedBuilder()
                    {
                        Title = $"{user.Username}'s avatar",
                        ImageUrl = user.GetAvatarUrl()
                    };
                }
                else
                {
                    await ReplyAsync($"Avatar can not be displayed for user {userID}.");

                    return;
                }
            }
            else
            {
                embed = new EmbedBuilder()
                {
                    Title = $"{Context.User.Username}'s avatar",
                    ImageUrl = Context.User.GetAvatarUrl()
                };
            }

            embed.WithAuthor(Context.Client.CurrentUser)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }
    }
}
