using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hateblo.Internal;

namespace Hateblo
{
    /// <summary>
    /// ブログを表します。
    /// </summary>
    public partial class Blog
    {
        private readonly Lazy<HttpClient> _client;
        private readonly string _collectionUri, _categoryUri;
        private bool _hasCurrentFeed;
        private Feed _currentFeed;

        /// <summary>
        /// 指定された認証情報とブログ ID を保持する、 <see cref="Blog"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="token">認証情報。</param>
        /// <param name="blogId">ブログ ID。</param>
        /// <exception cref="ArgumentException"><paramref name="blogId"/> が空文字列です。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> または <paramref name="blogId"/> が <see langword="null"/> です。</exception>
        public Blog(Token token, string blogId)
        {
            Validation.NotNull(token, nameof(token));
            Validation.NotNullOrEmpty(blogId, nameof(blogId));

            _client = token._client;
            _collectionUri = $"https://blog.hatena.ne.jp/{token.HatenaId}/{blogId}/atom/entry";
            _categoryUri = $"https://blog.hatena.ne.jp/{token.HatenaId}/{blogId}/atom/category";
            this.HatenaId = token.HatenaId;
            this.BlogId = blogId;
        }

        /// <summary>
        /// はてな ID を取得します。
        /// </summary>
        public string HatenaId { get; }

        /// <summary>
        /// ブログ ID を取得します。
        /// </summary>
        public string BlogId { get; }

        /// <summary>
        /// ブログエントリの一覧を取得します。
        /// </summary>
        /// <returns>ブログエントリの一覧を列挙する、 <see cref="IEnumerable{T}"/> 型の列挙子。</returns>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public IEnumerable<Entry> GetEntries()
            => GetObservableEntries().ToEnumerable();

        /// <summary>
        /// サーバーへのリクエストの間隔が指定された時間を下回らないように、ブログエントリの一覧を取得します。
        /// </summary>
        /// <param name="period">サーバーへのリクエストの最小間隔。</param>
        /// <returns>ブログエントリの一覧を列挙する、 <see cref="IEnumerable{T}"/> 型の列挙子。</returns>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public IEnumerable<Entry> GetEntries(TimeSpan period)
            => GetObservableEntries(period).ToEnumerable();

        /// <summary>
        /// ブログエントリの一覧を非同期で取得します。
        /// </summary>
        /// <returns>ブログエントリの一覧を非同期で列挙する、 <see cref="IObservable{T}"/> 型の通知プロバイダー。</returns>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public IObservable<Entry> GetObservableEntries()
            => GetObservableEntries(TimeSpan.FromSeconds(1));

        /// <summary>
        /// サーバーへのリクエストの間隔が指定された時間を下回らないように、ブログエントリの一覧を非同期で取得します。
        /// </summary>
        /// <param name="period">サーバーへのリクエストの最小間隔。</param>
        /// <returns>ブログエントリの一覧を非同期で列挙する、 <see cref="IObservable{T}"/> 型の通知プロバイダー。</returns>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public IObservable<Entry> GetObservableEntries(TimeSpan period)
        {
            return Observable.Defer(async () =>
            {
                var requestUri = !_hasCurrentFeed ? _collectionUri : _currentFeed.NextRequestUri;
                var response = await _client.Value.GetAsync(requestUri).ConfigureAwait(false);
                VerifyResponse(response);
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var xElement = XElement.Load(stream);
                _currentFeed = Feed.FromXElement(xElement);
                _hasCurrentFeed = true;
                return Observable.Return(_currentFeed);
            })
            .TimeInterval()
            .Select(ti => Observable.FromAsync(async () =>
            {
                Debug.WriteLine(ti.Interval);
                var delay = Extensions.Max(TimeSpan.Zero, period - ti.Interval);
                await Task.Delay(delay).ConfigureAwait(false);
                return ti.Value;
            }))
            .Concat()
            .SelectMany(f => f)
            .DoWhile(() => _currentFeed.NextRequestUri != null)
            .Finally(() =>
            {
                _currentFeed = null;
                _hasCurrentFeed = false;
            });
        }

        /// <summary>
        /// 指定された ID のブログエントリを取得します。
        /// </summary>
        /// <param name="entryId">ブログエントリ ID。</param>
        /// <returns>ブログエントリを返す非同期操作を表す <see cref="Task{TResult}"/> オブジェクト。</returns>
        /// <exception cref="ArgumentException"><paramref name="entryId"/> が空文字列です。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entryId"/> が <see langword="null"/> です。</exception>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public async Task<Entry> GetEntryAsync(string entryId)
        {
            Validation.NotNullOrEmpty(entryId, nameof(entryId));

            var memberUri = GetMemberUri(entryId);
            var response = await _client.Value.GetAsync(memberUri).ConfigureAwait(false);
            var entry = new Entry();
            await LoadAsync(entry, response).ConfigureAwait(false);
            return entry;
        }

