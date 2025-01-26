using System.Runtime.CompilerServices;
using Amazon.ECR;
using Amazon.ECR.Model;
using Microsoft.Extensions.Logging;

namespace Ecr.Browser;

public class EcrClient
{
    private readonly IAmazonECR _ecrClient;
    private readonly ILogger<EcrClient> _logger;

    public EcrClient(IAmazonECR ecrClient, ILogger<EcrClient> logger)
    {
        _ecrClient = ecrClient;
        _logger = logger;
    }

    public async Task RemoveImagesAsync(EcrBatchDelete batch, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing a total of {Count} images from repository {RepositoryName}", batch.ImageIdentifiers.Count, batch.RepositoryName);
        await _ecrClient.BatchDeleteImageAsync(new()
        {
            ImageIds = batch.ImageIdentifiers,
            RepositoryName = batch.RepositoryName,
        }, cancellationToken);
    }

    public async Task RemoveImagesAsync(List<ImageDetailsDto> imageDetails, CancellationToken cancellationToken = default)
    {
        // Group by repository name and batch them in groups of 100 using Chunk
        var imageDetailsByRepository = imageDetails
            .GroupBy(x => x.RepositoryName)
            .SelectMany(repository => repository
                .Select(x => new ImageIdentifier { ImageDigest = x.ImageDigest })
                .Chunk(100)
                .Select(imageIdentifiers => new EcrBatchDelete
                {
                    RepositoryName = repository.Key,
                    ImageIdentifiers = imageIdentifiers.ToList(),
                }));

        foreach (var batch in imageDetailsByRepository)
        {
            await RemoveImagesAsync(batch, cancellationToken);
        }
    }

    public async IAsyncEnumerable<string> ListRepositoriesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await _ecrClient.DescribeRepositoriesAsync(new(), cancellationToken);
        foreach (var repository in response.Repositories)
        {
            yield return repository.RepositoryName;
        }
    }
    
    public async IAsyncEnumerable<ImageDetailsDto> ListImagesAsync(string repositoryName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? nextToken = null;
        do
        {
            var response = await _ecrClient.DescribeImagesAsync(new()
            {
                Filter = new()
                {
                    TagStatus = TagStatus.TAGGED,
                },
                RepositoryName = repositoryName,
                MaxResults = 100,
                NextToken = nextToken,
            }, cancellationToken);
            nextToken = response.NextToken;
            foreach (var image in response.ImageDetails)
            {
                yield return image.ToDto(repositoryName);
            }
        } while (string.IsNullOrEmpty(nextToken) is false);
    }
}
