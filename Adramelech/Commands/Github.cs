using System.Text;
using Adramelech.Extensions;
using Adramelech.Services;
using Adramelech.Utilities;
using Discord.Interactions;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Adramelech.Commands;

[Group("github", "Get Github related information")]
public class Github : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    public const string BaseUrl = "https://api.github.com";

    [SlashCommand("repo", "Get information about a repository")]
    public async Task RepoAsync([Summary("user", "Github user")] string user,
        [Summary("repository", "Github repository")]
        string repository)
    {
        await DeferAsync();

        var response = await $"{BaseUrl}/repos/{user}/{repository}".Request<Repository>(ConfigService.UserAgent);
        if (response.IsDefault())
        {
            await Context.SendError("Failed to get repository information", true);
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

        var licenseField = response.License switch
        {
            null => "No license",
            { Key: "other" } => "Other",
            _ => await GetLicense(response.License?.Key!)
        };

        await FollowupAsync(
            embed: new EmbedBuilder()
                .WithColor(ConfigService.EmbedColor)
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

        var response = await $"{BaseUrl}/users/{user}".Request<User>(ConfigService.UserAgent);
        if (response.IsDefault())
        {
            await Context.SendError("Failed to get user information", true);
            return;
        }

        var (socials, socialsField) = await GetSocials(user);

        var mainField = $"**Username:** {response.Login}\n" +
                        $"**ID:** {response.Id}\n" +
                        $"**Type:** {response.Type}\n" +
                        $"**Name:** {response.Name ?? "N/A"}\n" +
                        $"**Company:** {response.Company ?? "N/A"}\n" +
                        $"**Website:** {response.Blog ?? "N/A"}\n" +
                        $"**Location:** {response.Location ?? "N/A"}\n" +
                        $"**Bio:** {response.Bio ?? "N/A"}\n";


        var statsField = $"**Public Repos:** {response.PublicRepos}\n" +
                         $"**Public Gists:** {response.PublicGists}\n" +
                         $"**Followers:** {response.Followers}\n" +
                         $"**Following:** {response.Following}";

        ComponentBuilder components = new();
        components.WithButton("Github", url: response.HtmlUrl, style: ButtonStyle.Link);
        socials.ForEach(x => components.WithButton(x.Provider.Capitalize(), url: x.Url, style: ButtonStyle.Link));

        await FollowupAsync(
            embed: new EmbedBuilder()
                .WithColor(ConfigService.EmbedColor)
                .WithTitle("__User Information__")
                .WithThumbnailUrl(response.AvatarUrl)
                .AddField(":zap: **Main**", mainField)
                .AddField(":bar_chart: **Statistics**", statsField)
                .AddField(":link: **Socials**", socialsField)
                .Build(),
            components: components.Build());
    }

    [SlashCommand("gist", "Get information about a gist")]
    public async Task GistAsync([Summary("user", "Github user")] string user)
    {
        await DeferAsync();

        var response = await $"{BaseUrl}/users/{user}/gists".Request<Gist[]>(ConfigService.UserAgent);
        if (response.IsDefault())
        {
            await Context.SendError("Failed to get gist information or user has no gists", true);
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
                .WithColor(ConfigService.EmbedColor)
                .WithTitle("__Gist Information__")
                .WithThumbnailUrl(content.Owner.AvatarUrl)
                .AddField(":bust_in_silhouette: **User**", userField)
                .AddField(":1234: **Number of Gists**", response.Length.ToString())
                .AddField(":arrow_up: **Latest Gist**", latestGistField)
                .Build(),
            components: new ComponentBuilder()
                .WithButton("Open user Gists", url: $"https://gist.github.com/{user}", style: ButtonStyle.Link)
                .WithButton("Open user Github", url: content.Owner.HtmlUrl, style: ButtonStyle.Link)
                .WithButton("Open latest Gist", url: content.HtmlUrl, style: ButtonStyle.Link)
                // This is just so I can get the user in the callback without having to storing it server side
                .AddRow(new ActionRowBuilder()
                    .WithButton("Get all Gists", customId: "getAllGists", style: ButtonStyle.Secondary)
                    .WithButton(content.Owner.Login, customId: "getAllGistsUser", style: ButtonStyle.Secondary,
                        disabled: true))
                .Build());
    }

    private static async Task<(List<Social>, string)> GetSocials(string user)
    {
        var response = await $"{BaseUrl}/users/{user}/social_accounts".Request<Social[]>(ConfigService.UserAgent);
        if (response.IsDefault())
            return ([], "No socials found");

        StringBuilder builder = new();

        foreach (var social in response!)
            builder.AppendLine($"**{social.Provider.Capitalize()}:** {social.Url}");

        return (response.ToList(), builder.ToString().TrimEnd('\n'));
    }

    private static async Task<string> GetLicense(string key)
    {
        var response = await $"{BaseUrl}/licenses/{key}".Request<License>(ConfigService.UserAgent);
        if (response.IsDefault())
            return "Failed to get license information";

        return $"**Name:** {response.Name}\n" +
               $"**Permissions:** {FormatArray(response.Permissions)}\n" +
               $"**Conditions:** {FormatArray(response.Conditions)}\n" +
               $"**Limitations:** {FormatArray(response.Limitations)}\n";
    }

    private static string FormatArray(IEnumerable<string> array) =>
        string.Join(", ", array
            .Select(x => x.Capitalize())
            .Select(x => x.Replace("-", " ")));

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
        [JsonProperty("public_repos")] public int PublicRepos { get; set; }
        [JsonProperty("public_gists")] public int PublicGists { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }
        [JsonProperty("html_url")] public string HtmlUrl { get; set; }
    }

    public struct Gist
    {
        [JsonProperty("html_url")] public string HtmlUrl { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public int Comments { get; set; }
        public InternalOwner Owner { get; init; }

        public struct InternalOwner
        {
            public string Login { get; set; }
            public int Id { get; set; }
            [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }
            [JsonProperty("html_url")] public string HtmlUrl { get; set; }
            public string Type { get; set; }
        }
    }

    private struct Social
    {
        public string Provider { get; set; }
        public string Url { get; set; }
    }
}

public class GithubComponents : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    [ComponentInteraction("getAllGists")]
    public async Task GetAllGists()
    {
        // Unholy syntax
        var test = Context.Interaction.Message.Components
            .FirstOrDefault(x => x.Components.Any(component => component.CustomId == "getAllGistsUser"))
            ?.Components.FirstOrDefault(x => x.CustomId == "getAllGistsUser");
        if (test is null)
        {
            await Context.SendError("Failed to get user");
            return;
        }

        var user = (test as ButtonComponent)?.Label;

        var response = await $"{Github.BaseUrl}/users/{user}/gists".Request<Github.Gist[]>(ConfigService.UserAgent);
        if (response.IsDefault())
        {
            await Context.SendError("Failed to get gist information or user has no gists");
            return;
        }

        StringBuilder builder = new();

        builder.AppendLine($"User: {user}\n" +
                           $"Number of Gists: {response?.Length}\n\n");

        foreach (var gist in response!)
            builder.AppendLine($"Description: {gist.Description.OrElse("No description")}\n" +
                               $"ID: {gist.Id}\n" +
                               $"Comments: {gist.Comments}\n" +
                               $"URL: {gist.HtmlUrl}\n");

        await Context.Interaction.UpdateAsync(p =>
        {
            // Stupid way of doing this
            p.Components = new ComponentBuilder()
                .WithButton("Open user Gists", url: $"https://gist.github.com/{user}", style: ButtonStyle.Link)
                .WithButton("Open user Github", url: response[0].Owner.HtmlUrl, style: ButtonStyle.Link)
                .WithButton("Open latest Gist", url: response[0].HtmlUrl, style: ButtonStyle.Link)
                .AddRow(new ActionRowBuilder()
                    .WithButton("Get all Gists", customId: "getAllGists", style: ButtonStyle.Secondary, disabled: true)
                    .WithButton(user, customId: "getAllGistsUser", style: ButtonStyle.Secondary, disabled: true))
                .Build();
        });

        await Context.Channel.SendFileAsync(
            stream: new MemoryStream(Encoding.UTF8.GetBytes(builder.ToString())),
            filename: "gists.txt",
            messageReference: Context.MessageReference());
    }
}