﻿using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class LobitaModule : ModuleBase<SocketCommandContext>
    {
        private readonly VideoService _videoService;
        private string baseAddress = Environment.GetEnvironmentVariable("PUBLIC_IP");
        private string workingDirectory = Directory.GetCurrentDirectory();
        private string imagesDirectory = "images";
        private string videosDirectory = "videos";
        private string footerText = "Powered by LobitaBot.";
        private string titleText = "Click here to play video...";
        private string[] imageHandles = new string[] 
        {
            "lysithea",
            "holo",
            "fenrir",
            "myuri",
            "ryouko",
            "nagatoro",
            "velvet"
        };
        private string[] videoHandles = new string[]
        {
            "OP",
            "ED"
        };

        public LobitaModule (VideoService vs) => _videoService = vs;

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

        [Command("help")]
        [Summary("Display the list of commands.")]
        public async Task HelpAsync()
        {
            string help = "---Commands---\n" +
                "- oka.lysithea\n" +
                "- oka.holo\n" +
                "- oka.fenrir\n" +
                "- oka.myuri\n" +
                "- oka.ryouko\n" +
                "- oka.nagatoro\n" +
                "- oka.velvet\n" +
                "- oka.mita";

            await ReplyAsync(help);
        }

        [Command("lysithea")]
        [Summary("Displays a random image of Lysithea.")]
        public async Task LysitheaAsync()
        {
            string path = BuildRandomImageLinkFor(imageHandles[0]);

            var embed = new EmbedBuilder()
            {
                ImageUrl = path
            };

            embed.WithTitle("Lysithea")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("holo")]
        [Summary("Displays a random image of Holo.")]
        public async Task HoloAsync()
        {
            string path = BuildRandomImageLinkFor(imageHandles[1]);

            var embed = new EmbedBuilder()
            {
                ImageUrl = path
            };

            embed.WithTitle("Holo")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("fenrir")]
        [Summary("Displays a random image of Fenrir.")]
        public async Task FenrirAsync()
        {
            string path = BuildRandomImageLinkFor(imageHandles[2]);

            var embed = new EmbedBuilder()
            {
                ImageUrl = path
            };

            embed.WithTitle("Fenrir")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("myuri")]
        [Summary("Displays a random image of Myuri.")]
        public async Task MyuriAsync()
        {
            string path = BuildRandomImageLinkFor(imageHandles[3]);

            var embed = new EmbedBuilder()
            {
                ImageUrl = path
            };

            embed.WithTitle("Myuri")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("ryouko")]
        [Summary("Displays a random image of Ryouko.")]
        public async Task RyoukoAsync()
        {
            string path = BuildRandomImageLinkFor(imageHandles[4]);

            var embed = new EmbedBuilder()
            {
                ImageUrl = path
            };

            embed.WithTitle("Ryouko")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("nagatoro")]
        [Summary("Displays a random image of Nagatoro.")]
        public async Task NagatoroAsync()
        {
            string path = BuildRandomImageLinkFor(imageHandles[5]);

            var embed = new EmbedBuilder()
            {
                ImageUrl = path
            };

            embed.WithTitle("Nagatoro")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("velvet")]
        [Summary("Displays a random image of Velvet.")]
        public async Task VelvetAsync()
        {
            string path = BuildRandomImageLinkFor(imageHandles[6]);

            var embed = new EmbedBuilder()
            {
                ImageUrl = path
            };

            embed.WithTitle("Velvet")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("mita")]
        [Summary("Displays a random image of any character.")]
        public async Task MitaAsync()
        {
            string path = BuildRandomImageLink();

            var embed = new EmbedBuilder()
            {
                ImageUrl = path
            };

            embed.WithTitle("Okamita")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = footerText)
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
                .WithFooter(footer => footer.Text = footerText)
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
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }
    }
}
