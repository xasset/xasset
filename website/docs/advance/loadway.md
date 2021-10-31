# 加载策略

在 xasset 7.0 中，资源的加载策略是：

1. 查询缓存中是否有记录，如果有直接取缓存的地址；

2. 缓存中如果没有，先看安装包内部是否有，如果有直接返回包内的加载地址；

3. 如果包内没有，再看下载目录是否有，如果有直接返回下载目录的地址；

4. 下载目录没有，直接返回服务器的下载地址。

## 核心代码

以下是相关代码的实现：

```csharp
internal static string GetBundlePathOrURL(ManifestBundle bundle)
{
    var assetBundleName = bundle.nameWithAppendHash;
    if (BundleWithPathOrUrLs.TryGetValue(assetBundleName, out var path)) return path;

    if (OfflineMode || builtinAssets.Contains(assetBundleName))
    {
        path = GetPlayerDataPath(assetBundleName);
        BundleWithPathOrUrLs[assetBundleName] = path;
        return path;
    }

    if (IsDownloaded(bundle))
    {
        path = GetDownloadDataPath(assetBundleName);
        BundleWithPathOrUrLs[assetBundleName] = path;
        return path;
    }

    path = GetDownloadURL(assetBundleName);
    BundleWithPathOrUrLs[assetBundleName] = path;
    return path;
}
```

## 自动更新

比较方便的是，不论是同步加载还是异步加载的资源，在加载的时候，只要本地没有，就会自动去服务器下载，下载后再自动加载，几乎没有额外的负担，这就是自动更新机制。

所以，当本地的资源被意外删除时，通过自动更新可以自动还原被删除的资源，从而让 App 具备一种自修复能力。