using Amazon.ECR.Model;

namespace Ecr.Browser;

public static class Extensions
{
    public static ImageDetailsDto ToDto(this ImageDetail imageDetail, string repositoryName)
    {
        return new()
        {
            RepositoryName = repositoryName,
            ImageDigest = imageDetail.ImageDigest,
            ImageTags = imageDetail.ImageTags,
            ImageSize = new() { Size = imageDetail.ImageSizeInBytes ?? 0 },
            ImagePushedAt = imageDetail.ImagePushedAt ?? DateTime.MinValue,
            ImagePulledAt = imageDetail.LastRecordedPullTime,
        };
    }
}
