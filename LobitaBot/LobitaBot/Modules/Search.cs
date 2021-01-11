using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobitaBot
{
    public struct TagData
    {
        public TagData(string tagName, int tagID, long numLinks)
        {
            TagName = tagName;
            TagID = tagID;
            NumLinks = numLinks;
        }

        public string TagName { get; }
        public int TagID { get; }
        public long NumLinks { get; }
    };

    public struct PostData
    {
        public PostData(int postId, string tagName, string link, string seriesName)
        {
            PostId = postId;
            TagName = tagName;
            Link = link;
            SeriesName = seriesName;
        }

        public int PostId { get; }
        public string TagName { get; }
        public string SeriesName { get; }
        public string Link { get; }
    };

    public class Search : ModuleBase<SocketCommandContext>
    {
        private SearchService _searchService;
        private Emoji rerollSeries = new Emoji("🔁");
        private Emoji rerollCharacter = new Emoji("🔂");
        private Emoji rerollRandom = new Emoji("🔄");
        private Emoji pageBack = new Emoji("⏪");
        private Emoji pageForward = new Emoji("⏩");
        private const string SuggestionTitle = "<Tag ID> Tag Name (#Posts). Showing top results.";
        private const int MaxResults = 1000;
        List<string> pages;

        public Search(SearchService ss)
        {
            _searchService = ss;
        }

        [Command("character")]
        [Summary("Search for random images related to a particular free-text character tag.")]
        public async Task CharacterAsync(string searchTerm = null)
        {
            EmbedBuilder embedBuilder = 
                SearchAsync(searchTerm, new DbCharacterIndex(ConfigUtils.GetCurrentDatabase())).Result;
            ulong msgId;
            PageData pageData;

            if (embedBuilder != null)
            {
                Embed embed = embedBuilder.Build();
                var toSend = await Context.Channel.SendMessageAsync(embed: embed);

                if (embed.Image != null)
                {
                    await toSend.AddReactionAsync(rerollCharacter);
                    await toSend.AddReactionAsync(rerollSeries);
                }
                else
                {
                    msgId = toSend.Id;
                    pageData = new PageData(pages);

                    _searchService.AddLimited(msgId, pageData);

                    await toSend.AddReactionAsync(pageBack);
                    await toSend.AddReactionAsync(pageForward);
                }
            }
        }

        [Command("series")]
        [Summary("Search for random images related to a particular free-text series tag.")]
        public async Task SeriesAsync(string searchTerm = null)
        {
            EmbedBuilder embedBuilder 
                = SearchAsync(searchTerm, new DbSeriesIndex(ConfigUtils.GetCurrentDatabase())).Result;
            ulong msgId;
            PageData pageData;

            if (embedBuilder != null)
            {
                Embed embed = embedBuilder.Build();
                var toSend = await Context.Channel.SendMessageAsync(embed: embed);

                if (embed.Image != null)
                {
                    await toSend.AddReactionAsync(rerollCharacter);
                    await toSend.AddReactionAsync(rerollSeries);
                }
                else
                {
                    msgId = toSend.Id;
                    pageData = new PageData(pages);

                    _searchService.AddLimited(msgId, pageData);

                    await toSend.AddReactionAsync(pageBack);
                    await toSend.AddReactionAsync(pageForward);
                }
            }
        }

        [Command("in_series")]
        [Summary("Lists characters that belong to the specified series.")]
        public async Task InSeriesAsync(string seriesName = null)
        {
            DbSeriesIndex seriesIndex = new DbSeriesIndex(ConfigUtils.GetCurrentDatabase());
            DbCharacterIndex charIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase());
            EmbedBuilder embed = new EmbedBuilder();
            int id;
            ulong msgId;
            string tag;
            string seriesNameEscaped;

            if (int.TryParse(seriesName, out id))
            {
                tag = seriesIndex.LookupSingleTag(id);

                if (!string.IsNullOrEmpty(tag))
                {
                    seriesName = tag;
                }
            }

            seriesName = TagParser.Format(seriesName);
            seriesNameEscaped = TagParser.EscapeUnderscore(seriesName);

            if (seriesIndex.TagExists(seriesName))
            {
                List<string> characters = seriesIndex.CharactersInSeries(seriesName);
                List<TagData> characterData = charIndex.LookupTagData(characters);

                pages = TagParser.CompileSuggestions(characterData);

                embed.WithTitle(SuggestionTitle)
                    .WithDescription(pages[0])
                    .WithFooter($"Page 1 of {pages.Count}");

                var toSend = await Context.Channel.SendMessageAsync(embed: embed.Build());

                msgId = toSend.Id;
                PageData pageData = new PageData(pages);

                _searchService.AddLimited(msgId, pageData);

                await toSend.AddReactionAsync(pageBack);
                await toSend.AddReactionAsync(pageForward);
            }
            else
            {
                await ReplyAsync($"No results found for '{seriesNameEscaped}'.");
            }
        }

        [Command("with_character")]
        [Summary("Lists series that feature the specified character.")]
        public async Task WithCharacterAsync(string charName = null)
        {
            DbCharacterIndex charIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase());
            DbSeriesIndex seriesIndex = new DbSeriesIndex(ConfigUtils.GetCurrentDatabase());
            EmbedBuilder embed = new EmbedBuilder();
            int id;
            ulong msgId;
            string tag;
            string charNameEscaped;

            if (int.TryParse(charName, out id))
            {
                tag = charIndex.LookupSingleTag(id);

                if (!string.IsNullOrEmpty(tag))
                {
                    charName = tag;
                }
            }

            charName = TagParser.Format(charName);
            charNameEscaped = TagParser.EscapeUnderscore(charName);

            if (charIndex.TagExists(charName))
            {
                List<string> series = charIndex.SeriesWithCharacter(charName);
                List<TagData> seriesData = seriesIndex.LookupTagData(series);

                pages = TagParser.CompileSuggestions(seriesData);

                embed.WithTitle(SuggestionTitle)
                    .WithDescription(pages[0])
                    .WithFooter($"Page 1 of {pages.Count}.");

                var toSend = await Context.Channel.SendMessageAsync(embed: embed.Build());

                msgId = toSend.Id;
                PageData pageData = new PageData(pages);

                _searchService.AddLimited(msgId, pageData);

                await toSend.AddReactionAsync(pageBack);
                await toSend.AddReactionAsync(pageForward);
            }
            else
            {
                await ReplyAsync($"No results found for '{charNameEscaped}'");
            }
        }

        [Command("random")]
        [Summary("Search for random images belonging to a random tag.")]
        public async Task RandomAsync()
        {
            EmbedBuilder embedBuilder = RandomPost().Result;

            if (embedBuilder != null)
            {
                var toSend = await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());

                await toSend.AddReactionAsync(rerollRandom);
            }
        }

        private async Task<EmbedBuilder> SearchAsync(string searchTerm, ITagIndex index)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                await ReplyAsync("Usage: oka.search search term");

                return null;
            }
            else
            {
                if (!_searchService.HandlerAdded)
                {
                    Context.Client.ReactionAdded += ReactionAdded_Event;
                    _searchService.HandlerAdded = true;
                }

                searchTerm = TagParser.Format(searchTerm);

                string tag;
                string title;
                string searchTermEscaped;
                int id;
                PostData postData;
                List<string> suggestions;
                List<string> tags;
                List<TagData> tagData;
                EmbedBuilder embedBuilder = new EmbedBuilder();

                if (int.TryParse(searchTerm, out id))
                {
                    tag = index.LookupSingleTag(id);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        searchTerm = tag;
                    }
                }

                searchTermEscaped = TagParser.EscapeUnderscore(searchTerm);

                if (index.TagExists(searchTerm))
                {
                    postData = index.LookupRandomPost(searchTerm);
                    title = TagParser.BuildTitle(postData.TagName);

                    if (!string.IsNullOrEmpty(postData.Link))
                    {
                        embedBuilder.WithTitle(title)
                            .AddField("Character ID", postData.PostId)
                            .AddField("Series Name", postData.SeriesName)
                            .WithDescription($"React with {rerollCharacter.Name} to reroll character, {rerollSeries.Name} to reroll from the same series.")
                            .WithImageUrl(postData.Link)
                            .WithUrl(postData.Link)
                            .WithAuthor(Context.Client.CurrentUser)
                            .WithFooter(footer => footer.Text = Constants.FooterText + Context.User)
                            .WithColor(Color.DarkGrey)
                            .WithCurrentTimestamp();
                    }
                    else
                    {
                        await ReplyAsync($"No images found for '{searchTermEscaped}'.");

                        return null;
                    }
                }
                else
                {
                    tags = index.LookupTags(searchTerm);

                    if (tags.Count < MaxResults)
                    {
                        suggestions = TagParser.FilterSuggestions(tags, searchTerm);
                    }
                    else
                    {
                        await ReplyAsync($"Your search for '{searchTerm}' is too broad! Please be more specific.");

                        return null;
                    }

                    if (suggestions.Count > 0)
                    {
                        tagData = index.LookupTagData(suggestions);
                        pages = TagParser.CompileSuggestions(tagData);

                        if (pages.Count > 0)
                        {
                            embedBuilder.WithTitle(SuggestionTitle)
                                .WithDescription(pages[0])
                                .WithFooter($"Page 1 of {pages.Count}");
                        }
                        else
                        {
                            await ReplyAsync($"No suggestions exist for search term '{searchTermEscaped}'.");

                            return null;
                        }
                    }
                    else
                    {
                        await ReplyAsync($"No results found for '{searchTermEscaped}'.");

                        return null;
                    }
                }

                return embedBuilder;
            }
        }

        private async Task<EmbedBuilder> RandomPost()
        {
            if (!_searchService.HandlerAdded)
            {
                Context.Client.ReactionAdded += ReactionAdded_Event;
                _searchService.HandlerAdded = true;
            }

            DbCharacterIndex characterIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase());
            EmbedBuilder embed = new EmbedBuilder();
            PostData postData;
            string tag = characterIndex.LookupRandomTag();
            string title;

            if (!string.IsNullOrEmpty(tag))
            {
                title = TagParser.BuildTitle(tag);
                postData = characterIndex.LookupRandomPost(tag);

                while (string.IsNullOrEmpty(postData.Link))
                {
                    tag = characterIndex.LookupRandomTag();
                    title = TagParser.BuildTitle(tag);
                    postData = characterIndex.LookupRandomPost(tag);
                }

                embed.WithTitle(title)
                    .AddField("Character ID", postData.PostId)
                    .AddField("Series Name", postData.SeriesName)
                    .WithDescription($"React with {rerollRandom.Name} to reroll a random character.")
                    .WithImageUrl(postData.Link)
                    .WithUrl(postData.Link)
                    .WithAuthor(Context.Client.CurrentUser)
                    .WithFooter(footer => footer.Text = Constants.FooterText + Context.User)
                    .WithColor(Color.DarkGrey)
                    .WithCurrentTimestamp();
            }
            else
            {
                await ReplyAsync("An error occurred during random tag lookup.");

                return null;
            }

            return embed;
        }

        public async Task ReactionAdded_Event(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var msg = message.GetOrDownloadAsync().Result;

            if (reaction.UserId == msg.Author.Id)
            {
                return;
            }

            IEmbed msgEmbed = msg.Embeds.First();
            var embedFields = msgEmbed.Fields;
            string embedTitle = msgEmbed.Title;
            string characterId;
            string seriesId;
            EmbedBuilder builder;
            EmbedBuilder embedBuilder;
            Embed embed;
            PageData pageData;
            bool success;

            if (reaction.Emote.Name == rerollCharacter.Name)
            {
                characterId = embedFields[0].Value;
                embedBuilder = SearchAsync(characterId, new DbCharacterIndex(ConfigUtils.GetCurrentDatabase())).Result;

                if (embedBuilder != null)
                {
                    embedBuilder.WithFooter(Constants.FooterText + msg.Author);

                    embed = embedBuilder.Build();
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(rerollCharacter);
                        await toSend.AddReactionAsync(rerollSeries);
                    }
                }
            }
            else if (reaction.Emote.Name == rerollSeries.Name)
            {
                seriesId = embedFields[1].Value;
                embedBuilder = SearchAsync(seriesId, new DbSeriesIndex(ConfigUtils.GetCurrentDatabase())).Result;

                if (embedBuilder != null)
                {
                    embedBuilder.WithFooter(Constants.FooterText + msg.Author);

                    embed = embedBuilder.Build();
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(rerollCharacter);
                        await toSend.AddReactionAsync(rerollSeries);
                    }
                }
            }
            else if (reaction.Emote.Name == rerollRandom.Name)
            {
                embedBuilder = RandomPost().Result;


                if (embedBuilder != null)
                {
                    embedBuilder.WithFooter(Constants.FooterText + msg.Author);

                    embed = embedBuilder.Build();
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(rerollRandom);
                    }
                }
            }
            else if (reaction.Emote.Name == pageBack.Name)
            {
                success = _searchService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success && pageData.PageNum > 0)
                {
                    _searchService.PageIndex[msg.Id].PageNum--;

                    builder = new EmbedBuilder()
                        .WithTitle(embedTitle)
                        .WithDescription(pageData.Pages[pageData.PageNum])
                        .WithFooter($"Page {pageData.PageNum + 1} of {pageData.Pages.Count}");

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
            else if (reaction.Emote.Name == pageForward.Name)
            {
                success = _searchService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success && pageData.PageNum < pageData.Pages.Count - 1)
                {
                    _searchService.PageIndex[msg.Id].PageNum++;

                    builder = new EmbedBuilder()
                        .WithTitle(embedTitle)
                        .WithDescription(pageData.Pages[pageData.PageNum])
                        .WithFooter($"Page {pageData.PageNum + 1} of {pageData.Pages.Count}");

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
        }
    }
}
