# Hateblo [![AppVeyor](https://img.shields.io/appveyor/ci/kuretchi/Hateblo.svg?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/kuretchi/hateblo) [![NuGet](https://img.shields.io/nuget/v/Hateblo.svg?style=flat-square)](https://www.nuget.org/packages/Hateblo) [![license](https://img.shields.io/github/license/kuretchi/Hateblo.svg?style=flat-square)](https://github.com/kuretchi/Hateblo/blob/master/LICENSE.txt)

[はてなブログ AtomPub](http://developer.hatena.ne.jp/ja/documents/blog/apis/atom) をラップする .NET Standard 1.3 ライブラリです。

```csharp
/* OAuth 認証 */
var session = await OAuth.AuthorizeAsync(consumerKey, consumerSecret);  // アプリケーションの認証
Process.Start(session.AuthorizationUri);                                // ブラウザで認証ページを開く
var verifier = Console.ReadLine();                                      // 確認コードをコンソールから入力
var token = await session.GetTokenAsync(verifier);                      // 確認コードからアクセストークンを取得
var blog = token.GetBlog(blogId);                                       // ブログ ID で操作対象のブログを指定

/* ブログエントリ一覧の取得 */
blog.GetObservableEntries()                                             // IObservable<Entry>
    .Where(entry => entry.IsDraft && entry.Categories.Contains("C#"))   // カテゴリに C# が含まれている下書き
    .Subscribe(entry =>
    {
        Console.WriteLine($"# {entry.Title} {entry.UpdateTime}");
        Console.WriteLine(entry.Summary);                               // タイトルと更新日時、要約をコンソールに出力
    });

/* ブログエントリの新規投稿 */
await blog.PostAsync(new Entry
{
    Title = "Hateblo を公開しました",
    Categories = { "はてなブログ", "AtomPub", "C#", ".NET Standard" },
    Content = { Text = "はてなブログ AtomPub をラップする .NET Standard 1.3 ライブラリです。" },
});
```

## 機能

- ブログエントリ一覧の取得
- ブログエントリの新規投稿 / 取得 / 更新 / 削除
- カテゴリ一覧の取得

## 導入

~~NuGet から [Hateblo](https://www.nuget.org/packages/Hateblo) をインストールしてください。~~ 近日公開予定です。

## 使い方

### 認証

OAuth 認証、WSSE 認証、Basic 認証に対応しています。通常は OAuth 認証をおすすめします。

#### OAuth 認証

あらかじめ [アプリケーション登録](http://developer.hatena.ne.jp/ja/documents/auth/apis/oauth/consumer) が必要です。「アプリケーションを登録して consumer key を取得する」を参考にして、Consumer Key と Consumer Secret を取得しておいてください。

はてなブログ AtomPub では、すべての操作で [read\_private と write\_private の scope](http://developer.hatena.ne.jp/ja/documents/auth/apis/oauth/scopes#read_private) が必要です。これらが許可されていない場合は認証に失敗します。

新規にアクセストークンを取得する場合は、次のようにします。

```csharp
// リクエストトークンを取得
var session = await OAuth.AuthorizeAsync(consumerKey, consumerSecret);

// 確認コードを取得
// 例 (ブラウザを起動し、標準入力から取得):
//   Process.Start(session.AuthorizationUri);
//   var verifier = Console.ReadLine();

// アクセストークンを取得
var token = await session.GetTokenAsync(verifier);
var blog = token.GetBlog(blogId);
```

`Token.GetBlog` メソッドが要求するブログ ID には、ブログのドメイン (例：`"kuretchi.hateblo.jp"`) を指定してください。

OAuth 認証で取得したアクセストークンは、`Token` インスタンスを `DataContractSerializer` 等でシリアライズするか、`Token.AccessToken` と `Token.AcceessTokenSecret` を保存しておくことで再利用できます。

後者の場合、`Token` インスタンスを再び生成するには次のようにします。

```csharp
var token = new Token(consumerKey, consumerSecret, accessToken, accessTokenSecret);
```

#### WSEE 認証

はてな ID と API キーを用いた認証方式です。API キーはブログの詳細設定から確認できます。

```csharp
var token = new WseeToken(hatenaId, apiKey);
var blog = token.GetBlog(blogId);
```

#### Basic 認証

はてな ID と API キーを用いた簡易な認証方式です。

```csharp
var token = new BasicToken(hatenaId, apiKey);
var blog = token.GetBlog(blogId);
```

### ブログエントリの取得

#### 一覧の取得

ブログエントリの一覧は、`GetEntries` メソッドで取得できます。

```csharp
var entries = blog.GetEntries();  // IEnumerable<Entry>
foreach (var entry in entries) Console.WriteLine(entry.Title);
```

`GetObservableEntries` メソッドを用いて、非同期で取得することもできます。戻り値は `IObservable<Entry>` 型で、[Reactive Extensions](https://github.com/Reactive-Extensions/Rx.NET) を活用することができます。

```csharp
var entries = blog.GetObaservableEntries();  // IObservable<Entry>
entries.Subscribe(entry => Console.WriteLine(entry.Title));
```

ブログエントリは更新日時 (`Entry.UpdateTime`) が新しい順に列挙されます。

これらのメソッドは、取得されるブログエントリ数に比例する回数のリクエストをサーバーに送信します。サーバーへの負荷を軽減するため、規定ではリクエスト毎に 1 秒間の待機を行います。この待機時間を指定するために、`TimeSpan` を受け取るオーバーロードが用意されています。

```csharp
var entries = blog.GetObservableEntries(TimeSpan.FromMilliseconds(500));  // リクエスト毎に 500 ms の間隔を空ける
```

#### ID からの取得

ブログエントリは ID からも取得できます。ブログエントリ ID は `Entry.Id` プロパティから取得できます。

```csharp
var entry = await blog.GetEntryAsync(entryId);
Console.WriteLine(entry.Title);
```

### ブログエントリの新規投稿

`Entry` クラスのインスタンスを作成し、`PostAsync` メソッドで投稿します。投稿に成功すると、同時に取得できる `Entry.Id` などのプロパティがすべて設定されます。

```csharp
var entry = new Entry
{
    Title = "ブログタイトル",
    Content = { Text = "ブログエントリの新規投稿のテストです。" },
    Categories = { "サンプル", "テスト" },
};

await blog.PostAsync(entry);
```

### ブログエントリの更新

取得したブログエントリを編集し、`UpdateAsync` メソッドで更新します。

```csharp
entry.Title += " (MM/DD 追記)";
entry.Content += " \nほげほげ";
entry.Categories.Remove("サンプル");
entry.Categories.Add("sample");

await blog.UpdateAsync(entry);
```

更新日時をその時点の日時に変更するには、`Entry.UpdateTime` に現在時刻を設定しておくか、もしくは `null` を設定すると更新時のサーバーの日時が自動的に設定されます。

```csharp
entry.UpdateTime = new HatenaDateTime(DateTimeOffset.Now);  // 現在時刻を設定
entry.UpdateTime = null;  // 更新時のサーバーの日時を自動で設定
```

### ブログエントリの削除

`RemoveAsync` メソッドで削除します。

```csharp
await blog.RemoveAsync(entry);
```

ブログエントリ ID を指定して削除することもできます。

```csharp
await blog.RemoveAsync(entryId);
```

### カテゴリ一覧の取得

`GetCategories` メソッド、または非同期の `GetObservableCategories` メソッドで取得できます。

```csharp
var categories = blog.GetCategories();  // IEnumerable<string>
var observableCategories = blog.GetObservableCategories();  // IObservable<string>
```

## 貢献

- 不具合報告やご提案は [Issues](https://github.com/kuretchi/Hateblo/issues) までお気軽にどうぞ。
- [Pull Request](https://github.com/kuretchi/Hateblo/pulls) も歓迎します。

## ライセンス

- このライブラリの主要な部分は [MIT License](https://github.com/kuretchi/Hateblo/blob/master/LICENSE.txt) です。
- [neuecc/AsyncOAuth](https://github.com/neuecc/AsyncOAuth) のコードを一部利用しており、これは [MIT License](https://opensource.org/licenses/MIT) に従います。
