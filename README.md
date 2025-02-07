# ECR Browser

## Overview

ECR Browser is a .NET 9 application designed to provide a user-friendly interface for browsing and managing container images stored in Amazon Elastic Container Registry (ECR). It consists of a Blazor-based frontend and a .NET backend for interacting with the AWS ECR service.

## Key Features

- **Browse ECR Repositories:** List and view container image repositories within your AWS account.
- **View Image Details:** Display detailed information about each image, including digest, tags, push date, pull date, and size.
- **Filtering and Sorting:** Filter images based on age, the number of images in a repository, and sort by pull date or size.
- **Batch Image Deletion:** Select and delete multiple images at once from your ECR repositories.
- **Clipboard Integration:** Easily copy image digests to the clipboard.
- **Asynchronous Operations:** Utilizes asynchronous operations for efficient data retrieval and management.
- **Caching:** Implements caching to improve performance and reduce API calls to AWS ECR.
- **UI using MudBlazor:** Modern and responsive UI components.
- **Aspire Integration:** Uses Aspire to orchestrate the application and its dependencies.

## Architecture

The application follows a multi-project structure:

- **Ecr.Browser.AppHost:** The Aspire AppHost project, responsible for orchestrating the application and its dependencies (like Redis for caching). See [src/Ecr.Browser.AppHost/Program.cs](src/Ecr.Browser.AppHost/Program.cs).
- **Ecr.Browser.Blazor:** The Blazor frontend project, providing the user interface for browsing and managing ECR images. See [src/Ecr.Browser.Blazor/Ecr.Browser.Blazor.csproj](src/Ecr.Browser.Blazor/Ecr.Browser.Blazor.csproj).
- **Ecr.Browser.ServiceDefaults:** Contains shared configurations and extensions for services, such as service discovery, health checks, and OpenTelemetry setup. See [src/Ecr.Browser.ServiceDefaults/Extensions.cs](src/Ecr.Browser.ServiceDefaults/Extensions.cs).
- **Ecr.Browser:** The core library project, containing the business logic for interacting with AWS ECR. See [src/Ecr.Browser/Ecr.Browser.csproj](src/Ecr.Browser/Ecr.Browser.csproj).

### Components

- **RepositoriesPage ([src/Ecr.Browser.Blazor/Components/Pages/RepositoriesPage.razor](src/Ecr.Browser.Blazor/Components/Pages/RepositoriesPage.razor), [src/Ecr.Browser.Blazor/Components/Pages/RepositoriesPage.razor.cs](src/Ecr.Browser.Blazor/Components/Pages/RepositoriesPage.razor.cs)):** This Blazor page displays the list of ECR repositories and their images. It handles filtering, sorting, and deletion of images. It uses the [`MudTable`](src/Ecr.Browser.Blazor/Components/Pages/RepositoriesPage.razor.cs) component to display the data.
- **EcrService ([src/Ecr.Browser.Blazor/EcrService.cs](src/Ecr.Browser.Blazor/EcrService.cs)):** This service is responsible for fetching image data from ECR, caching it, and providing methods for deleting images. It uses [`IFusionCache`](src/Ecr.Browser.Blazor/EcrService.cs) for caching.
- **EcrClient ([src/Ecr.Browser/EcrClient.cs](src/Ecr.Browser/EcrClient.cs)):** This class interacts directly with the AWS ECR API using the [`IAmazonECR`](src/Ecr.Browser/EcrClient.cs) interface.
- **ImageDetailsDto ([src/Ecr.Browser/ImageDetailsDto.cs](src/Ecr.Browser/ImageDetailsDto.cs)):** A data transfer object representing the details of a container image.
- **EcrBatchDelete ([src/Ecr.Browser/EcrBatchDelete.cs](src/Ecr.Browser/EcrBatchDelete.cs)):** Represents a batch of images to be deleted from ECR.
- **ClipboardService ([src/Ecr.Browser.Blazor/ClipboardService.cs](src/Ecr.Browser.Blazor/ClipboardService.cs)):** Provides functionality to copy text to the clipboard using JavaScript interop.
- **ConfirmationDialog ([src/Ecr.Browser.Blazor/Components/Pages/ConfirmationDialog.razor](src/Ecr.Browser.Blazor/Components/Pages/ConfirmationDialog.razor)):** A reusable dialog component for confirming actions.

