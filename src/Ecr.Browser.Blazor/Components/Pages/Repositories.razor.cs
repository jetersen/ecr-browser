using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecr.Browser;
using Microsoft.AspNetCore.Components;

namespace Ecr.Browser.Blazor.Components.Pages
{
    public partial class Repositories
    {
        [Inject]
        private EcrClient EcrClient { get; set; }

        private List<string> repositories = new();
        private HashSet<string> expandedRepositories = new();
        private HashSet<ImageDetailsDto> selectedImages = new();
        private DateTime? filterDate;

        protected override async Task OnInitializedAsync()
        {
            await foreach (var repository in EcrClient.ListRepositoriesAsync())
            {
                repositories.Add(repository);
            }
        }

        private void ToggleRepository(string repository)
        {
            if (expandedRepositories.Contains(repository))
            {
                expandedRepositories.Remove(repository);
            }
            else
            {
                expandedRepositories.Add(repository);
            }
        }

        private async Task<IEnumerable<ImageDetailsDto>> GetImages(string repository)
        {
            var images = new List<ImageDetailsDto>();
            await foreach (var image in EcrClient.ListImagesAsync(repository))
            {
                if (filterDate == null || image.ImagePulledAt >= filterDate)
                {
                    images.Add(image);
                }
            }
            return images;
        }

        private void ToggleImageSelection(ImageDetailsDto image, bool isSelected)
        {
            if (isSelected)
            {
                selectedImages.Add(image);
            }
            else
            {
                selectedImages.Remove(image);
            }
        }

        private void ApplyDateFilter(DateTime? date)
        {
            filterDate = date;
        }

        private void DeleteImage(ImageDetailsDto image)
        {
            // Implement the logic to delete a single image
        }

        private void DeleteSelectedImages()
        {
            // Implement the logic to delete all selected images
        }
    }
}
