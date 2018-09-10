# XAsset 

XAsset 为 Unity 项目提供了一套简便的资源管理环境，借助 XAsset，你可以很轻易的在 Unity 项目中对 AssetBundle 资源进行 打包、更新、加载、和回收。

#### 主要特点
* 自动化的资源依赖和生命周期管理：XAsset 内部会自动处理资源的依赖和变种加载逻辑，同时利用了基于引用计数的资源依赖加载策略，让同一份资源不会被重复加载亦不会轻易卸载，从而让资源管理变得更简单稳健，让大家不在对一大堆资源依赖和生命周期管理烦恼。

* 敏捷化的编辑器仿真资源加载模式：XAsset 的资源加载支持开发模式和 Bundle 模式，在开发模式下不用构建 AssetBundle 也能加载到想要的资源。同时，也可以通过启动 Bundle 模式，在编辑器下进行真实的 AssetBundle 资源加载测试，让开发效率更高。

* 简便化的运行时工程资源加载机制：XAsset 使用资源在工程的相对路径取得资源，可以让逻辑层不用关注 AssetBundle，同时提供了资源路径转换代理接口，让逻辑层可以通过实现该代理接口，就能通过同一个地址获取本地或 WebServer 上的资源，让使用成本更低。

* 批量化的可配置资源打包构建流程：XAsset 提供了一系列可配置规则的批量打包工具，在打包时会自动收集资源的依赖信息，把公共资源剥离出来单独打包，从而避免冗余。同时，只要开启 BUILD_ATLAS 宏，就会自动进行图集打包，具有一定参考价值。

#### 环境需求
XAsset 基于 Unity2017.2.0 进行开发，不过也可以通过导出源码源码的方式在低版本的Unity项目中运行，但是由于 AssetBundle.LoadFromFile（Async） 在 Android 上需要 Unity5.4 才能正常运行，所以不能低于此版本，建议不要使用5.4.2，过来人经验5.6相对比较稳定。

#### 使用范例 

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

为简化出包流程，XAsset 也还提供了一键输出程序包的命令，执行 "Assets/XAsset/Build Player" 可以很方便的一键输出 apk、app or exe 程序文件，文件名会根据 Unity 项目中 PlayerSettings 的 ProductName 和 BundleVersion 自动生成。


#### 核心文件

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

- XAsset/Editor/BuildScript.cs

  打包脚本，实现了一键出包的主要流程。

- XAsset/Utility.cs 

  辅助工具。

- XAsset/Logger.cs 

  日志工具。

##### 推荐阅读 #####
1. [Assets, Resources and AssetBundles](https://unity3d.com/cn/learn/tutorials/s/best-practices ) 
2. [如何用XAsset进行较理想的Unity项目资源打包方案？](./如何用XAsset进行较理想的Unity项目资源打包方案.md)
3. [如何用XAsset进行资源更新](./如何用XAsset进行资源更新.md)

##### 技术支持 #####

QQ群：693203087
