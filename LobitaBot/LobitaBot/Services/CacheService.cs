using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LobitaBot
{
    public class CacheService
    {
        private ConcurrentBag<PostData> tagCache = new ConcurrentBag<PostData>();
        public CancellationTokenSource CTS { get; set; }
        public Task CacheTask { get; set; }

        public CacheService()
        {
            CTS = new CancellationTokenSource();
        }

        public void Add(PostData pd)
        {
            tagCache.Add(pd);
        }

        public void SetCache(ICollection<PostData> pds)
        {
            tagCache = new ConcurrentBag<PostData>(pds);
        }

        public void Clear()
        {
            tagCache.Clear();
        }

        public bool IsEmpty()
        {
            return tagCache.Count == 0;
        }

        public bool CharacterAloneInCache(string tagName)
        {
            List<PostData> snapshot = tagCache.ToList();

            return snapshot.Count != 0 &&
                snapshot.All(pd => pd.TagName == tagName) &&
                snapshot.All(pd => pd.AdditionalData == null);
        }

        public bool SeriesInCache(string seriesName)
        {
            List<PostData> snapshot = tagCache.ToList();

            return snapshot.Count != 0 &&
                snapshot.All(pd => pd.SeriesName == seriesName) &&
                snapshot.All(pd => pd.AdditionalData == null);
        }

        public bool CollabInCache(string[] tagNames)
        {
            List<PostData> snapshot = tagCache.ToList();

            return snapshot.Count != 0 &&
                snapshot.All(pd => pd.TagName == tagNames[0]) &&
                snapshot.All(pd =>
                {
                    bool b = true;

                    for (int i = 1; i < tagNames.Length; i++)
                    {
                        if (pd.AdditionalData != null && pd.AdditionalData.AdditionalTagNames.Contains(tagNames[i]))
                        {
                            b &= true;
                        }
                        else
                        {
                            b &= false;
                        }
                    }

                    return b;
                });
        }

        public PostData CacheRandom()
        {
            PostData pd = null;

            if (tagCache.Count > 0)
            {
                Random rand = new Random();
                int index = rand.Next(0, tagCache.Count);

                pd = tagCache.ToArray()[index];
            }

            return pd;
        }

        public PostData CacheNext(int index)
        {
            PostData pd = null;
            PostData[] pds = tagCache.ToArray();
            Array.Sort(pds, (x, y) => x.PostIndex.CompareTo(y.PostIndex));

            if (index < tagCache.Count - 1 && index >= 0)
            {

                pd = pds[index + 1];
            }
            else if (index >= tagCache.Count - 1)
            {
                pd = pds.Last();
            }

            return pd;
        }

        public PostData CachePrevious(int index)
        {
            PostData pd = null;
            PostData[] pds = tagCache.ToArray();
            Array.Sort(pds, (x, y) => x.PostIndex.CompareTo(y.PostIndex));

            if (index > 0 && index < tagCache.Count)
            {

                pd = pds[index - 1];
            }
            else if (index <= 0)
            {
                pd = pds.First();
            }

            return pd;
        }

        public int CacheSize()
        {
            return tagCache.Count();
        }
    }
}
