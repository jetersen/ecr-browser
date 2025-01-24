using Amazon.ECR;
using Amazon.ECR.Model;

namespace Ecr.Browser;

public class ImageDetailsDto
{
    public string ImageDigest { get; set; }
    public List<string> ImageTags { get; set; }
    public DateTime? ImagePushedAt { get; set; }
    public DateTime? ImagePulledAt { get; set; }
}

public static class Extensions
{
    public static ImageDetailsDto ToDto(this ImageDetail imageDetail)
    {
        return new ImageDetailsDto
        {
            ImageDigest = imageDetail.ImageDigest,
            ImageTags = imageDetail.ImageTags,
            ImagePushedAt = imageDetail.ImagePushedAt,
            ImagePulledAt = imageDetail.LastRecordedPullTime,
        };
    }
}

public class EcrClient
{
    private readonly IAmazonECR _ecrClient;
    private string? _registryId;
    
    public EcrClient(IAmazonECR ecrClient)
    {
        _ecrClient = ecrClient;
    }
    
    public async ValueTask<string> GetRegistryIdAsync()
    {
        return _registryId ??= (await _ecrClient.DescribeRegistryAsync(new()).ConfigureAwait(false)).RegistryId;
    }
    
    public async IAsyncEnumerable<string> ListRepositoriesAsync()
    {
        var response = await _ecrClient.DescribeRepositoriesAsync(new()
        {
            RegistryId = await GetRegistryIdAsync(),
        });
        foreach (var repository in response.Repositories)
        {
            yield return repository.RepositoryName;
        }
    }
    
    public async IAsyncEnumerable<string> ListImagesAsync(string repositoryName)
    {
        var nextToken = "";
        do
        {
            var response = await _ecrClient.DescribeImagesAsync(new()
            {
                Filter = new()
                {
                    TagStatus = TagStatus.TAGGED,
                },
                RepositoryName = repositoryName,
                RegistryId = await GetRegistryIdAsync(),
                MaxResults = 100,
                NextToken = nextToken,
            });
            nextToken = response.NextToken;
            foreach (var image in response.ImageDetails)
            {
                yield return "";
            }
        } while (string.IsNullOrEmpty(nextToken) is false);
    }
}
