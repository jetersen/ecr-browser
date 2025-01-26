using Humanizer;

namespace Ecr.Browser;

public class ImageDetailsDto
{
    public required string ImageDigest { get; init; }
    public List<string> ImageTags { get; init; } = [];
    public required DateTime ImagePushedAt { get; init; }
    public DateTime? ImagePulledAt { get; init; }
    public required string RepositoryName { get; init; }
    public ImageSize ImageSize { get; init; } = new();

    public override int GetHashCode()
    {
        return HashCode.Combine(ImageDigest);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ImageDetailsDto other)
        {
            return false;
        }

        return ImageDigest == other.ImageDigest;
    }
}

public class ImageSize
{
    public long Size { get; set; }
    public override string ToString()
    {
        return Size.Bytes().ToString();
    }
}
