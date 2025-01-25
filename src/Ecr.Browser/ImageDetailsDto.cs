namespace Ecr.Browser;

public class ImageDetailsDto
{
    public required string ImageDigest { get; init; }
    public List<string> ImageTags { get; init; } = [];
    public DateTime? ImagePushedAt { get; init; }
    public DateTime? ImagePulledAt { get; init; }
}