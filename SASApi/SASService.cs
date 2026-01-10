using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace SASApi;

public sealed class SasService
{
    private readonly BlobServiceClient _blobServiceClient;
    private static UserDelegationKey? _userDelegationKey;
    private readonly IConfiguration _config;
    private const int UserDelegationDurationInMinutes = 60;
    private const int UserDelegationGracePeriodInMinutes = -10;
    private const int SasTokenDurationInMinutes = 60;
    public SasService(BlobServiceClient blobServiceClient, IConfiguration config)
    {
        _blobServiceClient = blobServiceClient;
        _config = config;
    }

    private async Task GetUserDelegationKey()
    {
        if (_userDelegationKey is null ||
            DateTimeOffset.UtcNow.CompareTo(_userDelegationKey.SignedExpiresOn.AddMinutes(UserDelegationGracePeriodInMinutes)) > 0)
        {
            var startsOn = DateTimeOffset.UtcNow;
            var expiresOn = startsOn.AddMinutes(UserDelegationDurationInMinutes);
        
            var response = await _blobServiceClient
                .GetUserDelegationKeyAsync(startsOn, expiresOn);
            
            if (!response.HasValue) throw new UnauthorizedAccessException();
            _userDelegationKey = response.Value;
        }
    }

    public async Task<string?> GetContainerSasToken(FileData fileData)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient("data");

        await GetUserDelegationKey();
        
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerClient.Name,
            BlobName = fileData.BlobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SasTokenDurationInMinutes)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);
        
        string sasToken = sasBuilder.ToSasQueryParameters(_userDelegationKey, 
            containerClient.AccountName).ToString();
        
        // var uriBuilder = new BlobUriBuilder(containerClient.Uri)
        // {
        //     Sas = sasBuilder.ToSasQueryParameters(
        //         _userDelegationKey,
        //         containerClient.GetParentBlobServiceClient().AccountName)
        // };
        return $"{_config["BlobStorage:Url"]}data/{fileData.BlobName}?{sasToken}";
    }
}