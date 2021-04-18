using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobitaBot
{
    enum ROLL_SEQUENCE
    {
        RANDOM, PREVIOUS, NEXT
    };

    enum CATEGORY
    {
        CHARACTER, SERIES
    };

    public class Search : ModuleBase<SocketCommandContext>
    {
        private PageService _pageService;
        private CacheService _cacheService;
        private const string SuggestionTitle = "<Tag ID> Tag Name (#Posts)";
        private string SuggestionDescription = $"React with {Constants.SortAlphabetical} to sort alphabetically, " +
                    $"{Constants.SortNumerical} to sort by number of posts, and " +
                    $"{Constants.ChangeOrder} to switch between ascending/descending order.";
        private const int MaxSearchResults = 1000;
        private const int MaxSequentialImages = 100;
        private const bool IsInline = false;
        List<List<TagData>> pages;

        public Search(PageService ps, CacheService cs)
        {
            _pageService = ps;
            _cacheService = cs;
        }

        [Command("character")]
        [Summary("Search for random images related to a particular free-text character tag.")]
        public async Task CharacterAsync(string searchTerm = null)
        {
            EmbedBuilder embedBuilder = 
                SearchAsync(searchTerm, CATEGORY.CHARACTER, new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService)).Result;
            ulong msgId;
            PageData pageData;

            if (embedBuilder != null)
            {
                Embed embed = embedBuilder.Build();
                var toSend = await Context.Channel.SendMessageAsync(embed: embed);

                if (embed.Image != null)
                {
                    await toSend.AddReactionAsync(Constants.RerollCharacter);
                    await toSend.AddReactionAsync(Constants.RerollSeries);
                    await toSend.AddReactionAsync(Constants.Characters);

                    if (_cacheService.CacheSize() < MaxSequentialImages)
                    {
                        await toSend.AddReactionAsync(Constants.PreviousImage);
                        await toSend.AddReactionAsync(Constants.NextImage);
                    }
                }
                else
                {
                    msgId = toSend.Id;
                    pageData = new PageData(pages);

                    _pageService.AddLimited(msgId, pageData);

                    await toSend.AddReactionAsync(Constants.PageBack);
                    await toSend.AddReactionAsync(Constants.PageForward);
                    await toSend.AddReactionAsync(Constants.SortAlphabetical);
                    await toSend.AddReactionAsync(Constants.SortNumerical);
                    await toSend.AddReactionAsync(Constants.ChangeOrder);
                }
            }
        }

        [Command("series")]
        [Summary("Search for random images related to a particular free-text series tag.")]
        public async Task SeriesAsync(string searchTerm = null)
        {
            EmbedBuilder embedBuilder 
                = SearchAsync(searchTerm, CATEGORY.SERIES, new DbSeriesIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService)).Result;
            ulong msgId;
            PageData pageData;

            if (embedBuilder != null)
            {
                Embed embed = embedBuilder.Build();
                var toSend = await Context.Channel.SendMessageAsync(embed: embed);

                if (embed.Image != null)
                {
                    await toSend.AddReactionAsync(Constants.RerollCharacter);
                    await toSend.AddReactionAsync(Constants.RerollSeries);
                    await toSend.AddReactionAsync(Constants.Characters);
                }
                else
                {
                    msgId = toSend.Id;
                    pageData = new PageData(pages);

                    _pageService.AddLimited(msgId, pageData);

                    await toSend.AddReactionAsync(Constants.PageBack);
                    await toSend.AddReactionAsync(Constants.PageForward);
                    await toSend.AddReactionAsync(Constants.SortAlphabetical);
                    await toSend.AddReactionAsync(Constants.SortNumerical);
                    await toSend.AddReactionAsync(Constants.ChangeOrder);
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

            DbSeriesIndex seriesIndex = new DbSeriesIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService);
            DbCharacterIndex charIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService);
            int id;
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

                pages = TagParser.CompileSuggestions(characterData, EmbedBuilder.MaxFieldCount);

                EmbedBuilder embed = BuildSuggestionsEmbed(pages);
                var toSend = await Context.Channel.SendMessageAsync(embed: embed.Build());
                ulong msgId = toSend.Id;
                PageData pageData = new PageData(pages);

                _pageService.AddLimited(msgId, pageData);

                await toSend.AddReactionAsync(Constants.PageBack);
                await toSend.AddReactionAsync(Constants.PageForward);
                await toSend.AddReactionAsync(Constants.SortAlphabetical);
                await toSend.AddReactionAsync(Constants.SortNumerical);
                await toSend.AddReactionAsync(Constants.ChangeOrder);
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

            DbCharacterIndex charIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService);
            DbSeriesIndex seriesIndex = new DbSeriesIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService);
            int id;
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

                pages = TagParser.CompileSuggestions(seriesData, EmbedBuilder.MaxFieldCount);
                
                EmbedBuilder embed = BuildSuggestionsEmbed(pages);
                var toSend = await Context.Channel.SendMessageAsync(embed: embed.Build());
                ulong msgId = toSend.Id;
                PageData pageData = new PageData(pages);

                _pageService.AddLimited(msgId, pageData);

                await toSend.AddReactionAsync(Constants.PageBack);
                await toSend.AddReactionAsync(Constants.PageForward);
                await toSend.AddReactionAsync(Constants.SortAlphabetical);
                await toSend.AddReactionAsync(Constants.SortNumerical);
                await toSend.AddReactionAsync(Constants.ChangeOrder);
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

                await toSend.AddReactionAsync(Constants.RerollRandom);
                await toSend.AddReactionAsync(Constants.Characters);
            }
        }

        private async Task<EmbedBuilder> SearchAsync(string searchTerm, CATEGORY category, ITagIndex tagIndex, int postIndex = 0, ROLL_SEQUENCE rollSequence = ROLL_SEQUENCE.RANDOM)
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
                PostData postData = null;
                List<string> suggestions;
                List<string> tags;
                List<TagData> tagData;
                EmbedBuilder embed = new EmbedBuilder();
                List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
                List<string> pageContent;
                string embedDescription = $"React with {Constants.RerollCharacter.Name} to reroll character, " + 
                    $"{Constants.RerollSeries.Name} to reroll from the same series. " + Environment.NewLine +
                    $"Use {Constants.Characters} to list all characters in the image.";

                if (int.TryParse(searchTerm, out id))
                {
                    tag = tagIndex.LookupSingleTag(id);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        searchTerm = tag;
                    }
                }

                searchTermEscaped = TagParser.EscapeUnderscore(searchTerm);

                if (tagIndex.TagExists(searchTerm))
                {
                    switch (rollSequence)
                    {
                        case ROLL_SEQUENCE.RANDOM:
                            postData = tagIndex.LookupRandomPost(searchTerm);
                            break;
                        case ROLL_SEQUENCE.PREVIOUS:
                            postData = tagIndex.LookupPreviousPost(searchTerm, postIndex);
                            break;
                        case ROLL_SEQUENCE.NEXT:
                            postData = tagIndex.LookupNextPost(searchTerm, postIndex);
                            break;
                    }

                    switch (category)
                    {
                        case CATEGORY.CHARACTER:
                            embedDescription += Environment.NewLine +
                                $"View previous image with {Constants.PreviousImage} and next image with {Constants.NextImage}.";
                            break;
                    }

                    if (postData != null && !string.IsNullOrEmpty(postData.Link))
                    {
                        title = TagParser.BuildTitle(postData.TagName);
                        seriesNameEscaped = TagParser.EscapeUnderscore(postData.SeriesName);

                        embed.WithTitle(title)
                            .AddField("Character ID", postData.TagId, true)
                            .AddField("Post ID", postData.LinkId, true)
                            .AddField("Series Name", seriesNameEscaped, true)
                            .WithDescription(embedDescription)
                            .WithImageUrl(postData.Link)
                            .WithUrl(postData.Link)
                            .WithAuthor(Context.Client.CurrentUser)
                            .WithFooter($"Image {postData.PostIndex + 1} of {_cacheService.CacheSize()}")
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
                    tags = tagIndex.LookupTags(searchTerm);

                    if (tags.Count < MaxSearchResults)
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
                        tagData = tagIndex.LookupTagData(suggestions);
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

            DbCharacterIndex characterIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService);
            EmbedBuilder embed = new EmbedBuilder();
            PostData postData;
            string tag = characterIndex.LookupRandomTag();
            string title;
            string seriesNameEscaped;

            if (!string.IsNullOrEmpty(tag))
            {
                title = TagParser.BuildTitle(tag);
                postData = characterIndex.LookupRandomPost(tag);

                while (postData == null)
                {
                    tag = characterIndex.LookupRandomTag();
                    title = TagParser.BuildTitle(tag);
                    postData = characterIndex.LookupRandomPost(tag);
                }

                seriesNameEscaped = TagParser.EscapeUnderscore(postData.SeriesName);

                embed.WithTitle(title)
                    .AddField("Character ID", postData.TagId, true)
                    .AddField("Post ID", postData.LinkId, true)
                    .AddField("Series Name", seriesNameEscaped, true)
                    .WithDescription($"React with {Constants.RerollRandom} to reroll a random character." +
                        Environment.NewLine +
                        $"Use {Constants.Characters} to list all characters in the image.")
                    .WithImageUrl(postData.Link)
                    .WithUrl(postData.Link)
                    .WithAuthor(Context.Client.CurrentUser)
                    .WithFooter($"Image {postData.PostIndex + 1} of {_cacheService.CacheSize()}")
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

        private EmbedBuilder BuildSuggestionsEmbed(List<List<TagData>> pages)
        {
            EmbedBuilder embed = new EmbedBuilder();
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            List<string> pageContent = TagParser.ToTagInfoList(pages[0]);
            int i = 1;

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
            string footerText = msgEmbed.Footer.ToString();
            int imageIndex = 0;
            int numImages = 0;
            string characterId;
            string seriesName;
            EmbedBuilder embedBuilder;
            Embed embed;
            PageData pageData;
            bool success;

            if (reaction.Emote.Name == Constants.RerollCharacter.Name)
            {
                characterId = embedFields[0].Value;
                embedBuilder = SearchAsync(characterId, CATEGORY.CHARACTER, new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService)).Result;

                if (embedBuilder != null)
                {
                    embed = embedBuilder.Build();
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(Constants.RerollCharacter);
                        await toSend.AddReactionAsync(Constants.RerollSeries);
                        await toSend.AddReactionAsync(Constants.Characters);

                        if (_cacheService.CacheSize() < MaxSequentialImages)
                        {
                            await toSend.AddReactionAsync(Constants.PreviousImage);
                            await toSend.AddReactionAsync(Constants.NextImage);
                        }
                    }
                }
            }
            else if (reaction.Emote.Name == Constants.RerollSeries.Name)
            {
                seriesName = TagParser.Format(embedFields[2].Value);
                embedBuilder = SearchAsync(seriesName, CATEGORY.SERIES, 
                    new DbSeriesIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService)).Result;

                if (embedBuilder != null)
                {
                    embed = embedBuilder.Build();
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(Constants.RerollCharacter);
                        await toSend.AddReactionAsync(Constants.RerollSeries);
                        await toSend.AddReactionAsync(Constants.Characters);
                    }
                }
            }
            else if (reaction.Emote.Name == Constants.PreviousImage.Name)
            {
                characterId = embedFields[0].Value;
                footerText.Split(" ").First(i => int.TryParse(i, out imageIndex));
                footerText.Split(" ").Last(i => int.TryParse(i, out numImages));

                if (imageIndex > 1)
                {
                    embedBuilder = SearchAsync(characterId, 
                        CATEGORY.CHARACTER, 
                        new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService), 
                        imageIndex - 1, 
                        ROLL_SEQUENCE.PREVIOUS).Result;

                    if (embedBuilder != null)
                    {
                        embed = embedBuilder.Build();
                        var toSend = await channel.SendMessageAsync(embed: embed);

                        if (embed.Image != null)
                        {
                            await toSend.AddReactionAsync(Constants.RerollCharacter);
                            await toSend.AddReactionAsync(Constants.RerollSeries);
                            await toSend.AddReactionAsync(Constants.Characters);

                            if (_cacheService.CacheSize() < MaxSequentialImages)
                            {
                                await toSend.AddReactionAsync(Constants.PreviousImage);
                                await toSend.AddReactionAsync(Constants.NextImage);
                            }
                        }
                    }
                }
            }
            else if (reaction.Emote.Name == Constants.NextImage.Name)
            {
                characterId = embedFields[0].Value;
                footerText.Split(" ").First(i => int.TryParse(i, out imageIndex));
                footerText.Split(" ").Last(i => int.TryParse(i, out numImages));

                if (imageIndex < numImages)
                {
                    embedBuilder = SearchAsync(characterId, 
                        CATEGORY.CHARACTER, 
                        new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService), 
                        imageIndex - 1, 
                        ROLL_SEQUENCE.NEXT).Result;

                    if (embedBuilder != null)
                    {
                        embed = embedBuilder.Build();
                        var toSend = await channel.SendMessageAsync(embed: embed);

                        if (embed.Image != null)
                        {
                            await toSend.AddReactionAsync(Constants.RerollCharacter);
                            await toSend.AddReactionAsync(Constants.RerollSeries);
                            await toSend.AddReactionAsync(Constants.Characters);

                            if (_cacheService.CacheSize() < MaxSequentialImages)
                            {
                                await toSend.AddReactionAsync(Constants.PreviousImage);
                                await toSend.AddReactionAsync(Constants.NextImage);
                            }
                        }
                    }
                }
            }
            else if (reaction.Emote.Name == Constants.Characters.Name)
            {
                DbCharacterIndex charIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService);
                List<string> charsInPost = charIndex.CharactersInPost(int.Parse(embedFields[1].Value));
                List<TagData> charData = charIndex.LookupTagData(charsInPost);

                pages = TagParser.CompileSuggestions(charData, EmbedBuilder.MaxFieldCount);
                pageData = new PageData(pages);

                embedBuilder = BuildSuggestionsEmbed(pages);
                var toSend = await channel.SendMessageAsync(embed: embedBuilder.Build());
                ulong msgId = toSend.Id;

                _pageService.AddLimited(msgId, pageData);

                await toSend.AddReactionAsync(Constants.PageBack);
                await toSend.AddReactionAsync(Constants.PageForward);
                await toSend.AddReactionAsync(Constants.SortAlphabetical);
                await toSend.AddReactionAsync(Constants.SortNumerical);
                await toSend.AddReactionAsync(Constants.ChangeOrder);

            }
            else if (reaction.Emote.Name == Constants.RerollRandom.Name)
            {
                embedBuilder = RandomPost().Result;

                if (embedBuilder != null)
                {
                    embed = embedBuilder.Build();
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(Constants.RerollRandom);
                        await toSend.AddReactionAsync(Constants.Characters);
                    }
                }
            }
            else if (reaction.Emote.Name == Constants.PageBack.Name)
            {
                success = _pageService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success && pageData.PageNum > 0)
                {
                    _pageService.PageIndex[msg.Id].PageNum--;

                    EmbedBuilder builder = UpdatePage(pageData, embedTitle);

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
            else if (reaction.Emote.Name == Constants.PageForward.Name)
            {
                success = _pageService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success && pageData.PageNum < pageData.Pages.Count - 1)
                {
                    _pageService.PageIndex[msg.Id].PageNum++;

                    EmbedBuilder builder = UpdatePage(pageData, embedTitle);

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
            else if (reaction.Emote.Name == Constants.SortAlphabetical.Name)
            {
                success = _pageService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success)
                {
                    _pageService.SortAlphabeticalAsc(msg.Id);

                    EmbedBuilder builder = UpdatePage(pageData, embedTitle);

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
            else if (reaction.Emote.Name == Constants.SortNumerical.Name)
            {
                success = _pageService.PageIndex.TryGetValue(msg.Id, out pageData);

                if (success)
                {
                    _pageService.SortPostNumAsc(msg.Id);

                    EmbedBuilder builder = UpdatePage(pageData, embedTitle);

                    await msg.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
            else if (reaction.Emote.Name == Constants.ChangeOrder.Name)
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
