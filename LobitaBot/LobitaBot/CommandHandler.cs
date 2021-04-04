using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class VideoService
    {
        public int RollIndex { get; set; } = -1;
    }

    public class PageData
    {
        public List<List<TagData>> Pages { get; set; }
        public int PageNum { get; set; }
        public DateTime DateTime { get; }
        public bool AlphabeticallySorted { get; set; } = false;
        public bool NumericallySorted { get; set; } = false;
        public bool SortedAscending { get; set; } = false;

        public PageData(List<List<TagData>> pages)
        {
            Pages = pages;
            PageNum = 0;
            DateTime = DateTime.Now;
        }
    }

    public class PageService
    {
        public bool HandlerAdded { get; set; } = false;
        public Dictionary<ulong, PageData> PageIndex { get; } = new Dictionary<ulong, PageData>();

        public void AddLimited(ulong msgId, PageData pageData)
        {
            if (PageIndex.Count == 100)
            {
                ulong oldestMsg = PageIndex.Aggregate((x, y) => x.Value.DateTime < y.Value.DateTime ? x : y).Key;

                PageIndex.Remove(oldestMsg);
            }

            PageIndex.Add(msgId, pageData);
        }

        public void SortAlphabeticalAsc(ulong msgId)
        {
            PageData pageData = PageIndex[msgId];
            List<TagData> tagData = new List<TagData>();

            foreach (List<TagData> page in pageData.Pages)
            {
                foreach (TagData t in page)
                {
                    tagData.Add(t);
                }
            }

            tagData.Sort((t1, t2) => t1.TagName.CompareTo(t2.TagName));

            PageIndex[msgId].Pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
            PageIndex[msgId].AlphabeticallySorted = true;
            PageIndex[msgId].NumericallySorted = false;
            PageIndex[msgId].SortedAscending = true;
        }

        public void SortAlphabeticalDesc(ulong msgId)
        {
            PageData pageData = PageIndex[msgId];
            List<TagData> tagData = new List<TagData>();

            foreach (List<TagData> page in pageData.Pages)
            {
                foreach (TagData t in page)
                {
                    tagData.Add(t);
                }
            }

            tagData.Sort((t1, t2) => t2.TagName.CompareTo(t1.TagName));

            PageIndex[msgId].Pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
            PageIndex[msgId].AlphabeticallySorted = true;
            PageIndex[msgId].NumericallySorted = false;
            PageIndex[msgId].SortedAscending = false;
        }

        public void SortPostNumAsc(ulong msgId)
        {
            PageData pageData = PageIndex[msgId];
            List<TagData> tagData = new List<TagData>();

            foreach (List<TagData> page in pageData.Pages)
            {
                foreach (TagData t in page)
                {
                    tagData.Add(t);
                }
            }

            tagData.Sort((t1, t2) => t1.NumLinks.CompareTo(t2.NumLinks));

            PageIndex[msgId].Pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
            PageIndex[msgId].AlphabeticallySorted = false;
            PageIndex[msgId].NumericallySorted = true;
            PageIndex[msgId].SortedAscending = true;
        }

        public void SortPostNumDesc(ulong msgId)
        {
            PageData pageData = PageIndex[msgId];
            List<TagData> tagData = new List<TagData>();

            foreach (List<TagData> page in pageData.Pages)
            {
                foreach (TagData t in page)
                {
                    tagData.Add(t);
                }
            }

            tagData.Sort((t1, t2) => t2.NumLinks.CompareTo(t1.NumLinks));

            PageIndex[msgId].Pages = TagParser.CompileSuggestions(tagData, EmbedBuilder.MaxFieldCount);
            PageIndex[msgId].AlphabeticallySorted = false;
            PageIndex[msgId].NumericallySorted = true;
            PageIndex[msgId].SortedAscending = false;
        }
    }

    public class CommandHandler
    {
        private readonly CommandService commands;
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider services;

        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            this.commands = commands;
            this.client = client;
            services = new ServiceCollection()
                .AddSingleton<VideoService>()
                .AddSingleton<PageService>()
                .BuildServiceProvider();

            this.client.SetGameAsync("oka.help");
        }

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;

            if (message == null)
            {
                return;
            }

            // Create a number to track where the prefix ends and the command begins
            int argPos = 3;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasStringPrefix(Constants.Prefix, ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
            {
                return;
            }

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: services);
        }
    }
}
