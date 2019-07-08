# xasset
xasset 致力于为 Unity 项目提供了一套 精简稳健 的资源管理环境

**主要特点**

- 自动管理依赖的加载和卸载，循环依赖下可以正常运行
- 接管了场景以及常规资源的加载（同步/异步）和卸载，逻辑开发无需关注 AssetBundle
- 基于引用计数管理资源对象生命周期，避免重复加载和轻易卸载
- 支持编辑器模式，不构建 AssetBundle 也可正常使用，开发效率高
- 集成了官方的 [AssetBundleBrowser](https://docs.unity3d.com/Manual/AssetBundles-Browser.html)，支持可视化的资源冗余预警，以及打包粒度调整
- 提供了支持断点续传的资源版本更新Demo
- 异步加载模式底层最大并发请求数量可配置

**未来计划**

计划提供资源性能预警工具，对单个资源的 内存/加载/渲染 开销进行真机采样，然后收集 prefab 的依赖并根据真机采样的数据，进行 Runtime 时的性能预警，把资源的性能问题在制作时提前发现提前处理，感兴趣的朋友可以加入技术支持群，一起交流探讨

**使用范例**

1. 资源初始化

   以下代码，可以在工程的 Assets/Demo/Scripts/AssetsInit.cs 中找到
  
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

2. 资源版本更新

   这里主要说明如何基于 Demo 场景进行测试资源版本更新

   首先，资源打包后，把 AssetsMenuItem 的 OnIntialize 替换为下面的样子：

   ```c#
   [InitializeOnLoadMethod]
   private static void OnInitialize()
   {
       var settings = BuildScript.GetSettings();
       if (settings.localServer)
       {
           bool isRunning = LaunchLocalServer.IsRunning();
           if (!isRunning)
           {
               LaunchLocalServer.Run();
           } 
       }
       else
       {
           bool isRunning = LaunchLocalServer.IsRunning();
           if (isRunning)
           {
               LaunchLocalServer.KillRunningAssetBundleServer();
           }
       }
       //Utility.dataPath = System.Environment.CurrentDirectory;
       Utility.downloadURL = BuildScript.GetManifest().downloadURL;
       Utility.assetBundleMode = settings.runtimeMode;
       Utility.getPlatformDelegate = BuildScript.GetPlatformName;
       Utility.loadDelegate = AssetDatabase.LoadAssetAtPath;
       assetRootPath = settings.assetRootPath;
   }
   ```

   其次，上面的改动主要是注释了 `Utility.dataPath = System.Environment.CurrentDirectory;`这行代码，这样在编辑器下，xasset 会从 StreamingAssets 目录下读取资源，所以，对于构建后的资源包，我们需要执行 `Assets/AssetBundles/拷贝到StreamingAssets`复制到这个目录下

   复制后，在编辑器下启动 Demo 场景，点击 Check 后，应该不会触发资源版本更新，停止播放后，可以修改已有的资源，例如在现有的 prefab 中添加新的内容，或者直接添加一些新的资源（这里是添加了一个新的 prefab），执行标记打包后，再次启动 Demo 场景，点击 Check 后，单步调试可以看到以下画面： ![update1](https://github.com/xasset/xasset/blob/master/Doc/update1.png)

   更新完成后，Demo 中会提示更新了几个文件，如下图所示： ![update2](https://github.com/xasset/xasset/blob/master/Doc/update2.png)

   最后，以上就是基于 Demo 场景进行资源版本更新的主要流程，更多演示请参考: [xasset 框架入门指南](https://zhuanlan.zhihu.com/p/69410498)

**测试环境**

引擎版本：Unity 5.6.7 / Unity2017.4 / Unity 2018.4

语言环境：.net 3.5/.net 4.0 (4.0版本有路径问题，如果发现有报错可以先切回 3.5 环境)

操作系统：macOS 10.14.5 

**技术支持**

QQ群: [693203087](https://jq.qq.com/?_wv=1027&k=5DyV09a) （可点击加入）

**贡献成员**

- [hemingfei](https://github.com/hemingfei): v2 解决下载有新增资源包文件报空指针的问题，AssetsUpdate.cs 中 更新完成后 completed 两次的问题
- [yusjoel](https://github.com/yusjoel): v2 处理Path.GetDirectoryName()获取的路径在.Net 3.5和.Net 4.0下斜杠不一致的问题
- [veboys](https://github.com/veboys): v1 WEBGL兼容性支持 
- [woshihuo12](https://github.com/woshihuo12): v1 修正编辑器下assetbundle模式报错的问题
- [CatImmortal](https://github.com/CatImmortal): v2 WebAsset底层支持UnityWebRequest 
- [ZhangDi](https://github.com/ZhangDi2018): v1 Tiny improve

**友情链接**

 - [ET](https://github.com/egametang/ET) Unity3D Client And C# Server Framework
 - [QFramework](https://github.com/liangxiegame/QFramework) Your first K.I.S.S Unity 3D Framework
