using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Hateblo.Internal;

namespace Hateblo
{
    /// <summary>
    /// ブログエントリの本文の編集モードを表します。
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        /// ブログエントリが投稿されていないため、未確定であることを表します。
        /// </summary>
        Unknown,

        /// <summary>
        /// 見たままモードを表します。
        /// </summary>
        Html,

        /// <summary>
        /// はてな記法を表します。
        /// </summary>
        HatenaSyntax,

        /// <summary>
        /// マークダウン記法を表します。
        /// </summary>
        Markdown,
    }

    internal static class ContentTypeParser
    {
        internal static ContentType Parse(string input)
        {
            switch (input)
            {
                case "text/html":
                    return ContentType.Html;
                case "text/x-hatena-syntax":
                    return ContentType.HatenaSyntax;
                case "text/x-markdown":
                    return ContentType.Markdown;
                default:
                    ThrowHelper.ReportBug(input);
                    throw new NotImplementedException();
            }
        }
    }
}
