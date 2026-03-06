namespace DinExApi.Core;

public interface IUserPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string password);
}
