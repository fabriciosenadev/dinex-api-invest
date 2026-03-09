namespace DinExApi.Core;

public interface IAssetAliasResolver
{
    string Resolve(string assetSymbol);
}
