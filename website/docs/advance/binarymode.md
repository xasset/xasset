# 安装包加密

要使用安装包加密功能，仅需在 Settings 中勾选 BinaryMode 就可以开启这个功能。

xasset 7.0 使用了最高效的资源加密方式来防止资源轻易被 AssetStudio 之类的工具破解。以下是加载同一个资源加密前后的 CPU 耗时和 GC 的真机 Profiler 采样数据：

|        | CPU（MS） | GC（KB） |
| ------ | --------- | -------- |
| 加密   | 30.15     | 2.5      |
| 未加密 | 33.12     | 2.7      |

> 注：测试设备为 Song XZs，Android 8.0。

## 测试记录

加密版本的 Profiler 采样截图如下： 

  ![profiler-binary](/img/profiler-binary.png)

未加密版本的 Profiler 采样截图如下：

  ![profiler-bundle](/img/profiler-bundle.png)

## 测试代码

以下是测试时使用的代码片段：

```csharp
public void Load()
{
    const string assetPath = "Logo";
    var assetType = typeof(Sprite);
    Profiler.BeginSample("LoadAsset:Async2Sync");
    _assets.Add(Asset.LoadAsync(assetPath, assetType));
    var asset = Asset.Load(assetPath, assetType);
    image.sprite = asset.Get<Sprite>();
    Profiler.EndSample();
    image.SetNativeSize();
    _assets.Add(asset);
}
```

## 声明

需要注意的是，安装包机密功能在不同的平台可能有不同的标准，这里的结果仅供参考。