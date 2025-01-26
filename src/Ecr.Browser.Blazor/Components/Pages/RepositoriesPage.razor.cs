using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Ecr.Browser.Blazor.Components.Pages;

public partial class RepositoriesPage
{
    private MudTable<ImageDetailsDto> _tableRef = null!;

    [Inject] public required ClipboardService ClipboardService { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Inject] public required EcrService EcrService { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }

    private async Task RemoveImagesAsync()
    {
        var selectedItems = _tableRef.Context.Selection;
        if (selectedItems.Count == 0)
        {
            return;
        }
        _deletionTotal = selectedItems.Count;
        _deletionProgress = 0;
        var parameters = new DialogParameters
        {
            { "ContentText", $"Are you sure you want to delete the {_deletionTotal} selected images?" },
            { "Color", Color.Error },
            { "ButtonText", "Delete" },
        };

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Delete Images", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: true } or { Data: false })
        {
            return;
        }
        _showDeleteProgress = true;
        StateHasChanged();

        var ecrBatchDeleteList = EcrService.GetBatchDeleteList(selectedItems);;

        foreach (var ecrBatchDelete in ecrBatchDeleteList)
        {
            await EcrService.RemoveImageAsync(ecrBatchDelete);
            _deletionProgress += ecrBatchDelete.ImageIdentifiers.Count;
            StateHasChanged();
        }
        EcrService.RemoveLocalImages(selectedItems);
        _tableRef.Context.Selection.Clear();
        _tableRef.Context.UpdateRowCheckBoxes();
        _tableRef.Context.TableStateHasChanged?.Invoke();
        StateHasChanged();
        await _tableRef.ReloadServerData();
        _showDeleteProgress = false;
        await Task.Delay(TimeSpan.FromSeconds(5));
        StateHasChanged();
    }

    private async Task RefreshData()
    {
        EcrService.RefreshImages();
        await _tableRef.ReloadServerData();
    }

    private readonly TableGroupDefinition<ImageDetailsDto> _groupDefinition = new()
    {
        GroupName = "Repository",
        Indentation = false,
        Expandable = true,
        IsInitiallyExpanded = false,
        Selector = e => e.RepositoryName,
    };

    private async Task CopyToClipboard(string imageDigest)
    {
        try
        {
            await ClipboardService.WriteTextAsync(imageDigest);
            Snackbar.Add("Digest copied to clipboard!", Severity.Success);
        }
        catch
        {
            Snackbar.Add("Failed to copy digest to clipboard!", Severity.Error);
        }
    }

    private async Task<TableData<ImageDetailsDto>> LoadImages(TableState state, CancellationToken cancellationToken = default)
    {
        var imageDetails = await EcrService.GetImagesAsync(cancellationToken);

        if (_includeOnlyOne is false)
        {
            imageDetails = imageDetails.GroupBy(x => x.RepositoryName)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g
                    .OrderByDescending(x => x.ImagePulledAt ?? x.ImagePushedAt)
                    .Skip(1)).ToList();
        }

        if (_filterByMonth > 0)
        {
            var monthsSubtracted = DateTime.UtcNow.AddMonths(-_filterByMonth);
            imageDetails = imageDetails.Where(x => x.ImagePulledAt == null || x.ImagePulledAt <= monthsSubtracted);
        }

        imageDetails = state.SortLabel switch
        {
            "pulled_at" => state.SortDirection == SortDirection.Ascending
                ? imageDetails.OrderBy(x => x.ImagePulledAt ?? x.ImagePushedAt)
                : imageDetails.OrderByDescending(x => x.ImagePulledAt ?? x.ImagePushedAt),
            "size" => state.SortDirection == SortDirection.Ascending
                ? imageDetails.OrderBy(x => x.ImageSize.Size)
                : imageDetails.OrderByDescending(x => x.ImageSize.Size),
            // order by name ascending and then by pushed_at descending
            _ => imageDetails.OrderBy(x => x.RepositoryName).ThenByDescending(y => y.ImagePushedAt),
        };

        var imageDetailsList = imageDetails.ToList();

        _totalImages = imageDetailsList.Count;
        _totalSize = imageDetailsList.Sum(x => x.ImageSize.Size);
        StateHasChanged();

        return new()
        {
            Items = imageDetailsList,
            TotalItems = imageDetailsList.Count,
        };
    }

    private int _filterByMonth;
    private int _totalImages;
    private long _totalSize;
    private bool _includeOnlyOne;
    private bool _showDeleteProgress;
    private long _deletionProgress;
    private long _deletionTotal;

    private Task OnFilter(int i)
    {
        _filterByMonth = i;
        return _tableRef.ReloadServerData();
    }

    private Task OnInclude()
    {
        return _tableRef.ReloadServerData();
    }
}
