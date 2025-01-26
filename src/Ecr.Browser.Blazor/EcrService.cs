using Amazon.ECR.Model;
using ZiggyCreatures.Caching.Fusion;

namespace Ecr.Browser.Blazor;

public class EcrService
{
    private readonly EcrClient _ecrClient;
    private List<ImageDetailsDto> _images = [];
    private readonly IFusionCache _cache;
    private readonly FusionCacheEntryOptions _cacheOptions;
    private const string CacheKey = "images";

    public EcrService(EcrClient ecrClient, IFusionCache cache)
    {
        _ecrClient = ecrClient;
        _cache = cache;
        _cacheOptions = new()
        {
            Duration = TimeSpan.FromMinutes(5),
            DistributedCacheDuration = TimeSpan.FromHours(1),
        };
    }

    public List<EcrBatchDelete> GetBatchDeleteList(HashSet<ImageDetailsDto> images)
    {
        // Group by repository name and batch them in groups of 100 using Chunk
        return images
            .GroupBy(x => x.RepositoryName)
            .SelectMany(repository => repository
                .Select(x => new ImageIdentifier { ImageDigest = x.ImageDigest })
                .Chunk(100)
                .Select(imageIdentifiers => new EcrBatchDelete
                {
                    RepositoryName = repository.Key,
                    ImageIdentifiers = imageIdentifiers.ToList(),
                }))
            .ToList();
    }

    public async Task RemoveLocalImages(HashSet<ImageDetailsDto> images, CancellationToken cancellationToken = default)
    {
        _images.RemoveAll(images.Contains);
        await _cache.SetAsync(CacheKey, _images, _cacheOptions, cancellationToken);
    }

    public async Task RemoveImageAsync(EcrBatchDelete images, CancellationToken cancellationToken = default)
    {
        await _ecrClient.RemoveImagesAsync(images, cancellationToken);
    }

    private async Task<List<ImageDetailsDto>> GetImagesInnerAsync(CancellationToken cancellationToken = default)
    {
        var images = new List<ImageDetailsDto>();
        await foreach (var repository in _ecrClient.ListRepositoriesAsync(cancellationToken))
        {
            await foreach (var image in _ecrClient.ListImagesAsync(repository, cancellationToken))
            {
                images.Add(image);
            }
        }

        return images;
    }

    public async Task<IEnumerable<ImageDetailsDto>> GetImagesAsync(CancellationToken cancellationToken = default)
    {
        _images = await _cache.GetOrSetAsync(CacheKey, GetImagesInnerAsync, _cacheOptions, cancellationToken);

        return _images;
    }

    public async Task RefreshImages(CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(CacheKey, _cacheOptions, cancellationToken);
        _images.Clear();
    }
}
