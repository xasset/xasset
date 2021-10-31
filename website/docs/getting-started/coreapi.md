---
id: coreapi
title: 核心接口
sidebar_position: 2
---

xasset 7.0 运行时的核心接口主要有：

- **初始化**：Versions.InitilaizeAsync 
- **更新版本信息**：Versions.UpdateAsync 
- **获取更新大小**：Versions.GetDownloadSizeAsync 
- **批量下载文件**：Versions.DownloadAsync 
- **常规资源加载**：Asset.Load(Async)
- **加载场景**：Scene.LoadAsync

通常，大部分业务开发都只需要用到这 6 个接口，5 分钟 1 个，岂不是半个小时就能掌握？

## 初始化

初始化主要用来完成平台路径的设置以及母包的清单文件的加载，以下是初始化的示例代码：

```csharp
Versions.DownloadURL = downloadURL;
var operation = Versions.InitializeAsync();
yield return operation;
Logger.I("Initialize: {0}", operation.status);
Logger.I("API Version: {0}", Versions.APIVersion);
Logger.I("Manifests Version: {0}", Versions.ManifestsVersion);
Logger.I("PlayerDataPath: {0}", Versions.PlayerDataPath);
Logger.I("DownloadDataPath: {0}", Versions.DownloadDataPath);
Logger.I("DownloadURL: {0}", Versions.DownloadURL);
```

注：在使用其他运行时API前，请先确保系统以及正常初始化了。

## 更新版本信息

更新版本信息主要用来同步服务器最新的清单文件。

xasset 7.0 版本信息主要由以下两部分组成:

- Manifest: 每个 Build 对应的清单文件（包含了每个 Build 中所有资源的版本信息）
- ManifestVersion：每个清单文件对应的版本文件（内容小，只包含清单文件的版本信息）

版本信息的更新流程是：

1. 下载清单的版本文件
2. 对比版本文件对应的清单是否已经下载
    - 如果下载了，就不再下载清单，直接转到 3
    - 如果没下载，就先下载清单，下载完成后再转到 3
3. 按需进行清单文件的装载
    - 对于安装包内部的清单，会先检查其对于的下载目录的清单是否存在，如果存在并且下载目录的清单版本 > 内部版本，直接加载下载目录的版本，否则加载内部版本
    - 对于服务器的清单，仅当版本发生变化时会读取数据到内存，当用户确认更新后才会覆盖内部版本的清单文件

以下是更新版本信息的示例代码：

```csharp
var updateAsync = Versions.UpdateAsync("Arts_hash", "Data_hash");
yield return updateAsync;
if (updateAsync.status == OperationStatus.Failed)
{
    yield return MessageBox.Show("提示", "更新版本信息失败，请检测网络链接后重试。", ok =>
    {
        if (ok)
            StartUpdate();
        else
            OnComplete();
    }, "重试", "跳过");
    yield break;
}
```

需要注意的是，在没有调用以下接口前，服务器的清单只是下载到本地，而不会覆盖已有的版本文件：

```csharp
updateAsync.Override();
```

如果需要高版本兼容低版本的支持，可以把 Override 的操作，可以放到获取更新大小之后，不需要的化可以在更新完成后直接调用，然后再执行以下代码：

```csharp
updateAsync.Dispose();
```

## 获取下载大小

获取下载大小主要通过如下接口实现：

- Versions.GetDownloadSizeAsync

以下是调用该接口的代码示例：

```csharp
var getDownloadSize = Versions.GetDownloadSizeAsync(updateAsync, group.assets);
yield return getDownloadSize;
if (getDownloadSize.totalSize > 0 || updateAsync.changed)
{
    var messageBox = MessageBox.Show("提示",
        $"发现更新({Utility.FormatBytes(getDownloadSize.totalSize)})：服务器版本号 {updateAsync.version}，本地版本号 {Versions.ManifestsVersion}，是否更新？",
        null, "更新", "跳过");
    yield return messageBox;
    if (messageBox.ok)
    {
        updateAsync.Override();
        StartDownload(getDownloadSize);
        yield break;
    }
}
```

