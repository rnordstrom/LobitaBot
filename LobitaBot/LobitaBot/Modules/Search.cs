using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class TagData
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
        private PageService _pageService;
        private const string SuggestionTitle = "<Tag ID> Tag Name (#Posts)";
        private string SuggestionDescription = $"React with {Constants.sortAlphabetical} to sort alphabetically, " +
                    $"{Constants.sortNumerical} to sort by number of posts, and " +
                    $"{Constants.changeOrder} to switch between ascending/descending order.";
        private const int MaxResults = 1000;
        private const bool IsInline = false;
        List<List<TagData>> pages;

        public Search(PageService ps)
        {
            _pageService = ps;
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
                    await toSend.AddReactionAsync(Constants.rerollCharacter);
                    await toSend.AddReactionAsync(Constants.rerollSeries);
                }
                else
                {
                    msgId = toSend.Id;
                    pageData = new PageData(pages);

                    _pageService.AddLimited(msgId, pageData);

                    await toSend.AddReactionAsync(Constants.pageBack);
                    await toSend.AddReactionAsync(Constants.pageForward);
                    await toSend.AddReactionAsync(Constants.sortAlphabetical);
                    await toSend.AddReactionAsync(Constants.sortNumerical);
                    await toSend.AddReactionAsync(Constants.changeOrder);
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
                    await toSend.AddReactionAsync(Constants.rerollCharacter);
                    await toSend.AddReactionAsync(Constants.rerollSeries);
                }
                else
                {
                    msgId = toSend.Id;
                    pageData = new PageData(pages);

                    _pageService.AddLimited(msgId, pageData);

                    await toSend.AddReactionAsync(Constants.pageBack);
                    await toSend.AddReactionAsync(Constants.pageForward);
                    await toSend.AddReactionAsync(Constants.sortAlphabetical);
                    await toSend.AddReactionAsync(Constants.sortNumerical);
                    await toSend.AddReactionAsync(Constants.changeOrder);
                }
            }
        }

        [Command("in_series")]
        [Summary("Lists characters that belong to the specified series.")]
        public async Task InSeriesAsync(string seriesName = null)
        {
            if (!_pageService.HandlerAdded)
            {
                Context.Client.ReactionAdded += ReactionAdded_Event;
                _pageService.HandlerAdded = true;
            }

            DbSeriesIndex seriesIndex = new DbSeriesIndex(ConfigUtils.GetCurrentDatabase());
            DbCharacterIndex charIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase());
            EmbedBuilder embed = new EmbedBuilder();
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            int id;
            ulong msgId;
            string tag;
            string seriesNameEscaped;
            List<string> pageContent;

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
                int i = 1;

                pages = TagParser.CompileSuggestions(characterData, EmbedBuilder.MaxFieldCount);
                pageContent = TagParser.ToTagInfoList(pages[0]);

                foreach (string s in pageContent)
                {
                    fields.Add(new EmbedFieldBuilder()
                        .WithName($"{i}.")
                        .WithValue(s)
                        .WithIsInline(IsInline));

                    i++;
                }

                embed.WithTitle(SuggestionTitle)
                    .WithFields(fields)
                    .WithDescription(SuggestionDescription)
                    .WithFooter($"Page 1 of {pages.Count}");

                var toSend = await Context.Channel.SendMessageAsync(embed: embed.Build());

                msgId = toSend.Id;
                PageData pageData = new PageData(pages);

                _pageService.AddLimited(msgId, pageData);

                await toSend.AddReactionAsync(Constants.pageBack);
                await toSend.AddReactionAsync(Constants.pageForward);
                await toSend.AddReactionAsync(Constants.sortAlphabetical);
                await toSend.AddReactionAsync(Constants.sortNumerical);
                await toSend.AddReactionAsync(Constants.changeOrder);
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
            if (!_pageService.HandlerAdded)
            {
                Context.Client.ReactionAdded += ReactionAdded_Event;
                _pageService.HandlerAdded = true;
            }

            DbCharacterIndex charIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase());
            DbSeriesIndex seriesIndex = new DbSeriesIndex(ConfigUtils.GetCurrentDatabase());
            EmbedBuilder embed = new EmbedBuilder();
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            int id;
            ulong msgId;
            string tag;
            string charNameEscaped;
            List<string> pageContent;

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
                int i = 1;

                pages = TagParser.CompileSuggestions(seriesData, EmbedBuilder.MaxFieldCount);
                pageContent = TagParser.ToTagInfoList(pages[0]);

                foreach (string s in pageContent)
                {
                    fields.Add(new EmbedFieldBuilder()
                        .WithName($"{i}.")
                        .WithValue(s)
                        .WithIsInline(IsInline));

                    i++;
                }

                embed.WithTitle(SuggestionTitle)
                    .WithFields(fields)
                    .WithDescription(SuggestionDescription)
                    .WithFooter($"Page 1 of {pages.Count}");

                var toSend = await Context.Channel.SendMessageAsync(embed: embed.Build());

                msgId = toSend.Id;
                PageData pageData = new PageData(pages);

                _pageService.AddLimited(msgId, pageData);

                await toSend.AddReactionAsync(Constants.pageBack);
                await toSend.AddReactionAsync(Constants.pageForward);
                await toSend.AddReactionAsync(Constants.sortAlphabetical);
                await toSend.AddReactionAsync(Constants.sortNumerical);
                await toSend.AddReactionAsync(Constants.changeOrder);
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

                await toSend.AddReactionAsync(Constants.rerollRandom);
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
                if (!_pageService.HandlerAdded)
                {
                    Context.Client.ReactionAdded += ReactionAdded_Event;
                    _pageService.HandlerAdded = true;
                }

                searchTerm = TagParser.Format(searchTerm);

                string tag;
                string title;
                string searchTermEscaped;
                string seriesNameEscaped;
                int id;
                PostData postData;
                List<string> suggestions;
                List<string> tags;
                List<TagData> tagData;
                EmbedBuilder embed = new EmbedBuilder();
                List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
                List<string> pageContent;

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
                    seriesNameEscaped = TagParser.EscapeUnderscore(postData.SeriesName);

                    if (!string.IsNullOrEmpty(postData.Link))
                    {
                        embed.WithTitle(title)
                            .AddField("Character ID", postData.PostId)
                            .AddField("Series Name", seriesNameEscaped)
                            .WithDescription($"React with {Constants.rerollCharacter.Name} to reroll character, " +
                            $"{Constants.rerollSeries.Name} to reroll from the same series.")
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
                        pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
                        pageContent = TagParser.ToTagInfoList(pages[0]);
                        int i = 1;

                        foreach (string s in pageContent)
                        {
                            fields.Add(new EmbedFieldBuilder()
                                .WithName($"{i}.")
                                .WithValue(s)
                                .WithIsInline(IsInline));

                            i++;
                        }

                        if (pages.Count > 0)
                        {
                            embed.WithTitle(SuggestionTitle)
                            .WithFields(fields)
                            .WithDescription(SuggestionDescription)
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

                return embed;
            }
        }

        private async Task<EmbedBuilder> RandomPost()
        {
            if (!_pageService.HandlerAdded)
            {
                Context.Client.ReactionAdded += ReactionAdded_Event;
                _pageService.HandlerAdded = true;
            }

            DbCharacterIndex characterIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase());
            EmbedBuilder embed = new EmbedBuilder();
            PostData postData;
            string tag = characterIndex.LookupRandomTag();
            string title;
            string seriesNameEscaped;

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

                seriesNameEscaped = TagParser.EscapeUnderscore(postData.SeriesName);

                embed.WithTitle(title)
                    .AddField("Character ID", postData.PostId)
                    .AddField("Series Name", seriesNameEscaped)
                    .WithDescription($"React with {Constants.rerollRandom} to reroll a random character.")
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
            string seriesName;
            EmbedBuilder embedBuilder;
            Embed embed;
            PageData pageData;
            bool success;

            if (reaction.Emote.Name == Constants.rerollCharacter.Name)
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
                        await toSend.AddReactionAsync(Constants.rerollCharacter);
                        await toSend.AddReactionAsync(Constants.rerollSeries);
                    }
                }
            }
            else if (reaction.Emote.Name == Constants.rerollSeries.Name)
            {
                seriesName = TagParser.Format(embedFields[1].Value);
                embedBuilder = SearchAsync(seriesName, new DbSeriesIndex(ConfigUtils.GetCurrentDatabase())).Result;

                if (embedBuilder != null)
                {
                    embedBuilder.WithFooter(Constants.FooterText + msg.Author);

                    embed = embedBuilder.Build();
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(Constants.rerollCharacter);
                        await toSend.AddReactionAsync(Constants.rerollSeries);
                    }
                }
            }
            else if (reaction.Emote.Name == Constants.rerollRandom.Name)
            {
                embedBuilder = RandomPost().Result;


                if (embedBuilder != null)
                {
                    embedBuilder.WithFooter(Constants.FooterText + msg.Author);

                    embed = embedBuilder.Build();
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(Constants.rerollRandom);
                    }
                }
            }
            else if (reaction.Emote.Name == Constants.pageBack.Name)
            {
                success = _pageService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success && pageData.PageNum > 0)
                {
                    _pageService.PageIndex[msg.Id].PageNum--;

                    EmbedBuilder builder = UpdatePage(pageData, embedTitle);

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
            else if (reaction.Emote.Name == Constants.pageForward.Name)
            {
                success = _pageService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success && pageData.PageNum < pageData.Pages.Count - 1)
                {
                    _pageService.PageIndex[msg.Id].PageNum++;

                    EmbedBuilder builder = UpdatePage(pageData, embedTitle);

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
            else if (reaction.Emote.Name == Constants.sortAlphabetical.Name)
            {
                success = _pageService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success)
                {
                    _pageService.SortAlphabeticalAsc(msg.Id);

                    EmbedBuilder builder = UpdatePage(pageData, embedTitle);

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
            else if (reaction.Emote.Name == Constants.sortNumerical.Name)
            {
                success = _pageService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success)
                {
                    _pageService.SortPostNumAsc(msg.Id);

                    EmbedBuilder builder = UpdatePage(pageData, embedTitle);

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
            else if (reaction.Emote.Name == Constants.changeOrder.Name)
            {
                success = _pageService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success)
                {
                    if (pageData.AlphabeticallySorted)
                    {
                        if(pageData.SortedAscending)
                        {
                            _pageService.SortAlphabeticalDesc(msg.Id);
                        }
                        else
                        {
                            _pageService.SortAlphabeticalAsc(msg.Id);
                        }
                    }
                    else if (pageData.NumericallySorted)
                    {
                        if (pageData.SortedAscending)
                        {
                            _pageService.SortPostNumDesc(msg.Id);
                        }
                        else
                        {
                            _pageService.SortPostNumAsc(msg.Id);
                        }
                    }

                    EmbedBuilder builder = UpdatePage(pageData, embedTitle);

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
        }

        private EmbedBuilder UpdatePage(PageData pageData, string embedTitle)
        {
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            List<string> pageContent = TagParser.ToTagInfoList(pageData.Pages[pageData.PageNum]);
            int i = pageData.PageNum * EmbedBuilder.MaxFieldCount + 1;

            foreach (string s in pageContent)
            {
                fields.Add(new EmbedFieldBuilder()
                    .WithName($"{i}.")
                    .WithValue(s)
                    .WithIsInline(IsInline));

                i++;
            }

            return new EmbedBuilder()
                .WithTitle(embedTitle)
                .WithFields(fields)
                .WithDescription(SuggestionDescription)
                .WithFooter($"Page {pageData.PageNum + 1} of {pageData.Pages.Count}");
        }
    }
}
