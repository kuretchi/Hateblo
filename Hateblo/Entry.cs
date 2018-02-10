using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Hateblo.Internal;

namespace Hateblo
{
    /// <summary>
    /// ブログエントリを表します。
    /// </summary>
    public class Entry
    {
        private static readonly string _memberUriPattern
            = $@"https://blog\.hatena\.ne\.jp/[^/]+/[^/]+/atom/entry/(?<entry_id>[^/]*)";

        private static readonly Regex _memberUriRegex
            = new Regex(_memberUriPattern);

        /// <summary>
        /// ブログエントリ ID を取得します。
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// ブログエントリのタイトルを取得または設定します。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// ブログエントリの投稿日時を取得または設定します。
        /// </summary>
        /// <remarks>
        /// <see langword="null"/> を設定した場合、 <see cref="Blog.PostAsync(Entry)"/> メソッドや <see cref="Blog.UpdateAsync(Entry)"/> メソッドによってブログエントリが投稿された際に、その時点の日時が自動的に設定されます。
        /// </remarks>
        public HatenaDateTime? UpdateTime { get; set; }

        // todo: ドキュメントコメント
        public HatenaDateTime PublicationTime { get; private set; }

        // todo: ドキュメントコメント
        public HatenaDateTime EditTime { get; private set; }

        /// <summary>
        /// ブログエントリのカテゴリを格納するコレクションを取得します。
        /// </summary>
        public ICollection<string> Categories { get; } = new HashSet<string>();

        /// <summary>
        /// ブログエントリの要約を取得します。
        /// </summary>
        public string Summary { get; private set; }

        /// <summary>
        /// ブログエントリの本文を格納する、 <see cref="Hateblo.Content"/> オブジェクトを取得します。
        /// </summary>
        public Content Content { get; } = new Content();

        /// <summary>
        /// ブログエントリの HTML エンコードされた本文を取得します。
        /// </summary>
        public string FormattedContent { get; private set; }

        /// <summary>
        /// ブログエントリが下書きであるかどうかを表す値を取得または設定します。
        /// </summary>
        public bool IsDraft { get; set; }

        internal string MemberUri { get; set; }

        internal XDocument ToXDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "uft-8", null),
                new XElement(Extensions._atomNamespace + "entry",
                    new XAttribute(XNamespace.Xmlns + "app", Extensions._appNamespace),
                    new XElement(Extensions._atomNamespace + "title",
                        this.Title),
                    new XElement(Extensions._atomNamespace + "content",
                        new XAttribute("type", "text/plain"),
                        this.Content.Text),
                    new XElement(Extensions._atomNamespace + "updated",
                        this.UpdateTime),
                    this.Categories.Select(c
                        => new XElement(Extensions._atomNamespace + "category", new XAttribute("term", c))),
                    new XElement(Extensions._appNamespace + "control",
                        new XElement(Extensions._appNamespace + "draft",
                            ToYesOrNo(this.IsDraft)))));
        }

        internal void FromXElement(XElement xElement)
        {
            this.MemberUri = xElement.Elements(Extensions._atomNamespace + "link")
                .Single(xe => xe.Attribute("rel").Value == "edit")
                .Attribute("href").Value;

            this.Id = _memberUriRegex.Match(this.MemberUri).Groups["entry_id"].Value;

            this.Title = xElement.Element(Extensions._atomNamespace + "title").Value;

            this.UpdateTime = HatenaDateTime.Parse(
                xElement.Element(Extensions._atomNamespace + "updated").Value);

            this.PublicationTime = HatenaDateTime.Parse(
                xElement.Element(Extensions._atomNamespace + "published").Value);

            this.EditTime = HatenaDateTime.Parse(
                    xElement.Element(Extensions._appNamespace + "edited").Value);

            this.Summary = xElement.Element(Extensions._atomNamespace + "summary").Value;

            this.FormattedContent = xElement.Element(Extensions._hatenaNamespace + "formatted-content").Value;

            this.IsDraft = FromYesOrNo(
                xElement.Element(Extensions._appNamespace + "control").Element(Extensions._appNamespace + "draft").Value);

            var contentXElement = xElement.Element(Extensions._atomNamespace + "content");
            this.Content.Type = ContentTypeParser.Parse(contentXElement.Attribute("type").Value);
            this.Content.Text = contentXElement.Value;

            foreach (var categoryXElement in xElement.Elements(Extensions._atomNamespace + "category"))
                this.Categories.Add(categoryXElement.Attribute("term").Value);
        }

        private static string ToYesOrNo(bool value)
            => value ? "yes" : "no";

        private static bool FromYesOrNo(string value)
        {
            switch (value)
            {
                case "yes":
                    return true;
                case "no":
                    return false;
                default:
                    ThrowHelper.ReportBug();
                    throw new NotImplementedException();
            }
        }
    }
}
