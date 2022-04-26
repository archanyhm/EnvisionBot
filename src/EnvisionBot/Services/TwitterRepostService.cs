using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using TwitterSharp.Client;
using TwitterSharp.Request;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Response.RMedia;
using TwitterSharp.Rule;

namespace EnvisionBot.Services;

public class TwitterRepostService : BackgroundService
{
    private readonly TwitterClient _twitterClient;

    private readonly IDiscordRestChannelAPI _discordRestChannelApi;

    private readonly ILogger<TwitterRepostService> _logger;

    public TwitterRepostService(TwitterClient twitterClient, IDiscordRestChannelAPI discordRestChannelApi, ILogger<TwitterRepostService> logger)
    {
        _twitterClient = twitterClient;
        _discordRestChannelApi = discordRestChannelApi;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tweetStreamInfo = await _twitterClient.GetInfoTweetStreamAsync();
        await _twitterClient.DeleteTweetStreamAsync(tweetStreamInfo.Select(val => val.Id).ToArray());
        await _twitterClient.AddTweetStreamAsync(new StreamRequest(Expression.Author("archillect")));
        await _twitterClient.NextTweetStreamAsync((tweet) =>
        {
            bool hasAttachment = !(tweet.Attachments.Media == null || tweet.Attachments.Media.Length < 1);

            if (hasAttachment)
            {
                var firstAttachment = tweet.Attachments.Media!.First();

                var embedDescription = tweet.Text;
                var embedAuthor = new EmbedAuthor(
                    $"@{tweet.Author.Username}",
                    tweet.Author.Url,
                    tweet.Author.ProfileImageUrl
                );
                var embedFooter = new EmbedFooter(tweet.Source);

                if (firstAttachment.Type == MediaType.Photo)
                {
                    var embed = new Embed(
                        Description: embedDescription,
                        Author: embedAuthor,
                        Footer: embedFooter,
                        Image: new EmbedImage(firstAttachment.Url)
                    );
                    
                    _discordRestChannelApi.CreateMessageAsync(new Snowflake(965996569226772500), embeds: new[] {embed}, ct: stoppingToken);
                }
                else if (firstAttachment.Type == MediaType.Video)
                {
                    var embed = new Embed(
                        Description: embedDescription,
                        Author: embedAuthor,
                        Footer: embedFooter,
                        Video: new EmbedVideo(firstAttachment.Url)
                    );
                    
                    _discordRestChannelApi.CreateMessageAsync(new Snowflake(965996569226772500), embeds: new[] {embed}, ct: stoppingToken);
                }
                else if (firstAttachment.Type == MediaType.AnimatedGif)
                {
                    var embed = new Embed(
                        Description: embedDescription,
                        Author: embedAuthor,
                        Footer: embedFooter,
                        Image: new EmbedImage(firstAttachment.Url)
                    );
                    
                    _discordRestChannelApi.CreateMessageAsync(new Snowflake(965996569226772500), embeds: new[] {embed}, ct: stoppingToken);
                }
                
                _logger.LogInformation(
                    $"Attachment URL: {firstAttachment.Url}"
                );
                
                _logger.LogInformation(
                    $"Attachment Type: {firstAttachment.Type.ToString()}"
                );
            }
        },
        new TweetSearchOptions
        {
            UserOptions = new [] {UserOption.Profile_Image_Url, UserOption.Url},
            MediaOptions = new [] {MediaOption.Url, MediaOption.Preview_Image_Url},
            TweetOptions = new [] {TweetOption.Attachments, TweetOption.Source}
        });
    }
}