        /// <summary>
        /// 指定されたブログエントリを新規投稿します。
        /// </summary>
        /// <param name="entry">新規投稿するブログエントリ。</param>
        /// <returns>非同期操作を表す <see cref="Task"/> オブジェクト。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> が <see langword="null"/> です。</exception>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public async Task PostAsync(Entry entry)
        {
            Validation.NotNull(entry, nameof(entry));

            await SendAsync(entry, HttpMethod.Post, _collectionUri).ConfigureAwait(false);
        }

        /// <summary>
        /// 指定されたブログエントリを更新します。
        /// </summary>
        /// <param name="entry">更新するブログエントリ。</param>
        /// <returns>非同期操作を表す <see cref="Task"/> オブジェクト。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> が <see langword="null"/> です。</exception>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public async Task UpdateAsync(Entry entry)
        {
            Validation.NotNull(entry, nameof(entry));

            await SendAsync(entry, HttpMethod.Put, GetMemberUri(entry)).ConfigureAwait(false);
        }

        /// <summary>
        /// 指定されたブログエントリを削除します。
        /// </summary>
        /// <param name="entry">削除するブログエントリ。</param>
        /// <returns>非同期操作を表す <see cref="Task"/> オブジェクト。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> が <see langword="null"/> です。</exception>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public async Task RemoveAsync(Entry entry)
        {
            Validation.NotNull(entry, nameof(entry));

            var response = await _client.Value.DeleteAsync(GetMemberUri(entry)).ConfigureAwait(false);
            VerifyResponse(response);
        }

        /// <summary>
        /// 指定された ID のブログエントリを削除します。
        /// </summary>
        /// <param name="entryId">削除するブログエントリ ID。</param>
        /// <returns>非同期操作を表す <see cref="Task"/> オブジェクト。</returns>
        /// <exception cref="ArgumentException"><paramref name="entryId"/> が空文字列です。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entryId"/> が <see langword="null"/> です。</exception>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public async Task RemoveAsync(string entryId)
        {
            Validation.NotNullOrEmpty(entryId, nameof(entryId));

            var response = await _client.Value.DeleteAsync(GetMemberUri(entryId)).ConfigureAwait(false);
            VerifyResponse(response);
        }

        /// <summary>
        /// カテゴリの一覧を取得します。
        /// </summary>
        /// <returns>カテゴリの一覧を列挙する、 <see cref="IEnumerable{T}"/> 型の列挙子。</returns>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public IEnumerable<string> GetCategories()
            => GetObservableCategories().ToEnumerable();

        /// <summary>
        /// カテゴリの一覧を非同期で取得します。
        /// </summary>
        /// <returns>カテゴリの一覧を非同期で列挙する、 <see cref="IObservable{T}"/> 型の通知プロバイダー。</returns>
        /// <exception cref="ResourceNotFoundException">存在しないリソースにアクセスしました。</exception>
        /// <exception cref="InternalServerErrorException">はてなブログ AtomPub で問題が発生しました。</exception>
        /// <exception cref="HttpRequestException">HTTP リクエストに失敗しました。</exception>
        public IObservable<string> GetObservableCategories()
        {
            return Observable.FromAsync(async () =>
            {
                var response = await _client.Value.GetAsync(_categoryUri).ConfigureAwait(false);
                VerifyResponse(response);
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var xElement = XElement.Load(stream);
                return Category.FromXElement(xElement);
            })
            .SelectMany(cs => cs);
        }

        private async Task SendAsync(Entry entry, HttpMethod method, string requestUri)
        {
            var xml = entry.ToXDocument().ToString(SaveOptions.DisableFormatting);
            var content = new StringContent(xml, Encoding.UTF8, "application/xml");
            var request = new HttpRequestMessage(method, requestUri) { Content = content };
            var response = await _client.Value.SendAsync(request).ConfigureAwait(false);
            VerifyResponse(response);
            await LoadAsync(entry, response).ConfigureAwait(false);
        }

        private async Task LoadAsync(Entry entry, HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var xElement = XElement.Load(stream);
            entry.FromXElement(xElement);
        }

        private string GetMemberUri(Entry entry)
            => entry.MemberUri ?? (entry.MemberUri = GetMemberUri(entry.Id));

        private string GetMemberUri(string entryId)
            => $"{_collectionUri}/{entryId}";

        private static void VerifyResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException();

                case HttpStatusCode.InternalServerError:
                    throw new InternalServerErrorException();

                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.MethodNotAllowed:
                default:
                    ThrowHelper.ReportBug(response.StatusCode.ToString());
                    throw new NotImplementedException();
            }
        }
    }
}