## How It Works

1. **Data Retrieval:** The [`RepositoriesPage`](src/Ecr.Browser.Blazor/Components/Pages/RepositoriesPage.razor.cs) component calls the [`GetImagesAsync`](src/Ecr.Browser.Blazor/EcrService.cs) method in the [`EcrService`](src/Ecr.Browser.Blazor/EcrService.cs) to retrieve image data.
2. **Caching:** The [`EcrService`](src/Ecr.Browser.Blazor/EcrService.cs) uses [`IFusionCache`](src/Ecr.Browser.Blazor/EcrService.cs) to cache the image data. If the data is not in the cache, it calls the [`GetImagesInnerAsync`](src/Ecr.Browser.Blazor/EcrService.cs) method to fetch the data from ECR.
3. **ECR Interaction:** The [`GetImagesInnerAsync`](src/Ecr.Browser.Blazor/EcrService.cs) method uses the [`EcrClient`](src/Ecr.Browser/EcrClient.cs) to interact with the AWS ECR API. It retrieves a list of repositories and then retrieves the images for each repository.
4. **Data Transformation:** The [`EcrClient`](src/Ecr.Browser/EcrClient.cs) transforms the data received from the AWS ECR API into [`ImageDetailsDto`](src/Ecr.Browser/ImageDetailsDto.cs) objects using the [`ToDto`](src/Ecr.Browser/Extensions.cs) extension method.
5. **Data Display:** The [`RepositoriesPage`](src/Ecr.Browser.Blazor/Components/Pages/RepositoriesPage.razor.cs) component displays the image data in a [`MudTable`](src/Ecr.Browser.Blazor/Components/Pages/RepositoriesPage.razor.cs) component, allowing users to sort, filter, and select images for deletion.
6. **Image Deletion:** When the user selects images for deletion, the [`RemoveImagesAsync`](src/Ecr.Browser.Blazor/Components/Pages/RepositoriesPage.razor.cs) method is called. This method groups the selected images by repository, creates [`EcrBatchDelete`](src/Ecr.Browser/EcrBatchDelete.cs) objects, and calls the [`RemoveImageAsync`](src/Ecr.Browser.Blazor/EcrService.cs) method in the [`EcrService`](src/Ecr.Browser.Blazor/EcrService.cs) to delete the images from ECR.
7. **Local Cache Update:** After deleting the images from ECR, the [`RemoveLocalImages`](src/Ecr.Browser.Blazor/EcrService.cs) method is called to update the local cache.

## Prerequisites

- .NET 9 SDK
- AWS Account
- AWS CLI configured with appropriate credentials to access ECR
- Docker (if running locally)

## Getting Started

1. **Clone the repository:**

    ```bash
    git clone github.
    cd ecr-browser
    ```

2. **Configure AWS Credentials:**

    Ensure your AWS credentials are configured correctly. The application uses the AWS SDK for .NET, which supports various methods for configuring credentials, including environment variables, shared credentials files, and IAM roles.

3. **Run the application:**

    Navigate to the [Ecr.Browser.AppHost](/src/Ecr.Browser.AppHost/) directory and run the application using the following command:

    ```bash
    dotnet run
    ```

    This will start the application and its dependencies, including Redis.

4. **Access the UI:**

    Open your web browser and navigate to the URL displayed in the console output (typically `https://localhost:7148`).

## Configuration

The application can be configured using environment variables and [appsettings.json](/src/Ecr.Browser.Blazor/appsettings.json) files.

### AWS Configuration

The AWS SDK for .NET uses the standard AWS configuration methods. You can configure the AWS region and credentials using environment variables, shared credentials files, or IAM roles.

### Caching Configuration

The caching behavior is configured in the [EcrService](/src/Ecr.Browser.Blazor/EcrService.cs). The `FusionCacheEntryOptions` class is used to configure the cache duration and other options.

### OpenTelemetry Configuration

The application uses OpenTelemetry for tracing and metrics. The OpenTelemetry endpoint can be configured using the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable.

## Contributing

Contributions are welcome! Please submit a pull request with your changes.

## License

This project is licensed under the MIT License.
