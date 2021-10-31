# 内存管理

## 引用计数

引用计数主要用来防止对象重复加载，以及使用中的对象被提前卸载。

xasset 7.0 的引用计数规则比较简单：

- Load 的时候默认引用计数会自增 1
- Release 的时候引用计数会自减 1

此外，如果对象加载失败，底层默认会自动 Release 一次。

所以，在使用 xasset 7.0 进行资源管理的时候，一般只要不踩 Unity 的这些坑：

- Android 下，Camera 引用 AssetBundle 中的 RendererTexture，需要先把 Camera.targetTexture 置空后，才能正常回收。 

只要确保 Load 的次数和 Release 的次数一致，资源就会得到正常回收。

## 生命周期

生命周期指的是对象的创建使用和回收的过程。创建是生命周期的开始，回收意味着生命周期的结束。

对象的生命周期通常可以分为以下几种：

- 常驻类型：创建出来后，直到应用程序退出才回收。
- 局部长期使用：创建出来后，直到场景回收、界面回收、特效回收、模型回收后才回收。
- 先进先出使用：创建后，放到一个固定数量的队列中，队列中的对象数量要溢出时，回收最先入队的对象。

为了让程序的内存管理进出可控，并让业务开发减少一些固定规律的编码，一些业务对象的基类，例如:

- App
- Scene
- View
- Unit

之类的对象，可以组合具备先进先出缓存和预加载缓存的内存管理组件，以下是示例代码：

```csharp
public class AssetManager
{
    private readonly Dictionary<string, Asset> cache = new Dictionary<string, Asset>();
    private readonly List<Asset> preload = new List<Asset>();
    private readonly Queue<Asset> queue = new Queue<Asset>();
    private int queueSize = 10;

    public void SetQueueSize(int size, bool autoMaxSize = true)
    {
        queueSize = autoMaxSize ? Math.Max(queueSize, size) : size;
    }

    public Asset Enqueue(string path, Type type, Action<Asset> completed = null)
    {
        if (queue.Count >= queueSize)
        {
            var first = queue.Dequeue();
            first.Release();
        }

        var asset = Asset.LoadAsync(path, type, completed);
        queue.Enqueue(asset);
        return asset;
    }

    public Asset Preload(string path, Type type, Action<Asset> completed = null)
    {
        if (cache.TryGetValue(path, out var value)) return value;

        value = Asset.LoadAsync(path, type, completed);
        cache.Add(path, value);
        preload.Add(value);
        return value;
    }

    public T GetAsset<T>(string path) where T : Object
    {
        return cache.TryGetValue(path, out var value) ? value.Get<T>() : null;
    }

    public float GetProgress()
    {
        var loaded = 0;
        foreach (var asset in preload)
            if (asset.isDone)
                loaded++;

        return loaded * 1f / preload.Count;
    }

    public void Clear()
    {
        foreach (var asset in queue) 
            if (string.IsNullOrEmpty(asset.error)) 
                asset.Release();

        queue.Clear();

        foreach (var item in preload)
        {
            if (item == null) continue;
            if (string.IsNullOrEmpty(item.error)) item.Release();
        }

        preload.Clear();
        cache.Clear();
    }
}
```

当 App, Scene, View, Unit 之类的对象创建后，在其内部组合一个 AssetManager 这样的对象，相应对象内部的资源加载直接通过 AssetMananger 对象按需加载。对象销毁时，执行下 AssetMananger 对象的 Clear 操作。这时，在业务层大部分的资源生命周期都可以被 AssetManager 对象自动管理起来，内存管理自然会变得井然有序。

以下是在 Unity 项目做好内存管理需要注意的事项：

- Instantiate 需要与 Destroy 配对(默认切场景的时候，非 DontDestroyOnLoad 的对象会自动 Destroy)
- Load 需要和 Release（Unload） 配对：xasset 是 Release，Unity 的 Resources 是 UnloadAsset
- WWW 不用之后需要 Dispose