using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Hateblo
{
    internal class Feed : IEnumerable<Entry>
    {
        private readonly List<Entry> _list
            = new List<Entry>(_maxCount);

        // 仕様上は最大 7 件だが、実際は 10 件流れてくる
        private const int _maxCount = 10;

        internal string NextRequestUri { get; private set; }

        public IEnumerator<Entry> GetEnumerator()
            => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        internal static Feed FromXElement(XElement xElement)
        {
            var feed = new Feed
            {
                NextRequestUri = xElement.Elements(Extensions._atomNamespace + "link")
                    .SingleOrDefault(xe => xe.Attribute("rel").Value == "next")?
                    .Attribute("href").Value,
            };

            var entries = xElement.Elements(Extensions._atomNamespace + "entry").Select(xe =>
            {
                var entry = new Entry();
                entry.FromXElement(xe);
                return entry;
            });

            feed._list.AddRange(entries);
            return feed;
        }
    }
}
