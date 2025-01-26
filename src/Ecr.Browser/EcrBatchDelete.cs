using Amazon.ECR.Model;

namespace Ecr.Browser;

public class EcrBatchDelete
{
    public required string RepositoryName { get; init; }
    public List<ImageIdentifier> ImageIdentifiers { get; init; } = [];
}
