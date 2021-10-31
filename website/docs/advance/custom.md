# 自定义扩展

## 加载地址

默认，xasset 7.0 使用 Assets 开头的相对路径加载资源，但是，如果有特殊需要，可以通过在初始化之前，实现这个代理：

- Versions.customLoadPath

重定义加载地址，以下是相关代码的实现参考：

```csharp
// ...
Versions.customLoadPath = LoadByNameWithoutExtension;
var initialize = Versions.InitializeAsync();
yield return initialize;
```

LoadByNameWithoutExtension 方法的实现如下：

```csharp
private string LoadByNameWithoutExtension(string assetPath)
{
    // loadKeys = { "Prefabs/" };
    if (loadKeys == null || loadKeys.Length == 0) return null; 
    if (!Array.Exists(loadKeys, assetPath.Contains)) return null; 
    var assetName = Path.GetFileNameWithoutExtension(assetPath);
    return assetName;
}
```

需要注意的是，自定义的加载路径需要自行规避同名冲突，如果不想使用自定义的加载路径就返回 null。

## 打包粒度

打包粒度主要通过资源分配的 bundle 名字控制，默认，系统提供了这 5 种打包方式：

- PackTogether：同一个分组的资源打包到一起。
- PackByFile：分组中的每个文件，按文件名单独打包。
- PackByDirectory：分组中的每个文件，按其所在的文件夹名字打包。
- PackByTopDirectory：分组中的每个文件，按一级子目录名字进行打包。
- PackByRaw：分组中的每个文件，按原始格式打包。

自定义打包粒度主要用来把默认的 BundleMode 修改为自己的实现方式。

以下是自定义打包粒度，打包所有 shader 打包到名字为 shader 的 bundle 的代码示例：

```csharp
public static class ExampleCustomPacker
{
    [InitializeOnLoadMethod]
    public static void Initialize()
    {
        global::Versions.Editor.Builds.Group.customPacker += CustomPacker;
    }

    private static string CustomPacker(string assetPath, string bundle, string group)
    {
        if (assetPath.Contains(".shader"))
        {
            return "shader";
        }
        return bundle;
    }
}
```

自定义打包粒度的代理会返回系统默认分配的 bundle，如果不需要使用自定义的可以直接通过代理方法返回这个 bundle，需要使用自定义的就返回自定义的实现。


## 下载地址

自定义下载地址是可以按需使用的功能，使用自定义下载地址可以避免某些资源使用独特的下载地址的时候污染默认下载地址的情况，当然也可以按需用作其他情况。

以下是使用自定义下载地址的代码示例：

```csharp
Versions.CustomDownloadURL = url =>
{
    if (url.EndsWith("arts"))
    {
        return url + "arts_hash";
    }
    return null;
};
```

就像上面的代码一样，如果输入的 url 需要使用自定义下载地址就返回自定义的下载地址，不需要则返回 null。