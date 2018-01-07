# XAsset 

XAsset 为 Unity 项目提供了一套"简便"的资源管理环境，借助 XAsset，你可以很方便的对Unity项目中的 AssetBundle 资源进行构建，加载，释放。 

Github https://github.com/fengjiyuan/XAsset

##### 主要特点

1. 对于资源加载

   * 逻辑层可以不用关注 AssetBundle，加载资源只需要提供资源的路径和类型，内部会自动处理资源的依赖和变种加载逻辑。内建了基于引用计数的缓存策略，确保同一份资源不会被重复加载亦不会轻易卸载。
   * 支持开发模式和 Bundle 模式，在编辑器环境中，开发模式下不用构建 AssetBundle 也能加载到想要的资源， 同时，也可以通过启动 Bundle 模式，从构建后的 AssetBundle 中加载资源。

2. 对于资源打包

   * XAsset 中预定义了一系列的 BuildRule，可以很方便的对 Unity 项目中的 Prefab、Texture、Material、TextAsset 等资源进行打包，只需按需配置 BuildRule，然后执行 "Assets/XAsset/Build AssetBundles" 就可以按定义的规则列表进行一键资源打包，对于预定义的规则，下面的核心文件会有进一步说明。
   * 为简化出包流程，XAsset 也还提供了一键输出程序包的命令，执行 "Assets/XAsset/Build Player" 可以很方便的一键输出 apk、app or exe 程序文件，文件名会根据 Unity 项目中 PlayerSettings 的 ProductName 和 BundleVersion 自动生成。



##### 演示范例 ##### 

以下是使用 XAsset 资源管理 API 进行资源加载和卸载的范例：

```c#
IEnumerator LoadAsset ()
{
	string assetPath = "Assets/SampleAssets/MyCube.prefab"; 
	/// 同步模式用路径加载资源
	var asset = Assets.Load<GameObject> (assetPath);
	if (asset != null && asset.asset != null) {
		var go = GameObject.Instantiate (asset.asset);
		GameObject.Destroy (go, 1);
	}
	/// 卸载
	asset.Unload ();
	asset = null; 

	/// 异步模式加载
	var assetAsync = Assets.LoadAsync<GameObject> (assetPath);
	if (assetAsync != null) {
		yield return assetAsync;
		if (assetAsync.asset != null) {
			var go = GameObject.Instantiate (assetAsync.asset);
			GameObject.Destroy (go, 1);
		} else {
			Debug.LogError (assetAsync.error);
		} 
		assetAsync.Unload ();
		assetAsync = null;
	}
}
```

##### 核心文件 #####
- XAsset/Assets.cs 

  提供了资源管理相关 API，让用户不需要关注 AssetBundle。主要提供了一下接口：

  - Initialize 

     初始化

  - Load 

     同步加载，阻塞主线程

  - LoadAsync 

     异步加载，不阻塞主线程

  - Unload 

     卸载资源，也可以参考上面的演示范例通过调用返回的 Asset 对象的 Unload 卸载资源

  > 注：编辑器下未激活 Bundle Mode 的时候都是同步加载


- XAsset/AssetsTest.cs

  使用 Assets API 进行资源加载（同步/异步）和卸载的示范。


- XAsset/Editor/AssetsMenuItem.cs

   编辑器 “Assets” 菜单的定义，主要包含以下功能：

  - "Assets/Copy Asset Path" 

     复制资源的在工程中的相对路径

  - "Assets/XAsset/Bundle Mode" 

     编辑器下用来 激活（勾选）或关闭（反选） Bundle Mode

  - "Assets/XAsset/Build AssetBundles"

     构建 AssetBundles

  - "Assets/XAsset/Build Player"

     构建 程序包

- XAsset/Editor/BuildRule.cs

   打包规则，定义了资源收集和打包策略，目前主要预定义了以下规则：

  - BuildAssetsWithAssetBundleName 
    将搜索到的所有资源按指定的 AssetBundleName 进行打包。

  - BuildAssetsWithDirectroyName 

    将搜索到的所有资源按资源所在的路径进行打包，同一个路径下的所有资源会被打到一个包

  - BuildAssetsWithFilename
    将搜索到的所有资源按每个资源的文件名进行打包，每个文件一个包。

    > 注：此规则内部会对每个资源收集依赖，如果依赖的资源没有被其他资源所引用，那么依赖的资源将和自己一起打包，如果依赖的资源有被其他资源所引用，那个依赖的资源会被单独打包。

- XAsset/Bundles.cs 

  封装了 AssetBundle 的依赖和变种的加载和释放的接口实现。

- XAsset/Manifest.cs

  配置文件，用来记录每个 AssetBundle 包含的所有文件。

- XAsset/Editor/BuildScript .cs

  打包脚本，实现了一键出包的主要流程。

- XAsset/Utility.cs 

  辅助工具。

- XAsset/Logger.cs 

  日志工具。

##### 推荐阅读 #####
1. Assets, Resources and AssetBundles https://unity3d.com/cn/learn/tutorials/s/best-practices 

##### FAQ

后续再逐步补充

##### 技术支持 #####

QQ群：693203087
