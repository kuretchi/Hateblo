using System;
using System.Collections.Generic;
using System.Text;

namespace Hateblo
{
    /// <summary>
    /// ブログエントリの本文を表します。
    /// </summary>
    public class Content
    {
        /// <summary>
        /// ブログエントリの本文の編集モードを取得します。
        /// </summary>
        public ContentType Type { get; internal set; }

        /// <summary>
        /// ブログエントリの本文を取得または設定します。
        /// </summary>
        public string Text { get; set; }
    }
}
