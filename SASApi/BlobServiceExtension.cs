using Azure.Identity;
using Azure.Storage.Blobs;

namespace SASApi;

public static class BlobServiceExtension
{
    public static void AddBlobService(this IServiceCollection services)
    {
        services.AddSingleton<BlobServiceClient>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var credential = new DefaultAzureCredential();

            return new BlobServiceClient(
                new Uri(config["BlobStorage:Url"]!),
                credential);
        });
    }
}