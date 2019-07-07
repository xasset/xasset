# xasset
xasset 致力于为 Unity 项目提供了一套 精简稳健 的资源管理环境

**主要特点**

- 自动管理依赖的加载和卸载，循环依赖下可以正常运行
- 接管了场景以及常规资源的加载（同步/异步）和卸载，逻辑开发无需关注 AssetBundle
- 基于引用计数管理资源对象生命周期，避免重复加载和轻易卸载
- 支持编辑器模式，不构建 AssetBundle 也可正常使用，开发效率高
- 集成了官方的 [AssetBundleBrowser](https://docs.unity3d.com/Manual/AssetBundles-Browser.html)，支持可视化的资源冗余预警，以及打包粒度调整
- 提供了支持断点续传的资源版本更新Demo

**未来计划**

计划提供资源性能预警工具，对单个资源的 内存/加载/渲染 开销进行真机采样，然后收集 prefab 的依赖并根据真机采样的数据，进行 Runtime 时的性能预警，把资源的性能问题在制作时提前发现提前处理，感兴趣的朋友可以加入技术支持群，一起交流探讨

**使用范例**

```c#
void Start()
{
    /// 初始化
    Assets.Initialize(OnInitialized, (error) => { Debug.Log(error); }); 
}

private void OnInitialized()
{
    var asset = Assets.Load(assetPath, typeof(UnityEngine.Object));
    asset.completed += delegate(Asset a) 
    {
        if (a.name.EndsWith(".prefab", StringComparison.CurrentCulture))
        {
            var go = Instantiate(a.asset);
            go.name = a.asset.name;
            /// 设置关注对象，当关注对象销毁时，回收资源
            a.Require(go); 
            Destroy(go, 3);
            /// 设置关注对象后，只需要释放一次，可以按自己的喜好调整，
            /// 例如 ABSystem 中，不需要 调用这个 Release，
            /// 这里如果之前没有调用 Require，下一帧这个资源就会被回收
            a.Release();   
        }
    };
} 
```

以上代码，可以在工程的 Assets/Demo/Scripts/Init.cs 中找到，更多参考: [xasset 框架入门指南](https://zhuanlan.zhihu.com/p/69410498)

**测试环境**

Unity 5.6.7 or new

**技术支持**

QQ群: [693203087](https://jq.qq.com/?_wv=1027&k=5DyV09a) （可点击加入）

**贡献成员**

- [hemingfei](https://github.com/hemingfei): v2 解决下载有新增资源包文件报空指针的问题
- [yusjoel](https://github.com/yusjoel): v2 处理Path.GetDirectoryName()获取的路径在.Net 3.5和.Net 4.0下斜杠不一致的问题
- [veboys](https://github.com/veboys): v1 WEBGL兼容性支持 
- [woshihuo12](https://github.com/woshihuo12): v1 修正编辑器下assetbundle模式报错的问题
- [CatImmortal](https://github.com/CatImmortal): v2 WebAsset底层支持UnityWebRequest 
- [ZhangDi](https://github.com/ZhangDi2018): v1 Tiny improve

**友情链接**

 - [ET](https://github.com/egametang/ET) Unity3D Client And C# Server Framework
 - [QFramework](https://github.com/liangxiegame/QFramework) Your first K.I.S.S Unity 3D Framework
