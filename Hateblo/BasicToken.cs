using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hateblo.Internal;

namespace Hateblo
{
    /// <summary>
    /// Basic 認証で用いる認証情報を管理します。
    /// </summary>
    public class BasicToken : Token
    {
        private class BasicMessageHandler : HttpClientHandler
        {
            private readonly AuthenticationHeaderValue _header;

            public BasicMessageHandler(string hatenaId, string apiKey)
            {
                Debug.Assert(!string.IsNullOrEmpty(hatenaId));
                Debug.Assert(!string.IsNullOrEmpty(apiKey));

                var bytes = Encoding.ASCII.GetBytes($"{hatenaId}:{apiKey}");
                var credentials = Convert.ToBase64String(bytes);
                _header = new AuthenticationHeaderValue("Basic", credentials);
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Authorization = _header;
                return base.SendAsync(request, cancellationToken);
            }
        }

        /// <summary>
        /// 指定された認証情報を格納する、 <see cref="BasicToken"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="hatenaId">はてな ID。</param>
        /// <param name="apiKey">API キー。</param>
        /// <exception cref="ArgumentException"><paramref name="hatenaId"/> または <paramref name="apiKey"/> が空文字列です。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="hatenaId"/> または <paramref name="apiKey"/> が <see langword="null"/> です。</exception>
        public BasicToken(string hatenaId, string apiKey) : base(hatenaId)
        {
            Validation.NotNullOrEmpty(hatenaId, nameof(hatenaId));
            Validation.NotNullOrEmpty(apiKey, nameof(apiKey));

            this.ApiKey = apiKey;
            this.HttpMessageHandler = new BasicMessageHandler(hatenaId, apiKey);
        }

        /// <summary>
        /// API キーを取得します。
        /// </summary>
        public string ApiKey { get; }

        /// <summary>
        /// Basic 認証用の HTTP メッセージハンドラーを取得します。
        /// </summary>
        protected override HttpMessageHandler HttpMessageHandler { get; }
    }
}
