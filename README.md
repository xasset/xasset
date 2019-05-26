# xasset
xasset 致力于为 Unity 项目提供了一套 精简稳健 的资源管理环境

## 主要特点
- 致力于用最少的代码实现最健全的功能，并通过 Delegate 把 Runtime 部分的业务和 Editor 部分的业务剥离，没有为了模式而模式或者为了设计而设计的累赘代码
- 自动管理 AssetBundle 的依赖的加载和卸载，在 Editor 下提供了敏捷高效的开发模式，不构建 AssetBundle 也可以加载相关的资源，对应还提供了 Runtime 模式，走构建后的加载流程
- 基于内建的引用计数机制确保资源不会重复加载和轻易卸载，并对提供了给资源设置关注对象的机制，在对象被销毁时，底层自动释放资源引用计数，当引用计数为 0 时，再自动卸载资源
- 支持 Buildin 和 AssetBundle 中的场景，以及 WWW 和 AssetBundle 中的常规资源的加载和卸载，并都提供了同步和异步的加载模式，和基于离散文件的资源版本更新机制，以及一系列的批处理打包工具 

## 接口说明
**Assets.Initialize** 

系统唯一的初始化接口，为更好的兼容 WebGL，初始化内部采用异步实现，对业务层使用输入的 Callback 函数 OnSucces 和 OnError 进行状态返回

**Assets.LoadScene/Assets.UnloadScene**

主要用来对 Buildin 和 AssetBundle 中的场景进行加载（同步/异步）和卸载。此外，对于场景的卸载，还可以通过调用加载时返回的 SceneAsset 对象的 Release 方法来卸载

**Assets.Load(Async)/Assets.Unload**

主要用来对 WWW 和 AssetBundle 中的资源进行加载（同步/异步）和卸载，同上，对于资源的卸载，还可以通过调用加载时返回的 Asset 对象的 Release 方法来卸载

**Asset.Release**

资源在 Load 后如果不想用了，就调用这个接口释放资源的引用计数，引用计数为 0 时，底层会对资源进行回收处理

**Asset.Require**

让资源关注输入的 Unity 对象，当输入对象为空时，底层会自动释放对应的引用计数，用这种方式可以避免 基于 OnDestroy 回收资源时，对象销毁时可能不触发 OnDestroy 导致资源不能正常回收的问题，这个机制借鉴了[ABSystem](https://github.com/tangzx/ABSystem) 的做法，具体可以点击链接查看 
​ 
**Asset.text/Asset.bytes**
​ 
当通过 WebAsset 加载文本/二进制 时，可以通过这个属性访问 文本/二进制 的内容

**Asset.progress**
​ 
资源加载进度

**Asset.completed**
​ 
资源加载完成时的事件回调

**Asset.isDone**
​ 
资源是否加载完成

**Asset.error**
​ 
资源加载的错误

## 操作流程
#### 打包
对于资源打包，建议集成官方的 [AssetBundleBrowser](https://github.com/Unity-Technologies/AssetBundles-Browser ) 来检查 AssetBundle 中的资源冗余情况，打包的主要流程是：

1. 标记 要打包的资源

2. 生成 AssetBundle

3. 生成 播放器

下面是框架中提供的一些辅助工具：

**Assets/AssetBundles/按目录标记**

同一个目录下的资源一个AssetBundle 
​ 
**Assets/AssetBundles/按文件标记**

每个文件一个AssetBundle，AssetBundle名称包含文件路径 

**Assets/AssetBundles/按名称标记**

每个文件一个AssetBundle，AssetBundle名称不包含文件路径

**Assets/AssetBundles/生成配置**

对工程中的所有打包 AssetBundle 的资源生成 Manifest 文件，主要用来寻址

**Assets/AssetBundles/生成資源包**

将所有标记过的资源生成对应的 AssetBundle

**Assets/AssetBundles/生成播放器**

针对当前平台生产对应的可执行文件，例如 exe，apk，ipa(需要在安装了xcode的mac下进行)等 

#### 寻址
所有资源加载，会优先判断资源是否在 AssetBundle 中，然后再根据具体的资源类型分别执行对应的操作： 
1. 对于场景，不在 AssetBundle 中就从 Buildin 中的数据加载    
2. 对于常规资源，路径中以这些符号 http:// https:// file:// ftp:// 开头的，在AssetBundle 中的，会基于 WWW 的 AssetBundle 资源加载, 不在的则基于 WWW 的普通资源加载

#### 更新
版本更新主要包括以下 3 个状态, 可以在框架中的 Demo 场景查看
1. **Check** 

    读取本地和远程资源版本，对比生成需要的下载的资源列表，如果列表的文件数量 > 0, 执行 Download，否则执行 Complete

2. **Download**

    依次下载列表中的资源文件，下载完成后，完成更新执行 Complete

3. **Complete**

    如果有下载更新，将下载的版本记录写入本地，并重新初始化完成资源热更


## 技术支持
Email: jiyuan.feng@live.com

QQ-group: 693203087

## 贡献成员
- [oucfeng](https://github.com/oucfeng)
- [wl-666](https://github.com/wl-666)
- [backjy](https://github.com/backjy)
- [CatImmortal](https://github.com/CatImmortal)
- [RoneBlog](https://github.com/RoneBlog)