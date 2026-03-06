namespace DinExApi.Infra;

internal sealed class Argon2IdUserPasswordHasher(IOptions<AppSettings> options) : IUserPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 3;
    private const int MemorySizeKb = 65536;
    private const int DegreeOfParallelism = 2;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        var pepper = GetPepper();
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = ComputeHash(password, pepper, salt, Iterations, MemorySizeKb, DegreeOfParallelism, HashSize);

        return $"argon2id${Iterations}${MemorySizeKb}${DegreeOfParallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string hashedPassword, string password)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var parts = hashedPassword.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 6 || !string.Equals(parts[0], "argon2id", StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations)
            || !int.TryParse(parts[2], out var memorySizeKb)
            || !int.TryParse(parts[3], out var degreeOfParallelism))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[4]);
            expectedHash = Convert.FromBase64String(parts[5]);
        }
        catch
        {
            return false;
        }

        var pepper = GetPepper();
        var computedHash = ComputeHash(
            password,
            pepper,
            salt,
            iterations,
            memorySizeKb,
            degreeOfParallelism,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
    }

    private static byte[] ComputeHash(
        string password,
        string pepper,
        byte[] salt,
        int iterations,
        int memorySizeKb,
        int degreeOfParallelism,
        int hashSize)
    {
        var input = Encoding.UTF8.GetBytes($"{password}{pepper}");
        var argon2 = new Argon2id(input)
        {
            Salt = salt,
            Iterations = iterations,
            MemorySize = memorySizeKb,
            DegreeOfParallelism = degreeOfParallelism
        };

        return argon2.GetBytes(hashSize);
    }

    private string GetPepper()
    {
        var pepper = options.Value.PasswordPepper;
        if (string.IsNullOrWhiteSpace(pepper))
        {
            throw new InvalidOperationException("AppSettings.PasswordPepper is not configured.");
        }

        return pepper;
    }
}
