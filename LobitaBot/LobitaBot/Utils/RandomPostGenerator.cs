﻿using Discord;
using LobitaBot.Services;
using System;
using System.Xml;

namespace LobitaBot.Utils
{
    class RandomPostGenerator
    {
        public Embed RandomPost(string tags, string url)
        {
            var result = HttpXmlService.GetRequestXml(url);

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
            string characters = root.SelectSingleNode("tag-string-character").InnerText;
            string artist = root.SelectSingleNode("tag-string-artist").InnerText;
            string created = root.SelectSingleNode("created-at").InnerText;
            DateTime parsed = DateTime.Parse(created);
            string title = tags == null ? Constants.RandomImageTitle : Format.Sanitize(tags);
            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(Format.Sanitize(description))
                .WithImageUrl(imageUrl)
                .WithUrl(imageUrl)
                .WithColor(Color.DarkGrey)
                .WithCurrentTimestamp()
                .WithFooter("Created on " + parsed.ToLongDateString())
                .AddField("Characters", Format.Sanitize(characters))
                .AddField("Artist", Format.Sanitize(artist))
                .AddField("Series", Format.Sanitize(series));

            return embedBuilder.Build();
        }
    }
}
