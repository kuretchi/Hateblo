using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AsyncOAuth;
using Hateblo.Internal;

namespace Hateblo
{
    /// <summary>
    /// OAuth 認証で用いる認証情報を管理します。
    /// </summary>
    public partial class OAuthToken : Token
    {
        static OAuthToken()
        {
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                using (var hmac = new HMACSHA1(key))
                {
                    return hmac.ComputeHash(buffer);
                }
            };
        }

        /// <summary>
        /// 指定したアクセストークンを格納する、<see cref="OAuthToken"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="consumerKey">アプリケーションの Consumer Key。</param>
        /// <param name="consumerSecret">アプリケーションの Consumer Secret。</param>
        /// <param name="accessToken">OAuth 認証用の Access Token。</param>
        /// <param name="accessTokenSecret">OAuth 認証用の Access Token Secret。</param>
        /// <param name="hatenaId">はてな ID。</param>
        /// <exception cref="ArgumentException"><paramref name="consumerKey"/>、<paramref name="consumerSecret"/>、<paramref name="accessToken"/>、<paramref name="accessTokenSecret"/>、または <paramref name="hatenaId"/> が空文字列です。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="consumerKey"/>、<paramref name="consumerSecret"/>、<paramref name="accessToken"/>、<paramref name="accessTokenSecret"/>、または <paramref name="hatenaId"/> が <see langword="null"/> です。</exception>
        public OAuthToken(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string hatenaId)
            : base(hatenaId)
        {
            Validation.NotNullOrEmpty(consumerKey, nameof(consumerKey));
            Validation.NotNullOrEmpty(consumerSecret, nameof(consumerSecret));
            Validation.NotNullOrEmpty(accessToken, nameof(accessToken));
            Validation.NotNullOrEmpty(accessTokenSecret, nameof(accessTokenSecret));
            Validation.NotNullOrEmpty(hatenaId, nameof(hatenaId));

            this.ConsumerKey = consumerKey;
            this.ConsumerSecret = consumerSecret;
            this.AccessToken = accessToken;
            this.AccessTokenSecret = accessTokenSecret;

            var token = new AccessToken(accessToken, accessTokenSecret);
            this.HttpMessageHandler = new OAuthMessageHandler(consumerKey, consumerSecret, token);
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
        /// OAuth 認証用の Access Token を取得します。
        /// </summary>
        public string AccessToken { get; }

        /// <summary>
        /// OAuth 認証用の Access Token Secret を取得します。
        /// </summary>
        public string AccessTokenSecret { get; }

        /// <summary>
        /// OAuth 認証用の HTTP メッセージハンドラーを取得します。
        /// </summary>
        protected override HttpMessageHandler HttpMessageHandler { get; }
    }
}
