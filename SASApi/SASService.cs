using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace SASApi;

public sealed class SasService
{
    private readonly BlobServiceClient _blobServiceClient;
    private static UserDelegationKey? _userDelegationKey;
    private const int UserDelegationDurationInMinutes = 60;
    private const int UserDelegationGracePeriodInMinutes = -10;
    private const int SasTokenDurationInMinutes = 2;
    public SasService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
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

    public async Task<string?> GetContainerSasToken(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        await GetUserDelegationKey();
        
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerClient.Name,
            Resource = "c",
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SasTokenDurationInMinutes)
        };

        sasBuilder.SetPermissions(BlobContainerSasPermissions.Create);
        var uriBuilder = new BlobUriBuilder(containerClient.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(
                _userDelegationKey,
                containerClient.GetParentBlobServiceClient().AccountName)
        };
        return uriBuilder.ToUri().AbsoluteUri;
    }
}