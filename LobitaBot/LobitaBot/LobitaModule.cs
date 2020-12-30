using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
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

    public class LobitaModule : ModuleBase<SocketCommandContext>
    {
        private readonly VideoService _videoService;
        private ITagIndex index = new DbTagIndex();
        private TagParser parser = new TagParser();
        private string baseAddress = Environment.GetEnvironmentVariable("PUBLIC_IP");
        private string workingDirectory = Directory.GetCurrentDirectory();
        private string imagesDirectory = "images";
        private string videosDirectory = "videos";
        private string footerText = "Requested by ";
        private string titleText = "Click here to play video...";
        private const int MaxResults = 1000;
        private string[] imageHandles = new string[] 
        {
            "lysithea",
            "holo",
            "fenrir",
            "myuri",
            "ryouko",
            "nagatoro",
            "velvet",
            "hololive"
        };
        private string[] videoHandles = new string[]
        {
            "OP",
            "ED"
        };

        public LobitaModule(VideoService vs)
        {
            _videoService = vs;
        }

        private string BuildRandomImageLinkFor(string imageHandle)
        {
            DirectoryInfo di = new DirectoryInfo(Path.Join(workingDirectory, imagesDirectory, imageHandle));
            FileInfo[] files = di.GetFiles();
            Random rand = new Random();
            int chosen = rand.Next(0, files.Length - 1);

            return $"http://{baseAddress}/{imagesDirectory}/{imageHandle}/{files[chosen].Name}" + GenerateUniqueParam();
        }

        private string BuildRandomVideoLinkFor(string videoHandle)
        {
            DirectoryInfo di = new DirectoryInfo(Path.Join(workingDirectory, videosDirectory, videoHandle));
            FileInfo[] files = di.GetFiles();
            
            if(_videoService.RollIndex < 0)
            {
                _videoService.RollIndex = files.Length - 1;
            }

            return $"http://{baseAddress}/{videosDirectory}/{videoHandle}/{files[_videoService.RollIndex--].Name}" + GenerateUniqueParam();
        }

        private string BuildRandomImageLink()
        {
            Random rand = new Random();
            int chosenDir = rand.Next(0, imageHandles.Length - 1);

            return BuildRandomImageLinkFor(imageHandles[chosenDir]);
        }

        private string GenerateUniqueParam()
        {
            return $"?_={DateTime.Now.Millisecond}";
        }

        private Embed CreateImageEmbedFor(string cmdName, string cmdHandle)
        {
            var embed = new EmbedBuilder()
            {
                ImageUrl = BuildRandomImageLinkFor(cmdHandle)
            };

            embed.WithTitle(cmdName)
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText + Context.User.Username)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            return embed.Build();
        }

        [Command("help")]
        [Summary("Display the list of commands.")]
        public async Task HelpAsync()
        {
            string help = "Images:\n" +
                "\t-oka.lysithea\n" +
                "\t-oka.holo\n" +
                "\t-oka.fenrir\n" +
                "\t-oka.myuri\n" +
                "\t-oka.ryouko\n" +
                "\t-oka.nagatoro\n" +
                "\t-oka.velvet\n" +
                "\t-oka.hololive\n" +
                "\t-oka.mita\n" +
                "\t-oka.search search_term\n" +
                "Videos:\n" +
                "\t-oka.op\n" +
                "\t-oka.ed\n" +
                "Avatar:\n" +
                "\t-oka.avatar [user_ID]";

            await ReplyAsync(help);
        }

        [Command("lysithea")]
        [Summary("Displays a random image of Lysithea.")]
        public async Task LysitheaAsync()
        {
            Embed e = CreateImageEmbedFor("Lysithea", imageHandles[0]);

            await ReplyAsync(embed: e);
        }

        [Command("holo")]
        [Summary("Displays a random image of Holo.")]
        public async Task HoloAsync()
        {
            Embed e = CreateImageEmbedFor("Holo", imageHandles[1]);

            await ReplyAsync(embed: e);
        }

        [Command("fenrir")]
        [Summary("Displays a random image of Fenrir.")]
        public async Task FenrirAsync()
        {
            Embed e = CreateImageEmbedFor("Fenrir", imageHandles[2]);

            await ReplyAsync(embed: e);
        }

        [Command("myuri")]
        [Summary("Displays a random image of Myuri.")]
        public async Task MyuriAsync()
        {
            Embed e = CreateImageEmbedFor("Myuri", imageHandles[3]);

            await ReplyAsync(embed: e);
        }

        [Command("ryouko")]
        [Summary("Displays a random image of Ryouko.")]
        public async Task RyoukoAsync()
        {
            Embed e = CreateImageEmbedFor("Ryouko", imageHandles[4]);

            await ReplyAsync(embed: e);
        }

        [Command("nagatoro")]
        [Summary("Displays a random image of Nagatoro.")]
        public async Task NagatoroAsync()
        {
            Embed e = CreateImageEmbedFor("Nagatoro", imageHandles[5]);

            await ReplyAsync(embed: e);
        }

        [Command("velvet")]
        [Summary("Displays a random image of Velvet.")]
        public async Task VelvetAsync()
        {
            Embed e = CreateImageEmbedFor("Velvet", imageHandles[6]);

            await ReplyAsync(embed: e);
        }

        [Command("hololive")]
        [Summary("Displays a random Hololive image.")]
        public async Task HololiveAsync()
        {
            Embed e = CreateImageEmbedFor("Hololive", imageHandles[7]);

            await ReplyAsync(embed: e);
        }

        [Command("mita")]
        [Summary("Displays a random image of any character.")]
        public async Task MitaAsync()
        {
            var embed = new EmbedBuilder()
            {
                ImageUrl = BuildRandomImageLink()
            };

            embed.WithTitle("Okamita")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText + Context.User.Username)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("op")]
        [Summary("Displays a random anime opening theme.")]
        public async Task OpAsync()
        {
            string path = BuildRandomVideoLinkFor(videoHandles[0]);

            var embed = new EmbedBuilder()
            {
                Url = path,
                ImageUrl = $"http://{baseAddress}/images/OP_img.png" + GenerateUniqueParam()
            };

            embed.WithTitle(titleText)
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText + Context.User.Username)
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("ed")]
        [Summary("Displays a random anime opening theme.")]
        public async Task EdAsync()
        {
            string path = BuildRandomVideoLinkFor(videoHandles[1]);

            var embed = new EmbedBuilder()
            {
                Url = path,
                ImageUrl = $"http://{baseAddress}/images/ED_img.png" + GenerateUniqueParam()
            };

            embed.WithTitle(titleText)
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText + Context.User.Username)
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("avatar")]
        [Summary("Displays the requesting user's avatar.")]
        public async Task AvatarAsync(string userID = null)
        {
            var users = Context.Message.MentionedUsers;
            EmbedBuilder embed = null;

            if (!string.IsNullOrEmpty(userID))
            {
                if(users.Count() == 1)
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
                .WithFooter(footer => footer.Text = footerText + Context.User.Username)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("search")]
        [Summary("Search for images related to a particular free-text tag.")]
        public async Task SearchAsync(string searchTerm = null)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                await ReplyAsync("Usage: oka.search search term");

                return;
            }
            else
            {
                searchTerm = searchTerm.Replace("\\", string.Empty);
                searchTerm = searchTerm.ToLower();

                string tag;
                string link;
                string title;
                string searchTermEscaped;
                string description;
                int id;
                List<string> suggestions;
                List<string> tags;
                List<TagData> tagData;
                EmbedBuilder embed = new EmbedBuilder();

                if (int.TryParse(searchTerm, out id))
                {
                    tag = index.LookupSingleTag(id);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        searchTerm = tag;
                    }
                }

                if (searchTerm.Contains("_"))
                {
                    searchTermEscaped = searchTerm.Insert(searchTerm.IndexOf("_"), @"\");
                    searchTermEscaped = searchTerm.Replace("_", "\\_");
                }
                else
                {
                    searchTermEscaped = searchTerm;
                }

                if (index.TagExists(searchTerm))
                {
                    link = index.LookupRandomLink(searchTerm);
                    title = parser.BuildTitle(searchTerm);

                    if (!string.IsNullOrEmpty(link))
                    {
                        embed.WithTitle(title)
                        .WithImageUrl(link)
                        .WithUrl(link)
                        .WithAuthor(Context.Client.CurrentUser)
                        .WithFooter(footer => footer.Text = footerText + Context.User.Username)
                        .WithColor(Color.DarkGrey)
                        .WithCurrentTimestamp();
                    }
                    else
                    {
                        await ReplyAsync($"No images found for '{searchTermEscaped}'.");

                        return;
                    }
                }
                else
                {
                    tags = index.LookupTags(searchTerm);

                    if (tags.Count < MaxResults)
                    {
                        suggestions = parser.FilterSuggestions(tags, searchTerm);
                    }
                    else
                    {
                        await ReplyAsync($"Your search for '{searchTerm}' is too broad! Please be more specific.");

                        return;
                    }

                    if (suggestions.Count > 0)
                    {
                        tagData = index.LookupTagData(suggestions);
                        description = parser.CompileSuggestions(tagData);

                        if (!string.IsNullOrEmpty(description))
                        {
                            embed.WithTitle($"<Tag ID> Tag Name (#Posts). Showing top results.");
                            embed.WithDescription(description);
                        }
                        else
                        {
                            await ReplyAsync($"No suggestions exist for search term '{searchTermEscaped}'.");

                            return;
                        }
                    }
                    else
                    {
                        await ReplyAsync($"No results found for '{searchTermEscaped}'.");

                        return;
                    }
                }

                await Context.Channel.SendMessageAsync(embed: embed.Build());
            }
        }

        [Command("random")]
        [Summary("Search for random images belonging to a random tag.")]
        public async Task RandomAsync()
        {
            EmbedBuilder embed = new EmbedBuilder();
            string tag = index.LookupRandomTag();
            string title;
            string link;

            if (!string.IsNullOrEmpty(tag))
            {
                title = parser.BuildTitle(tag);
                link = index.LookupRandomLink(tag);

                while (string.IsNullOrEmpty(link))
                {
                    tag = index.LookupRandomTag();
                    title = parser.BuildTitle(tag);
                    link = index.LookupRandomLink(tag);
                }

                embed.WithTitle(title)
                .WithImageUrl(link)
                .WithUrl(link)
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText + Context.User.Username)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();
            }
            else
            {
                await ReplyAsync("An error occurred during random tag lookup.");

                return;
            }

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
