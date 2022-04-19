using Microsoft.Extensions.Hosting;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using TwitterSharp.Client;
using TwitterSharp.Request;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Rule;

namespace EnvisionBot.Services;

public class TwitterRepostService : BackgroundService
{
    private readonly TwitterClient _twitterClient;

    private readonly IDiscordRestChannelAPI _discordRestChannelApi;

    public TwitterRepostService(TwitterClient twitterClient, IDiscordRestChannelAPI discordRestChannelApi)
    {
        _twitterClient = twitterClient;
        _discordRestChannelApi = discordRestChannelApi;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tweetStreamInfo = await _twitterClient.GetInfoTweetStreamAsync();
        await _twitterClient.DeleteTweetStreamAsync(tweetStreamInfo.Select(val => val.Id).ToArray());
        await _twitterClient.AddTweetStreamAsync(new StreamRequest(Expression.Author("archillect")));
        await _twitterClient.NextTweetStreamAsync((tweet) =>
        {
            var embed = new Embed(
                Description: $"{tweet.Text}",
                Author: new EmbedAuthor(
                    $"@{tweet.Author.Username}", 
                    tweet.Author.Url, 
                    tweet.Author.ProfileImageUrl
                ),
                Footer: new EmbedFooter(tweet.Source),
                Image: tweet.Attachments.Media == null || tweet.Attachments.Media.Length == 0
                    ? new Optional<IEmbedImage>()
                    : new EmbedImage(tweet.Attachments.Media.First().Url)
            );
            Console.WriteLine(tweet.Author.Url);
            _discordRestChannelApi.CreateMessageAsync(new Snowflake(965996569226772500), embeds: new[] {embed}, ct: stoppingToken);
        },
        new TweetSearchOptions
        {
            
            UserOptions = new [] {UserOption.Profile_Image_Url, UserOption.Url},
            MediaOptions = new [] {MediaOption.Url, MediaOption.Preview_Image_Url},
            TweetOptions = new [] {TweetOption.Attachments, TweetOption.Source}
        });
    }
}