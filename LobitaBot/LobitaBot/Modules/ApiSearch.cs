﻿using Discord;
using Discord.Commands;
using LobitaBot.Services;
using LobitaBot.Utils;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LobitaBot.Modules
{
    public class ApiSearch : ModuleBase<SocketCommandContext>
    {
        [Command("random")]
        [Summary("Rolls a random image (safe).")]
        public async Task RandomAsync([Remainder] string tags = null)
        {
            var url = $"{Constants.PostsUrlBase} {tags}";
            var generator = new RandomPostGenerator();
            var embed = generator.RandomPost(tags, url);

            if (embed == null)
            {
                await Context.Channel.SendMessageAsync("No results found.");
            }

            var toSend = await Context.Channel.SendMessageAsync(embed: embed);
            await toSend.AddReactionAsync(Constants.RerollRandom);
        }

        [Command("tag")]
        [Summary("Searches for a qualified tag using your input.")]
        public async Task TagAsync(string input)
        {
            var url = $"https://danbooru.donmai.us/tags.xml?search[name_matches]=*{input}*";
            var result = HttpXmlService.GetRequestXml(url);

            if (result == null)
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            XmlElement root = doc.DocumentElement;
            XmlNodeList tags = root.SelectNodes("tag");
            StringBuilder stringBuilder = new StringBuilder();
            int i = 1;

            foreach (XmlNode tag in tags)
            {
                string name = tag.SelectSingleNode("name").InnerText;
                string postCount = tag.SelectSingleNode("post-count").InnerText;
                stringBuilder.Append($"{i++}. {Format.Sanitize(name)} ({postCount})\n");

                if (i > 20)
                {
                    break;
                }
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Matching Tags")
                .WithDescription(stringBuilder.ToString())
                .WithColor(Color.DarkerGrey)
                .WithCurrentTimestamp();

            await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}
