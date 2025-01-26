using Amazon.ECR.Model;

namespace Ecr.Browser.Blazor;

public class EcrService
{
    private readonly EcrClient _ecrClient;
    private readonly List<ImageDetailsDto> _images = [];

    public EcrService(EcrClient ecrClient)
    {
        _ecrClient = ecrClient;
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

    public void RemoveLocalImages(HashSet<ImageDetailsDto> images)
    {
        // Remove the images from the local list
        _images.RemoveAll(images.Contains);
    }

    public async Task RemoveImageAsync(EcrBatchDelete images, CancellationToken cancellationToken = default)
    {
        await _ecrClient.RemoveImagesAsync(images, cancellationToken);
    }

    public async Task<IEnumerable<ImageDetailsDto>> GetImagesAsync(CancellationToken cancellationToken = default)
    {
        if (_images.Count != 0)
        {
            return _images;
        }
        await foreach (var repository in _ecrClient.ListRepositoriesAsync(cancellationToken))
        {
            await foreach (var image in _ecrClient.ListImagesAsync(repository, cancellationToken))
            {
                _images.Add(image);
            }
        }

        return _images;
    }

    public void RefreshImages()
    {
        _images.Clear();
    }
}
