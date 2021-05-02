using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private const string ExcessiveResults = "Your search for '%' is too broad! Please be more specific.";
        private const string NoResults = "No results found for '%'.";
        private const string NoImages = "No images found for '%'.";
        private string suggestionDescription = $"React with {Constants.SortAlphabetical} to sort alphabetically, " +
                    $"{Constants.SortNumerical} to sort by number of posts, and " +     
                    $"{Constants.ChangeOrder} to switch between ascending/descending order.";
        private string rerollCharacterDescription = $"React with {Constants.RerollCharacter.Name} to reroll character";
        private string rerollSeriesDescription = $"{Constants.RerollSeries.Name} to reroll from the same series";
        private string listCharactersDescription = $"Use {Constants.Characters} to list all characters in the image";
        private string cycleCharacterPageDescription = $"View previous image with {Constants.PreviousImage} and next image with {Constants.NextImage}";
        private string rerollRandomDescription = $"React with {Constants.RerollRandom} to reroll a random character";
        private string rerollCollabDescription = $"React with {Constants.RerollCollab} to reroll this collab";
        private const int MaxSearchResults = 10000;
        private const int MaxSequentialImages = 100;
        private const bool IsInline = false;
        List<List<TagData>> pages;

        public Search(PageService ps, CacheService cs)
        {
            _pageService = ps;
            _cacheService = cs;
        }

        [Command("character")]
        [Summary("Rolls a random image of a character.\n" +
            "Lists all alternatives if a conclusive match is not found.")]
        public async Task CharacterAsync(string charName = null)
        {
            EmbedBuilder embedBuilder = 
                SearchAsync(charName, CATEGORY.CHARACTER, new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService)).Result;
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
        [Summary("Rolls a random image from a series.\n" +
            "Lists all alternatives if a conclusive match is not found.")]
        public async Task SeriesAsync(string seriesName = null)
        {
            EmbedBuilder embedBuilder 
                = SearchAsync(seriesName, CATEGORY.SERIES, new DbSeriesIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService)).Result;
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

        [Command("with_character")]
        [Summary("Lists series with the specified character.")]
        public async Task WithCharacterAsync(string charName = null)
        {
            if (string.IsNullOrEmpty(charName))
            {
                await ReplyAsync("Usage: oka.with_character character_name");

                return;
            }

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
                tag = charIndex.LookupTagById(id);

                if (!string.IsNullOrEmpty(tag))
                {
                    charName = tag;
                }
            }

            charName = TagParser.Format(charName);
            charNameEscaped = TagParser.EscapeUnderscore(charName);

            string series = charIndex.SeriesWithCharacter(charName);
            List<string> seriesList = new List<string>();

            seriesList.Add(series);

            if (!string.IsNullOrEmpty(series))
            {
                if (seriesList.Count < MaxSearchResults)
                {
                    List<TagData> seriesData = seriesIndex.LookupTagData(seriesList);

                    pages = TagParser.CompileSuggestions(seriesData, EmbedBuilder.MaxFieldCount);

                    EmbedBuilder embed = BuildSuggestionsEmbed(pages);

                    if (embed != null)
                    {
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
                }
                else
                {
                    await ReplyAsync(ExcessiveResults.Replace("%", charNameEscaped));
                }
            }
            else
            {
                await ReplyAsync(NoResults.Replace("%", charNameEscaped));
            }
        }

        [Command("in_series")]
        [Summary("Lists characters that belong to the specified series.")]
        public async Task InSeriesAsync(string seriesName = null)
        {
            if (string.IsNullOrEmpty(seriesName))
            {
                await ReplyAsync("Usage: oka.in_series series_name");

                return;
            }

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
                tag = seriesIndex.LookupTagById(id);

                if (!string.IsNullOrEmpty(tag))
                {
                    seriesName = tag;
                }
            }

            seriesName = TagParser.Format(seriesName);
            seriesNameEscaped = TagParser.EscapeUnderscore(seriesName);

            List<string> characters = seriesIndex.CharactersInSeries(seriesName);

            if (characters.Count > 0)
            {
                if (characters.Count < MaxSearchResults)
                {
                    List<TagData> characterData = charIndex.LookupTagData(characters);

                    pages = TagParser.CompileSuggestions(characterData, EmbedBuilder.MaxFieldCount);

                    EmbedBuilder embed = BuildSuggestionsEmbed(pages);

                    if (embed != null)
                    {
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
                }
                else
                {
                    await ReplyAsync(ExcessiveResults.Replace("%", seriesNameEscaped));
                }
            }
            else
            {
                await ReplyAsync(NoResults.Replace("%", seriesNameEscaped));
            }
        }

        [Command("random")]
        [Summary("Rolls a random character image.")]
        public async Task RandomAsync()
        {
            EmbedBuilder embedBuilder = RandomPostAsync().Result;

            if (embedBuilder != null)
            {
                var toSend = await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());

                await toSend.AddReactionAsync(Constants.RerollRandom);
                await toSend.AddReactionAsync(Constants.Characters);
            }
        }

        [Command("collab")]
        [Summary("Rolls an image that features all specified characters.")]
        public async Task CollabAsync(params string[] charNames)
        {
            EmbedBuilder embedBuilder = SearchCollabAsync(charNames).Result;
            ulong msgId;
            PageData pageData;

            if (embedBuilder != null)
            {
                Embed embed = embedBuilder.Build();
                var toSend = await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());

                if (embed.Image != null)
                {
                    await toSend.AddReactionAsync(Constants.RerollCollab);
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

        private async Task<EmbedBuilder> SearchCollabAsync(string[] searchTerms, int postIndex = 0, ROLL_SEQUENCE rollSequence = ROLL_SEQUENCE.RANDOM)
        {
            if (!_pageService.HandlerAdded)
            {
                Context.Client.ReactionAdded += ReactionAdded_Event;
                _pageService.HandlerAdded = true;
            }

            if (searchTerms.Length < 2 || searchTerms.Length > 5)
            {
                await ReplyAsync("Please provide between 2 and 5 character names.");

                return null;
            }

            bool wildcardLastArg = false;

            if (searchTerms[searchTerms.Length - 1] == "*")
            {
                wildcardLastArg = true;
                Array.Resize(ref searchTerms, searchTerms.Length - 1);
            }

            List<string> searchTermsList = new List<string>(searchTerms);

            foreach (string s in searchTermsList)
            {
                if (searchTermsList.FindAll(t => t == s).Count > 1)
                {
                    await ReplyAsync("Names may not be specified more than once.");

                    return null;
                }
            }

            DbCharacterIndex charIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService);

            int id;
            string tag;
            string matchedName;

            for (int i = 0; i < searchTerms.Length; i++)
            {
                if (int.TryParse(searchTerms[i], out id))
                {
                    tag = charIndex.LookupTagById(id);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        searchTerms[i] = tag;
                    }
                }

                searchTerms[i] = TagParser.Format(searchTerms[i]);

                if (!charIndex.HasExactMatch(searchTerms[i], out matchedName))
                {
                    await ReplyAsync($"Character name '{TagParser.EscapeUnderscore(searchTerms[i])}' could not be found.");

                    return null;
                }
                else
                {
                    searchTerms[i] = matchedName;
                }
            }

            EmbedBuilder embed;

            if (wildcardLastArg)
            {
                List<string> furtherCollabs = charIndex.CollabsWithCharacters(searchTerms);

                if (furtherCollabs.Count == 0)
                {
                    await ReplyAsync($"No further collabs with the specified character(s) exist.");

                    return null;
                }

                List<TagData> tagData = charIndex.LookupTagData(furtherCollabs);

                pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
                embed = BuildSuggestionsEmbed(pages);
            }
            else
            {
                PostData postData = null;

                switch (rollSequence)
                {
                    case ROLL_SEQUENCE.RANDOM:
                        postData = charIndex.LookupRandomCollab(searchTerms);
                        break;
                    case ROLL_SEQUENCE.PREVIOUS:
                        postData = charIndex.LookupPreviousCollab(searchTerms, postIndex);
                        break;
                    case ROLL_SEQUENCE.NEXT:
                        postData = charIndex.LookupNextCollab(searchTerms, postIndex);
                        break;
                }

                string embedDescription = rerollCollabDescription + "." +
                    Environment.NewLine +
                    listCharactersDescription + ".";

                if (_cacheService.CacheSize() < MaxSequentialImages)
                {
                    embedDescription += Environment.NewLine + cycleCharacterPageDescription + ".";
                }

                if (postData != null && !string.IsNullOrEmpty(postData.Link))
                {
                    embed = BuildImageEmbed(postData, embedDescription);
                }
                else
                {
                    await ReplyAsync($"No images found for this collab.");

                    return null;
                }
            }

            return embed;
        }

        private async Task<EmbedBuilder> SearchAsync(string searchTerm, CATEGORY category, ITagIndex tagIndex, int postIndex = 0, ROLL_SEQUENCE rollSequence = ROLL_SEQUENCE.RANDOM)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                string usageString = "Usage: ";

                switch (category)
                {
                    case CATEGORY.CHARACTER:
                        await ReplyAsync(usageString + "oka.character character_name");
                        break;
                    case CATEGORY.SERIES:
                        await ReplyAsync(usageString + "oka.series series_name");
                        break;
                }

                return null;
            }

            if (!_pageService.HandlerAdded)
            {
                Context.Client.ReactionAdded += ReactionAdded_Event;
                _pageService.HandlerAdded = true;
            }

            searchTerm = TagParser.Format(searchTerm);

            string tag;
            string searchTermEscaped;
            string searchTermMatched;
            int id;
            PostData postData = null;
            List<string> tags;
            List<TagData> tagData;
            EmbedBuilder embed;
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            string embedDescription = rerollCharacterDescription + ", " + 
                rerollSeriesDescription + ". " +
                Environment.NewLine +
                listCharactersDescription +".";

            if (int.TryParse(searchTerm, out id))
            {
                tag = tagIndex.LookupTagById(id);

                if (!string.IsNullOrEmpty(tag))
                {
                    searchTerm = tag;
                }
            }

            searchTermEscaped = TagParser.EscapeUnderscore(searchTerm);

            if (tagIndex.HasExactMatch(searchTerm, out searchTermMatched))
            {
                switch (rollSequence)
                {
                    case ROLL_SEQUENCE.RANDOM:
                        postData = tagIndex.LookupRandomPost(searchTermMatched);
                        break;
                    case ROLL_SEQUENCE.PREVIOUS:
                        postData = tagIndex.LookupPreviousPost(searchTermMatched, postIndex);
                        break;
                    case ROLL_SEQUENCE.NEXT:
                        postData = tagIndex.LookupNextPost(searchTermMatched, postIndex);
                        break;
                }

                switch (category)
                {
                    case CATEGORY.CHARACTER:
                        if (_cacheService.CacheSize() < MaxSequentialImages)
                        {
                            embedDescription += Environment.NewLine + cycleCharacterPageDescription + ".";
                        }
                        break;
                }

                if (postData != null && !string.IsNullOrEmpty(postData.Link))
                {
                    embed = BuildImageEmbed(postData, embedDescription);
                }
                else
                {
                    await ReplyAsync(NoImages.Replace("%", searchTermEscaped));

                    return null;
                }
            }
            else
            {
                embed = new EmbedBuilder();
                tags = tagIndex.LookupTags(searchTerm);

                if (tags.Count > 0)
                {
                    if (tags.Count > MaxSearchResults)
                    {
                        await ReplyAsync(ExcessiveResults.Replace("%", searchTerm));

                        return null;
                    }

                    tagData = tagIndex.LookupTagData(tags);
                    pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
                    embed = BuildSuggestionsEmbed(pages);
                }
                else
                {
                    await ReplyAsync(NoResults.Replace("%", searchTermEscaped));

                    return null;
                }
            }

            return embed;
        }

        private async Task<EmbedBuilder> RandomPostAsync()
        {
            if (!_pageService.HandlerAdded)
            {
                Context.Client.ReactionAdded += ReactionAdded_Event;
                _pageService.HandlerAdded = true;
            }

            DbCharacterIndex characterIndex = new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService);
            EmbedBuilder embed;
            PostData postData;
            string tag = characterIndex.LookupRandomTag();
            string title;

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

                string embedDescription =  rerollRandomDescription + "." +
                    Environment.NewLine +
                    listCharactersDescription + ".";

                embed = BuildImageEmbed(postData, embedDescription);
            }
            else
            {
                await ReplyAsync("An error occurred during random tag lookup.");

                return null;
            }

            return embed;
        }

        private EmbedBuilder BuildImageEmbed(PostData postData, string embedDescription)
        {
            StringBuilder seriesNames = new StringBuilder(TagParser.EscapeUnderscore(postData.SeriesName));
            StringBuilder title = new StringBuilder(TagParser.BuildTitle(postData.TagName));
            StringBuilder tagIds = new StringBuilder(postData.TagId.ToString());
            StringBuilder tagIdTitle = new StringBuilder("Character ID");
            StringBuilder seriesNameTitle = new StringBuilder("Series Name");

            if (postData.AdditionalData != null)
            {
                foreach (string s in postData.AdditionalData.AdditionalTagNames)
                {
                    title.Append($", {TagParser.BuildTitle(s)}");
                }

                foreach (int i in postData.AdditionalData.AdditionalTagIds)
                {
                    tagIds.Append($", {i}");
                }

                tagIdTitle.Append("s");

                foreach (string s in postData.AdditionalData.AdditionalSeriesNames)
                {
                    if (s != postData.SeriesName)
                    {
                        seriesNames.Append($", {s}");
                    }
                }

                seriesNameTitle.Append("s");
            }

            return new EmbedBuilder().WithTitle(title.ToString())
                            .AddField(tagIdTitle.ToString(), tagIds.ToString(), true)
                            .AddField("Post ID", postData.LinkId, true)
                            .AddField(seriesNameTitle.ToString(), seriesNames.ToString(), true)
                            .WithDescription(embedDescription)
                            .WithImageUrl(postData.Link)
                            .WithUrl(postData.Link)
                            .WithAuthor(Context.Client.CurrentUser)
                            .WithFooter($"Image {postData.PostIndex + 1} of {_cacheService.CacheSize()}")
                            .WithColor(Color.DarkGrey)
                            .WithCurrentTimestamp();
        }

        private EmbedBuilder BuildSuggestionsEmbed(List<List<TagData>> pages)
        {
            EmbedBuilder embed = null;

            if (pages.Count > 0)
            {
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

                embed = new EmbedBuilder()
                    .WithTitle(SuggestionTitle)
                    .WithFields(fields)
                    .WithDescription(suggestionDescription)
                    .WithFooter($"Page 1 of {pages.Count}");
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
                embedBuilder = SearchAsync(characterId, CATEGORY.CHARACTER, 
                    new DbCharacterIndex(ConfigUtils.GetCurrentDatabase(Constants.ProductionConfig), _cacheService)).Result;

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
            else if (reaction.Emote.Name == Constants.RerollCollab.Name)
            {
                characterId = embedFields[0].Value;
                embedBuilder = SearchCollabAsync(characterId.Split(", ")).Result;

                if (embedBuilder != null)
                {
                    embed = embedBuilder.Build();
                    var toSend = await channel.SendMessageAsync(embed: embed);

                    if (embed.Image != null)
                    {
                        await toSend.AddReactionAsync(Constants.RerollCollab);
                        await toSend.AddReactionAsync(Constants.Characters);

                        if (_cacheService.CacheSize() < MaxSequentialImages)
                        {
                            await toSend.AddReactionAsync(Constants.PreviousImage);
                            await toSend.AddReactionAsync(Constants.NextImage);
                        }
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
                    if (characterId.Contains(", "))
                    {
                        embedBuilder = SearchCollabAsync(characterId.Split(", "), imageIndex - 1, ROLL_SEQUENCE.PREVIOUS).Result;

                        if (embedBuilder != null)
                        {
                            embed = embedBuilder.Build();
                            var toSend = await channel.SendMessageAsync(embed: embed);

                            if (embed.Image != null)
                            {
                                await toSend.AddReactionAsync(Constants.RerollCollab);
                                await toSend.AddReactionAsync(Constants.Characters);

                                if (_cacheService.CacheSize() < MaxSequentialImages)
                                {
                                    await toSend.AddReactionAsync(Constants.PreviousImage);
                                    await toSend.AddReactionAsync(Constants.NextImage);
                                }
                            }
                        }
                    }
                    else
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
            }
            else if (reaction.Emote.Name == Constants.NextImage.Name)
            {
                characterId = embedFields[0].Value;
                footerText.Split(" ").First(i => int.TryParse(i, out imageIndex));
                footerText.Split(" ").Last(i => int.TryParse(i, out numImages));

                if (imageIndex < numImages)
                {
                    if (characterId.Contains(", "))
                    {
                        embedBuilder = SearchCollabAsync(characterId.Split(", "), imageIndex - 1, ROLL_SEQUENCE.NEXT).Result;

                        if (embedBuilder != null)
                        {
                            embed = embedBuilder.Build();
                            var toSend = await channel.SendMessageAsync(embed: embed);

                            if (embed.Image != null)
                            {
                                await toSend.AddReactionAsync(Constants.RerollCollab);
                                await toSend.AddReactionAsync(Constants.Characters);

                                if (_cacheService.CacheSize() < MaxSequentialImages)
                                {
                                    await toSend.AddReactionAsync(Constants.PreviousImage);
                                    await toSend.AddReactionAsync(Constants.NextImage);
                                }
                            }
                        }
                    }
                    else
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
                embedBuilder = RandomPostAsync().Result;

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
                .WithDescription(suggestionDescription)
                .WithFooter($"Page {pageData.PageNum + 1} of {pageData.Pages.Count}");
        }
    }
}
