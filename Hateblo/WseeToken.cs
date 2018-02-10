using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hateblo.Internal;

namespace Hateblo
{
    /// <summary>
    /// WSEE 認証で用いる認証情報を管理します。
    /// </summary>
    public class WseeToken : Token
    {
        private class WseeMessageHandler : HttpClientHandler
        {
            private static readonly RandomNumberGenerator _rand
                = RandomNumberGenerator.Create();

            private const int _nonceLength = 40;

            private readonly string _hatenaId, _apiKey;

            public WseeMessageHandler(string hatenaId, string apiKey)
            {
                Debug.Assert(!string.IsNullOrEmpty(hatenaId));
                Debug.Assert(!string.IsNullOrEmpty(apiKey));

                _hatenaId = hatenaId;
                _apiKey = apiKey;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var buffer = new byte[_nonceLength];
                _rand.GetBytes(buffer);
                var nonce = Convert.ToBase64String(buffer);
                var created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

                string digest;
                using (var sha1 = SHA1.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(nonce + created + _apiKey);
                    digest = Convert.ToBase64String(sha1.ComputeHash(bytes));
                }

                var credentials
                    = "UsernameToken "
                    + $"Username=\"{_hatenaId}\", "
                    + $"PasswordDigest=\"{digest}\", "
                    + $"Nonce=\"{nonce}\", "
                    + $"Created=\"{created}\"";

                request.Headers.Add("X-WSSE", credentials);

                return base.SendAsync(request, cancellationToken);
            }
        }

        /// <summary>
        /// 指定された認証情報を格納する、 <see cref="WseeToken"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="hatenaId">はてな ID。</param>
        /// <param name="apiKey">API キー。</param>
        /// <exception cref="ArgumentException"><paramref name="hatenaId"/> または <paramref name="apiKey"/> が空文字列です。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="hatenaId"/> または <paramref name="apiKey"/> が <see langword="null"> です。</see></exception>
        public WseeToken(string hatenaId, string apiKey) : base(hatenaId)
        {
            Validation.NotNullOrEmpty(hatenaId, nameof(hatenaId));
            Validation.NotNullOrEmpty(apiKey, nameof(apiKey));

            this.ApiKey = apiKey;
            this.HttpMessageHandler = new WseeMessageHandler(hatenaId, apiKey);
        }

        /// <summary>
        /// API キーを取得します。
        /// </summary>
        public string ApiKey { get; }

        /// <summary>
        /// WSEE 認証用の HTTP メッセージハンドラーを取得します。
        /// </summary>
        protected override HttpMessageHandler HttpMessageHandler { get; }
    }
}
