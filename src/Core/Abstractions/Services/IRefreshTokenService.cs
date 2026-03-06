namespace DinExApi.Core;

public interface IRefreshTokenService
{
    RefreshTokenResult Generate();
    string ComputeHash(string refreshToken);
}
