using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using Discord;
using Discord.WebSocket;
using Flurl;
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
        var response = await Utilities.Request<Repository>(new Url(BaseUrl)
                .AppendPathSegments("repos", user, repository),
            Config.Bot.UserAgent);
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Failed to get repository information");
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

        string licenseField;
        if (!response.License.IsInvalid())
        {
            var license = await Utilities.Request<License>(new Url(BaseUrl)
                    .AppendPathSegments("licenses", response.License?.Key),
                Config.Bot.UserAgent);
            if (license.IsInvalid())
            {
                await Context.ErrorResponse("Failed to get license information");
                return;
            }

            licenseField = $"**Name:** {license.Name}\n" +
                           $"**Permissions:** {license.Permissions.ArrayFormat()}\n" +
                           $"**Conditions:** {license.Conditions.ArrayFormat()}\n" +
                           $"**Limitations:** {license.Limitations.ArrayFormat()}\n";
        }
        else
            licenseField = "No license";

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Repository Information__")
            .WithThumbnailUrl(response.Owner.AvatarUrl)
            .AddField(":zap: **Main**", mainField)
            .AddField(":bust_in_silhouette: **Owner**", ownerField)
            .AddField(":scroll: **License**", licenseField)
            .Build();

        var buttons = new ComponentBuilder()
            .WithButton("Repository", url: response.HtmlUrl, style: ButtonStyle.Link)
            .WithButton("Owner", url: $"https://github.com/{response.Owner.Login}", style: ButtonStyle.Link)
            .Build();

        await RespondAsync(embed: embed, components: buttons);
    }

    [SlashCommand("user", "Get information about a user")]
    public async Task UserAsync([Summary("user", "Github user")] string user)
    {
        var response = await Utilities.Request<User>(new Url(BaseUrl).AppendPathSegments("users", user),
            Config.Bot.UserAgent);
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Failed to get user information");
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

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__User Information__")
            .WithThumbnailUrl(response.AvatarUrl)
            .AddField(":zap: **Main**", mainField)
            .AddField(":bar_chart: **Statistics**", statsField)
            .Build();

        var buttons = response.TwitterUsername.IsInvalid()
            ? new ComponentBuilder()
                .WithButton("Github", url: response.HtmlUrl, style: ButtonStyle.Link)
                .Build()
            : new ComponentBuilder()
                .WithButton("Github", url: response.HtmlUrl, style: ButtonStyle.Link)
                .WithButton("Twitter", url: $"https://twitter.com/{response.TwitterUsername}", style: ButtonStyle.Link)
                .Build();

        await RespondAsync(embed: embed, components: buttons);
    }

    [SlashCommand("gist", "Get information about a gist")]
    public async Task GistAsync([Summary("user", "Github user")] string user)
    {
        var response = await Utilities.Request<Gist[]>(new Url(BaseUrl).AppendPathSegments("users", user, "gists"),
            Config.Bot.UserAgent);
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Failed to get gist information or user has no gists");
            return;
        }

        var content = response![0];

        var userField = $"**Username:** {content.Owner.Login}\n" +
                        $"**ID:** {content.Owner.Id}\n" +
                        $"**Type:** {content.Owner.Type}\n";

        var latestGistField = $"**Description:** {content.Description.OrElse("No description")}\n" +
                              $"**ID:** {content.Id}\n" +
                              $"**Comments:** {content.Comments}\n";

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Gist Information__")
            .WithThumbnailUrl(content.Owner.AvatarUrl)
            .AddField(":bust_in_silhouette: **User**", userField)
            .AddField(":1234: **Number of Gists**", response.Length.ToString())
            .AddField(":arrow_up: **Latest Gist**", latestGistField)
            .Build();

        var buttons = new ComponentBuilder()
            .WithButton("Open user Gists", url: $"https://gist.github.com/{user}", style: ButtonStyle.Link)
            .WithButton("Open user profile", url: content.Owner.HtmlUrl, style: ButtonStyle.Link)
            .WithButton("Open latest Gist", url: content.HtmlUrl, style: ButtonStyle.Link)
            .Build();

        await RespondAsync(embed: embed, components: buttons);
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Local")]
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
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
        public InternalOwner Owner { get; set; }
        public InternalLicense? License { get; set; }

        internal struct InternalOwner
        {
            public string Login { get; set; }
            public int Id { get; set; }
            public string Type { get; set; }
            [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }
        }

        internal struct InternalLicense
        {
            public string Key { get; set; }
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private struct License
    {
        public string Name { get; set; }
        public string[] Permissions { get; set; }
        public string[] Conditions { get; set; }
        public string[] Limitations { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
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

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Local")]
    private struct Gist
    {
        [JsonProperty("html_url")] public string HtmlUrl { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public int Comments { get; set; }
        public InternalOwner Owner { get; set; }

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

internal static class GithubCommandExtensions
{
    internal static string ArrayFormat(this IEnumerable<string> array)
    {
        var formatted = array
            .Select(x => x.Capitalize())
            .Select(x => x.Replace("-", " "));
        return string.Join(", ", formatted);
    }
}