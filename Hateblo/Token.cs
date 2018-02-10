using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Hateblo.Internal;

namespace Hateblo
{
    /// <summary>
    /// 認証情報を管理するための基本クラスを表します。このクラスは抽象クラスです。
    /// </summary>
    public abstract class Token
    {
        internal readonly Lazy<HttpClient> _client;

        /// <summary>
        /// 指定されたはてな ID を格納した、 <see cref="Token"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="hatenaId">はてな ID。</param>
        /// <exception cref="ArgumentException"><paramref name="hatenaId"/> が空文字列です。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="hatenaId"/> が <see langword="null"/> です。</exception>
        public Token(string hatenaId)
        {
            Validation.NotNullOrEmpty(hatenaId, nameof(hatenaId));

            _client = new Lazy<HttpClient>(() => new HttpClient(this.HttpMessageHandler));
            this.HatenaId = hatenaId;
        }

        /// <summary>
        /// はてな ID を取得します。
        /// </summary>
        public string HatenaId { get; }

        /// <summary>
        /// 派生クラスでオーバーライドされた場合、認証用の HTTP メッセージハンドラーを取得します。
        /// </summary>
        protected abstract HttpMessageHandler HttpMessageHandler { get; }

        /// <summary>
        /// 指定された ID のブログを取得します。
        /// </summary>
        /// <param name="blogId">ブログ ID。</param>
        /// <returns>指定された ID のブログを表す <see cref="Blog"/> オブジェクト。</returns>
        public Blog GetBlog(string blogId)
            => new Blog(this, blogId);
    }
}
