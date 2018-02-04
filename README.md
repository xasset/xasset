# XAsset 

XAsset 为 Unity 项目提供了一套"简便"的资源管理环境，借助 XAsset，你可以很方便的对Unity项目中的 AssetBundle 资源进行打包，加载，释放和优化。

GitHub: https://github.com/fengjiyuan/XAsset （PS: 如果觉得好用请Star一个）

##### 主要特点

1. 对于资源加载

   * 逻辑层可以不用关注 AssetBundle，加载资源只需要提供资源的路径和类型，内部会自动处理资源的依赖和变种加载逻辑。内建了基于引用计数的缓存策略，确保同一份资源不会被重复加载亦不会轻易卸载。
   * 进行资源优化调整 AssetBundle 的打包粒度时，通常只要资源路径不变就不会对逻辑层（进行资源加载和卸载的代码）造成影响，维护成本相对较少。
   * 支持开发模式和 Bundle 模式，在编辑器环境中，开发模式下不用构建 AssetBundle 也能加载到想要的资源，可以加快开发效率。 同时，也可以通过启动 Bundle 模式，进行真实环境下的 AssetBundle 加载。

2. 对于资源打包

   * XAsset 会自动把所有要打包的资源所依赖的公共资源剥离出来，先对公共资源进行打包然后再把非公共资源按当前规则进行独立打包，尽量避免同一资源在多个 AssetBundle 中冗余。同时，只要开启 BUILD_ATLAS 宏， XAsset 也会自动进行图集打包。
   * XAsset 中预定义了一系列的 BuildRule，可以很方便的对 Unity 项目中的 Prefab、Texture、Material、TextAsset 等资源进行收集和打包，只需按需配置 BuildRule，然后执行 "Assets/XAsset/Build AssetBundles" 就可以按定义的规则列表进行一键资源打包。
   * 为简化出包流程，XAsset 也还提供了一键输出程序包的命令，执行 "Assets/XAsset/Build Player" 可以很方便的一键输出 apk、app or exe 程序文件，文件名会根据 Unity 项目中 PlayerSettings 的 ProductName 和 BundleVersion 自动生成。


**环境需求**

XAsset 基于 Unity2017.2.0 进行开发，不过也可以通过导出源码源码的方式在低版本的Unity项目中运行，但是由于 AssetBundle.LoadFromFile（Async） 在 Android 上需要 Unity5.4 才能正常运行，所以不能低于此版本，建议不要使用5.4.2，过来人经验5.6相对比较稳定。

##### 使用范例 ##### 

使用 XAsset 资源管理 API 进行资源加载和卸载

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

> ***注：XAsset 提供了同步/异步两种加载模式，但是为了功能能够正常运转，对于同一个资源的加载，请不要在异步加载没有完成前进行同步加载，否则同步加载的资源将得不到正常的返回。***

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

  > ***注：编辑器下未激活 Bundle Mode 的时候都是同步加载***


- XAsset/AssetsTest.cs

  使用 Assets API 进行资源加载（同步/异步）和卸载的示范。


- XAsset/Editor/AssetsMenuItem.cs

   编辑器 “Assets” 菜单的定义，主要包含以下功能：

   > "Assets/Copy Asset Path" 
   >
   > ​	复制资源的在工程中的相对路径
   >
   > "Assets/XAsset/Bundle Mode" 
   >
   > ​	编辑器下用来 激活（勾选）或关闭（反选） Bundle Mode
   >
   > "Assets/XAsset/Build AssetBundles"
   >
   > ​	构建 AssetBundles
   >
   > "Assets/XAsset/Build Player"
   >
   > ​	构建 程序包，iOS模式下会导出 xcode 工程

- XAsset/Editor/BuildRule.cs

   打包规则，实现了资源收集和打包（包括图集）策略，目前主要预定义了以下规则：

   > BuildAssetsWithAssetBundleName 
   >
   > ​	将搜索到的所有资源按指定的 AssetBundleName 进行打包。
   >
   > BuildAssetsWithDirectroyName 
   >
   > ​	将搜索到的所有资源按资源所在的路径进行打包，同一个路径下的所有资源会被打到一个包。
   >
   > BuildAssetsWithFilename
   > ​	将搜索到的所有资源按每个资源的文件名进行打包，每个文件一个包。
   >
   > ***注意：以上规则默认都会将规则中每个资源的同其非共享的所有依赖的资源打到同一个包***

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
1. [Assets, Resources and AssetBundles](https://unity3d.com/cn/learn/tutorials/s/best-practices ) 
2. [如何用XAsset进行较理想的Unity项目资源打包方案？](./如何用XAsset进行较理想的Unity项目资源打包方案.md)

##### 技术支持 #####

QQ群：693203087

##### 更新日志
20180204 - fjy
1. 增加 ReleaseAssetOnDestroy 组件可以用来自动回收场景对象的资源，用法参考 AssetsTest
2. 增加 Assets 资源回收逻辑，修改之前预制件中静态引用的贴图，在销毁并卸载预制件后，在Profiler 中没有正常回收的问题
3. 优化编码，尽可能把编辑器相关逻辑和 Runtime 逻辑分开，方便后续构建 dll，gendll.sh 可以用来在 mac 下生成 xasset 的 dll
