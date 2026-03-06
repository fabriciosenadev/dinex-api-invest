namespace DinExApi.Core;

public interface IAccessTokenService
{
    AccessTokenResult Generate(User user);
}
