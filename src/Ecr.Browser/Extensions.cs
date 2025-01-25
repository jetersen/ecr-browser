using Amazon.ECR.Model;

namespace Ecr.Browser;

public static class Extensions
{
    public static ImageDetailsDto ToDto(this ImageDetail imageDetail)
    {
        return new()
        {
            ImageDigest = imageDetail.ImageDigest,
            ImageTags = imageDetail.ImageTags,
            ImagePushedAt = imageDetail.ImagePushedAt,
            ImagePulledAt = imageDetail.LastRecordedPullTime,
        };
    }
}