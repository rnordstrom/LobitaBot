using Discord;
using Discord.WebSocket;
using LobitaBot.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace LobitaBot.Utils
{
    class RandomPostGenerator
    {
        public Embed RandomPost(string tags)
        {
            var result = HttpXmlService.GetRequestXml(tags);

            if (result == null)
            {
                return null;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            XmlElement root = doc.DocumentElement;
            string imageUrl = root.SelectSingleNode("file-url").InnerText;
            string description = root.SelectSingleNode("tag-string-general").InnerText;
            string series = root.SelectSingleNode("tag-string-copyright").InnerText;
            var embedBuilder = new EmbedBuilder()
                .WithTitle(tags)
                .WithDescription(description)
                .WithImageUrl(imageUrl)
                .WithUrl(imageUrl)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp()
                .AddField("Series", series);

            return embedBuilder.Build();
        }

        public async Task ReactionAdded_Event(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
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
                var embed = RandomPost(searchTerm);

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
