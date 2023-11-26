using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
using Discord.Interactions;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Adramelech.Commands;

[Group("github", "Get Github related information")]
public class Github : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private const string BaseUrl = "https://api.github.com";

    [SlashCommand("repo", "Get information about a repository")]
    public async Task RepoAsync([Summary("user", "Github user")] string user,
        [Summary("repository", "Github repository")]
        string repository)
    {
        await DeferAsync();

        var response = await $"{BaseUrl}/repos/{user}/{repository}".Request<Repository>(OtherConfig.UserAgent);
        if (response.IsDefault())
        {
            await Context.ErrorResponse("Failed to get repository information", true);
            return;
        }

        var mainField = $"**Name:** {response.Name}\n" +
                        $"**ID:** {response.Id}\n" +
                        $"**Description:** {response.Description}\n" +
                        $"**Is Fork:** {response.Fork}\n" +
                        $"**Main Language:** {response.Language}\n" +
                        $"**Stars:** {response.StargazersCount}\n" +
                        $"**Watchers:** {response.WatchersCount}\n" +
                        $"**Forks:** {response.ForksCount}\n";

        var ownerField = $"**Username:** {response.Owner.Login}\n" +
                         $"**ID:** {response.Owner.Id}\n" +
                         $"**Type:** {response.Owner.Type}\n";

        var licenseField = response.License is not null
            ? await GetLicense(response.License?.Key!)
            : "No license";

        await FollowupAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("__Repository Information__")
                .WithThumbnailUrl(response.Owner.AvatarUrl)
                .AddField(":zap: **Main**", mainField)
                .AddField(":bust_in_silhouette: **Owner**", ownerField)
                .AddField(":scroll: **License**", licenseField)
                .Build(),
            components: new ComponentBuilder()
                .WithButton("Repository", url: response.HtmlUrl, style: ButtonStyle.Link)
                .WithButton("Owner", url: $"https://github.com/{response.Owner.Login}", style: ButtonStyle.Link)
                .Build());
    }

    [SlashCommand("user", "Get information about a user")]
    public async Task UserAsync([Summary("user", "Github user")] string user)
    {
        await DeferAsync();

        var response = await $"{BaseUrl}/users/{user}".Request<User>(OtherConfig.UserAgent);
        if (response.IsDefault())
        {
            await Context.ErrorResponse("Failed to get user information", true);
            return;
        }

        var mainField = $"**Username:** {response.Login}\n" +
                        $"**ID:** {response.Id}\n" +
                        $"**Type:** {response.Type}\n" +
                        $"**Name:** {response.Name.OrElse("N/A")}\n" +
                        $"**Company:** {response.Company.OrElse("N/A")}\n" +
                        $"**Website:** {response.Blog.OrElse("N/A")}\n" +
                        $"**Location:** {response.Location.OrElse("N/A")}\n" +
                        $"**Bio:** {response.Bio.OrElse("N/A")}\n" +
                        $"**Twitter:** {response.TwitterUsername.OrElse("N/A")}";


        var statsField = $"**Public Repos:** {response.PublicRepos}\n" +
                         $"**Public Gists:** {response.PublicGists}\n" +
                         $"**Followers:** {response.Followers}\n" +
                         $"**Following:** {response.Following}";

        await FollowupAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("__User Information__")
                .WithThumbnailUrl(response.AvatarUrl)
                .AddField(":zap: **Main**", mainField)
                .AddField(":bar_chart: **Statistics**", statsField)
                .Build(),
            components: response.TwitterUsername.IsNullOrEmpty()
                ? new ComponentBuilder()
                    .WithButton("Github", url: response.HtmlUrl, style: ButtonStyle.Link)
                    .Build()
                : new ComponentBuilder()
                    .WithButton("Github", url: response.HtmlUrl, style: ButtonStyle.Link)
                    .WithButton("Twitter", url: $"https://twitter.com/{response.TwitterUsername}",
                        style: ButtonStyle.Link)
                    .Build());
    }

    [SlashCommand("gist", "Get information about a gist")]
    public async Task GistAsync([Summary("user", "Github user")] string user)
    {
        await DeferAsync();

        var response = await $"{BaseUrl}/users/{user}/gists".Request<Gist[]>(OtherConfig.UserAgent);
        if (response.IsDefault())
        {
            await Context.ErrorResponse("Failed to get gist information or user has no gists", true);
            return;
        }

        var content = response![0];

        var userField = $"**Username:** {content.Owner.Login}\n" +
                        $"**ID:** {content.Owner.Id}\n" +
                        $"**Type:** {content.Owner.Type}";

        var latestGistField = $"**Description:** {content.Description.OrElse("No description")}\n" +
                              $"**ID:** {content.Id}\n" +
                              $"**Comments:** {content.Comments}";

        await FollowupAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("__Gist Information__")
                .WithThumbnailUrl(content.Owner.AvatarUrl)
                .AddField(":bust_in_silhouette: **User**", userField)
                .AddField(":1234: **Number of Gists**", response.Length.ToString())
                .AddField(":arrow_up: **Latest Gist**", latestGistField)
                .Build(),
            components: new ComponentBuilder()
                .WithButton("Open user Gists", url: $"https://gist.github.com/{user}", style: ButtonStyle.Link)
                .WithButton("Open user profile", url: content.Owner.HtmlUrl, style: ButtonStyle.Link)
                .WithButton("Open latest Gist", url: content.HtmlUrl, style: ButtonStyle.Link)
                .Build());
    }

    private static async Task<string> GetLicense(string key)
    {
        var response = await $"{BaseUrl}/licenses/{key}".Request<License>(OtherConfig.UserAgent);
        if (response.IsDefault())
            return "No license";

        return $"**Name:** {response.Name}\n" +
               $"**Permissions:** {FormatArray(response.Permissions)}\n" +
               $"**Conditions:** {FormatArray(response.Conditions)}\n" +
               $"**Limitations:** {FormatArray(response.Limitations)}\n";
    }

    private static string FormatArray(IEnumerable<string> array)
    {
        var formatted = array
            .Select(x => x.Capitalize())
            .Select(x => x.Replace("-", " "));
        return string.Join(", ", formatted);
    }

    private struct Repository
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonProperty("html_url")] public string HtmlUrl { get; set; }
        public string Description { get; set; }
        public bool Fork { get; set; }
        public string Language { get; set; }
        [JsonProperty("stargazers_count")] public int StargazersCount { get; set; }
        [JsonProperty("watchers_count")] public int WatchersCount { get; set; }
        [JsonProperty("forks_count")] public int ForksCount { get; set; }
        public InternalOwner Owner { get; init; }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public InternalLicense? License { get; set; }

        internal struct InternalOwner
        {
            public string Login { get; set; }
            public int Id { get; set; }
            public string Type { get; set; }
            [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }
        }

        // ReSharper disable once MemberCanBePrivate.Local
        internal struct InternalLicense
        {
            public string Key { get; set; }
        }
    }

    private struct License
    {
        public string Name { get; set; }
        public string[] Permissions { get; set; }
        public string[] Conditions { get; set; }
        public string[] Limitations { get; set; }
    }

    private struct User
    {
        public string Login { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }
        public string? Name { get; set; }
        public string? Company { get; set; }
        public string? Blog { get; set; }
        public string? Location { get; set; }
        public string? Bio { get; set; }
        [JsonProperty("twitter_username")] public string? TwitterUsername { get; set; }
        [JsonProperty("public_repos")] public int PublicRepos { get; set; }
        [JsonProperty("public_gists")] public int PublicGists { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }
        [JsonProperty("html_url")] public string HtmlUrl { get; set; }
    }

    private struct Gist
    {
        [JsonProperty("html_url")] public string HtmlUrl { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public int Comments { get; set; }
        public InternalOwner Owner { get; init; }

        internal struct InternalOwner
        {
            public string Login { get; set; }
            public int Id { get; set; }
            [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }
            [JsonProperty("html_url")] public string HtmlUrl { get; set; }
            public string Type { get; set; }
        }
    }
}