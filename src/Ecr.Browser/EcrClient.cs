using Amazon.ECR;

namespace Ecr.Browser;

public class EcrClient
{
    private readonly IAmazonECR _ecrClient;

    public EcrClient(IAmazonECR ecrClient)
    {
        _ecrClient = ecrClient;
    }

    public async IAsyncEnumerable<string> ListRepositoriesAsync()
    {
        var response = await _ecrClient.DescribeRepositoriesAsync(new());
        foreach (var repository in response.Repositories)
        {
            yield return repository.RepositoryName;
        }
    }
    
    public async IAsyncEnumerable<ImageDetailsDto> ListImagesAsync(string repositoryName)
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
                MaxResults = 100,
                NextToken = nextToken,
            });
            nextToken = response.NextToken;
            foreach (var image in response.ImageDetails)
            {
                yield return image.ToDto();
            }
        } while (string.IsNullOrEmpty(nextToken) is false);
    }
}
