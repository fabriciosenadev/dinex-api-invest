namespace DinExApi.Tests;

public sealed class UserEntityTests
{
    [Fact]
    public void CreateUser_Should_Add_Notification_When_Password_Is_Invalid()
    {
        var user = User.CreateUser("Fabricio", "fabricio@email.com", "123", "123");

        Assert.False(user.IsValid);
        Assert.Contains(user.Notifications, n => n.Key == "User.Password");
    }

    [Fact]
    public void CreateUser_Should_Add_Notification_When_Passwords_Do_Not_Match()
    {
        var user = User.CreateUser("Fabricio", "fabricio@email.com", "Senha@123", "Senha@456");

        Assert.False(user.IsValid);
        Assert.Contains(user.Notifications, n => n.Key == "User.ConfirmPassword");
    }

    [Fact]
    public void SetPasswordHash_Should_Add_Notification_When_Hash_Is_Empty()
    {
        var user = ValidInactiveUser();

        user.SetPasswordHash(string.Empty);

        Assert.False(user.IsValid);
        Assert.Contains(user.Notifications, n => n.Key == "User.PasswordHash");
    }

    [Fact]
    public void AssignActivationCode_Should_Set_Expiration_And_Reset_Failed_Attempts()
    {
        var user = ValidInactiveUser();

        user.AssignActivationCode("ab12cd34");

        Assert.Equal("AB12CD34", user.ActivationCode);
        Assert.True(user.HasValidActivationCode());
        Assert.Equal(0, user.ActivationCodeFailedAttempts);
        Assert.True(user.UpdatedAt.HasValue);
    }

    [Fact]
    public void RegisterActivationFailure_Should_Clear_Code_When_Reaching_Max_Attempts()
    {
        var user = ValidInactiveUser();
        user.AssignActivationCode("AB12CD34");

        var locked = false;
        for (var i = 0; i < 5; i++)
        {
            locked = user.RegisterActivationFailure(5);
        }

        Assert.True(locked);
        Assert.Null(user.ActivationCode);
        Assert.Null(user.ActivationCodeExpiresAtUtc);
    }

    [Fact]
    public void AssignPasswordResetCode_Should_Add_Notification_When_Expiration_Is_Past()
    {
        var user = ValidActiveUser();

        user.AssignPasswordResetCode("AB12CD34", DateTime.UtcNow.AddMinutes(-1));

        Assert.False(user.IsValid);
        Assert.Contains(user.Notifications, n => n.Key == "User.PasswordResetCode");
    }

    [Fact]
    public void HasValidPasswordResetCode_Should_Return_False_For_Mismatch()
    {
        var user = ValidActiveUser();
        user.AssignPasswordResetCode("AB12CD34", DateTime.UtcNow.AddMinutes(30));

        var valid = user.HasValidPasswordResetCode("ZZ99ZZ99", DateTime.UtcNow);

        Assert.False(valid);
    }

    [Fact]
    public void ClearPasswordResetCode_Should_Clear_Fields()
    {
        var user = ValidActiveUser();
        user.AssignPasswordResetCode("AB12CD34", DateTime.UtcNow.AddMinutes(30));

        user.ClearPasswordResetCode();

        Assert.Null(user.PasswordResetCode);
        Assert.Null(user.PasswordResetCodeExpiresAtUtc);
    }

    [Fact]
    public void AssignRefreshToken_Should_Add_Notification_When_ExpiresAt_Is_Past()
    {
        var user = ValidActiveUser();

        user.AssignRefreshToken("hash", DateTime.UtcNow.AddMinutes(-1));

        Assert.False(user.IsValid);
        Assert.Contains(user.Notifications, n => n.Key == "User.RefreshToken");
    }

    [Fact]
    public void HasValidRefreshToken_Should_Return_False_For_Different_Hash()
    {
        var user = ValidActiveUser();
        user.AssignRefreshToken("hash-1", DateTime.UtcNow.AddMinutes(30));

        var valid = user.HasValidRefreshToken("hash-2", DateTime.UtcNow);

        Assert.False(valid);
    }

    [Fact]
    public void RegisterFailedLogin_Should_Lock_After_Max_Attempts()
    {
        var user = ValidActiveUser();
        var now = DateTime.UtcNow;

        var locked = false;
        for (var i = 0; i < 5; i++)
        {
            locked = user.RegisterFailedLogin(5, TimeSpan.FromMinutes(15), now);
        }

        Assert.True(locked);
        Assert.True(user.IsLoginLocked(now.AddMinutes(1)));
        Assert.Equal(0, user.LoginFailedAttempts);
    }

    [Fact]
    public void RegisterSuccessfulLogin_Should_Reset_Login_Counters()
    {
        var user = ValidActiveUser();
        var now = DateTime.UtcNow;
        user.RegisterFailedLogin(5, TimeSpan.FromMinutes(15), now);

        user.RegisterSuccessfulLogin(now.AddMinutes(1));

        Assert.Equal(0, user.LoginFailedAttempts);
        Assert.Null(user.LoginLockedUntilUtc);
    }

    private static User ValidInactiveUser()
    {
        return new User(
            fullName: "Fabricio Sena",
            email: "fabricio@email.com",
            password: "hashed",
            userStatus: UserStatus.Inactive,
            activationCode: null,
            activationCodeExpiresAtUtc: null,
            activationCodeFailedAttempts: 0,
            passwordResetCode: null,
            passwordResetCodeExpiresAtUtc: null,
            refreshTokenHash: null,
            refreshTokenExpiresAtUtc: null,
            loginFailedAttempts: 0,
            loginLockedUntilUtc: null,
            createdAt: DateTime.UtcNow,
            updatedAt: null,
            deletedAt: null);
    }

    private static User ValidActiveUser()
    {
        var user = ValidInactiveUser();
        user.Activate();
        return user;
    }
}