updateAsync 是之前更新版本信息时返回的对象，这表示直接使用服务器的版本信息进行检查，如果只想针对本地的版本信息进行检查可以使用这个版本：

```csharp
var getDownloadSize = Versions.GetDownloadSizeAsync(group.assets);
yield return getDownloadSize;
if (getDownloadSize.totalSize > 0)
{
    var messageBox = MessageBox.Show("提示",
        $"发现更新({Utility.FormatBytes(getDownloadSize.totalSize)})：服务器版本号 {updateAsync.version}，本地版本号 {Versions.ManifestsVersion}，是否更新？",
        null, "更新", "跳过");
    yield return messageBox;
    if (messageBox.ok)
    {
        StartDownload(getDownloadSize);
        yield break;
    }
}
```

通常，只有第一次进行版本更新才传入更新版本信息时返回的对象，后续的内容都可以使用本地的版本信息进行检查。需要注意的是 group.assets 是一组资源列表，为什么不使用 group 的名字而是使用列表呢？这主要是游戏的内容会随着时间轴的变化而变化，预先制作的东西具有一定时效性，传入一个动态列表更灵活。

## 批量下载文件

批量下载文件是一种一次下载多个文件的操作。在获取下载大小后，如果内容有更新，可以通过以下代码，启动下载：

```csharp
var downloadAsync = Versions.DownloadAsync(getDownloadSize.result.ToArray());
yield return downloadAsync;
if (downloadAsync.status == OperationStatus.Failed)
{
    var messageBox2 = MessageBox.Show("提示！", "下载失败！请检查网络状态后重试。", null);
    yield return messageBox2;
    if (messageBox2.ok)
        StartDownload(getDownloadSize);
    else
        OnComplete();
    yield break;
}
```

注：为了更快的响应异常，批量下载文件在内部是 one by one 的下载。

## 常规资源加载

常规资源指的是除了场景外所有打包 AssetBundle 的资源，对此，xasset 7.0 提供了以下接口进行加载：

- Asset.Load(Async): 同步(异步)加载

以下是加载常规资源的代码示例：

```csharp
// 加载文本
var asset = Asset.Load(pathToAsset, typeof(TextAsset));
var ta = asset.asset as TextAsset;
var text = ta.text;
// 如果需要加载文本中的二进制可以这样:
var bytes = ta.bytes;

// 加载 Sprite
var asset = Asset.Load(pathToAsset, typeof(Sprite));
var sprite = asset.asset as Sprite;

// 加载 Texture
var asset = Asset.Load(pathToAsset, typeof(Texture));
var texture = asset.asset as Texture;

// 加载 prefab
var asset = Asset.Load(pathToAsset, typeof(Object));
var prefab = asset.asset as Object;
// 如果要在场景中创建 prefab 的 GameObject 可以这样：
var gameObject = GameObject.Instantiate(prefab);

// 加载 音频
var asset = Asset.Load(pathToAsset, typeof(AudioClip));
var clip = asset.asset as AudioClip;

// 加载 ScriptableObject
var asset = Asset.Load(pathToAsset, typeof(ScriptableObject));
var so = asset.asset as ScriptableObject;
```

限于篇幅，这里暂且列举这么多，可以参考 AssetDatabase.LoadAssetAtPath 的使用方法来使用 Asset.Load(Async)。

## 加载场景

在 xasset 7.0 中，不论是 ScenesInBuild 中的场景，还是打包 AssetBundle 的场景，都是使用下面这个接口进行加载:

- Scene.LoadAsync

以下是加载场景的示例代码：

```csharp
// 加载一个 Single 场景
Scene.LoadAsync(pathToScene);

// 加载一个 叠加场景
Scene.LoadAdditiveAsync(pathToScene);
```

需要注意的是，场景加载主要包括 Additive 和 Single 两种模式，每次加载一个新的 Single 模式的场景后，之前加载的所有场景将会自动释放。

所以，Single 场景不需要主动调用 Release，而 Additive 场景如果不需要使用的时候，可以主动调用 Release 释放场景。