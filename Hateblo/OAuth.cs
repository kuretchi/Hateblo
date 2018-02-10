using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AsyncOAuth;
using Hateblo.Internal;

namespace Hateblo
{
    // todo: ドキュメントコメント
    public static class OAuth
    {
        static OAuth()
        {
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                using (var hmac = new HMACSHA1(key))
                {
                    return hmac.ComputeHash(buffer);
                }
            };
        }

        private static readonly string _requestTokenUri
            = "https://www.hatena.com/oauth/initiate";

        private static readonly string _accessTokenUri
            = "https://www.hatena.com/oauth/token";

        private static readonly Dictionary<string, string> _parameters
            = new Dictionary<string, string>
            {
                { "oauth_callback", "oob" },
            };

        private static readonly Dictionary<string, string> _postContents
            = new Dictionary<string, string>
            {
                { "scope", "read_private,write_private" },
            };

        /// <summary>
        /// アプリケーション認証用の URI を取得します。
        /// </summary>
        /// <param name="consumerKey">アプリケーションの Consumer Key。</param>
        /// <param name="consumerSecret">アプリケーションの Consumer Secret。</param>
        /// <returns>アプリケーション認証用の URI を返す非同期操作を表す <see cref="Task{TResult}"/> オブジェクト。</returns>
        /// <exception cref="ArgumentException"><paramref name="consumerKey"/> または <paramref name="consumerSecret"/> が空文字列です。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="consumerKey"/> または <paramref name="consumerSecret"/> が <see langword="null"/> です。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public async static Task<Session> AuthorizeAsync(string consumerKey, string consumerSecret)
        {
            Validation.NotNullOrEmpty(consumerKey, nameof(consumerKey));
            Validation.NotNullOrEmpty(consumerSecret, nameof(consumerSecret));

            var authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);
            var tokenResponse = await authorizer.GetRequestToken(_requestTokenUri, _parameters, new FormUrlEncodedContent(_postContents));
            var requestToken = tokenResponse.Token;

            return new Session(authorizer, requestToken, consumerKey, consumerSecret);
        }

        /// <summary>
        /// アプリケーション認証用の URI を格納する認証セッションを表します。
        /// </summary>
        public class Session
        {
            private static readonly string _authorizationUri
                = "https://www.hatena.ne.jp/oauth/authorize";

            private readonly OAuthAuthorizer _authorizer;
            private readonly RequestToken _requestToken;

            internal Session(OAuthAuthorizer authorizer, RequestToken requestToken, string consumerKey, string consumerSecret)
            {
                Debug.Assert(authorizer != null);
                Debug.Assert(requestToken != null);
                Debug.Assert(!string.IsNullOrEmpty(consumerKey));
                Debug.Assert(!string.IsNullOrEmpty(consumerSecret));

                _authorizer = authorizer;
                _requestToken = requestToken;
                this.AuthorizationUri = authorizer.BuildAuthorizeUrl(_authorizationUri, requestToken);
                this.ConsumerKey = consumerKey;
                this.ConsumerSecret = consumerSecret;
            }

            /// <summary>
            /// アプリケーションの Consumer Key を取得します。
            /// </summary>
            public string ConsumerKey { get; }

            /// <summary>
            /// アプリケーションの Consumer Secret を取得します。
            /// </summary>
            public string ConsumerSecret { get; }

            /// <summary>
            /// アプリケーション認証用の URI を取得します。
            /// </summary>
            public string AuthorizationUri { get; }

            /// <summary>
            /// アプリケーション認証用の確認コードを使用して、アクセストークンを取得します。
            /// </summary>
            /// <param name="verifier">アプリケーション認証用の確認コード。</param>
            /// <returns>アクセストークンを返す非同期操作を表す <see cref="Task{TResult}"/> オブジェクト。</returns>
            /// <exception cref="ArgumentException"><paramref name="verifier"/> が空文字列です。</exception>
            /// <exception cref="ArgumentNullException"><paramref name="verifier"/> が <see langword="null"/> です。</exception>
            /// <exception cref="HttpRequestException">認証、もしくは HTTP リクエストに失敗しました。</exception>
            public async Task<OAuthToken> GetTokenAsync(string verifier)
            {
                Validation.NotNullOrEmpty(verifier, nameof(verifier));

                var accessTokenResponse = await _authorizer.GetAccessToken(_accessTokenUri, _requestToken, verifier);
                var accessToken = accessTokenResponse.Token;
                var hatenaId = accessTokenResponse.ExtraData["url_name"].Single();
                
                return new OAuthToken(this.ConsumerKey, this.ConsumerSecret, accessToken.Key, accessToken.Secret, hatenaId);
            }
        }
    }
}
