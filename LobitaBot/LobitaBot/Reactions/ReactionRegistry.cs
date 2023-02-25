using Discord;
using Discord.WebSocket;
using LobitaBot.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace LobitaBot.Reactions
{
    public static class ReactionRegistry
    {
        public static async Task ReactionAdded_Event(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var msg = message.GetOrDownloadAsync().Result;
            IEmbed msgEmbed = msg.Embeds.First();
            var searchTerm = msgEmbed.Title;

            if (reaction.UserId == msg.Author.Id)
            {
                return;
            }

            if (reaction.Emote.Name == Constants.RerollRandom.Name)
            {
                var generator = new RandomPostGenerator();
                var url = $"{Constants.PostsUrlBase} {searchTerm}";
                var embed = generator.RandomPost(searchTerm, url);

                if (embed != null)
                {
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(Constants.RerollRandom);
                    }
                }
            }
        }
    }
}
