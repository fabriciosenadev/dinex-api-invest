namespace DinExApi.Service;

public sealed class JsonAssetAliasResolver : IAssetAliasResolver
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Lazy<Dictionary<string, string>> Aliases = new(LoadAliases, LazyThreadSafetyMode.ExecutionAndPublication);

    public string Resolve(string assetSymbol)
    {
        if (string.IsNullOrWhiteSpace(assetSymbol))
        {
            return string.Empty;
        }

        var normalized = assetSymbol.Trim().ToUpperInvariant();
        return Aliases.Value.TryGetValue(normalized, out var resolved)
            ? resolved
            : normalized;
    }

    private static Dictionary<string, string> LoadAliases()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in GetCandidateFiles())
        {
            if (!File.Exists(filePath))
            {
                continue;
            }

            try
            {
                using var stream = File.OpenRead(filePath);
                var fileData = JsonSerializer.Deserialize<AssetAliasFile>(stream, JsonOptions);
                if (fileData is null)
                {
                    continue;
                }

                if (fileData.Aliases is not null)
                {
                    foreach (var pair in fileData.Aliases)
                    {
                        if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value))
                        {
                            continue;
                        }

                        map[pair.Key.Trim().ToUpperInvariant()] = pair.Value.Trim().ToUpperInvariant();
                    }
                }

                if (fileData.Items is not null)
                {
                    foreach (var item in fileData.Items)
                    {
                        if (string.IsNullOrWhiteSpace(item.Alias) || string.IsNullOrWhiteSpace(item.Canonical))
                        {
                            continue;
                        }

                        map[item.Alias.Trim().ToUpperInvariant()] = item.Canonical.Trim().ToUpperInvariant();
                    }
                }
            }
            catch
            {
                // Alias file is optional. Invalid content should not break the API startup.
            }
        }

        return map;
    }

    private static IEnumerable<string> GetCandidateFiles()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "AssetAliases.json");
        yield return Path.Combine(Directory.GetCurrentDirectory(), "AssetAliases.json");
        yield return Path.Combine(Directory.GetCurrentDirectory(), "src", "Api", "AssetAliases.json");
    }

    private sealed class AssetAliasFile
    {
        public Dictionary<string, string>? Aliases { get; init; }
        public List<AssetAliasItem>? Items { get; init; }
    }

    private sealed class AssetAliasItem
    {
        public string? Alias { get; init; }
        public string? Canonical { get; init; }
    }
}
