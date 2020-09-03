XASSET 5.1已经发布

XASSET 5.1为Unity项目提供了可以快速投入到生产环境中使用的具有更智能和灵活的资源分包、热更新机制和稳健高效的资源加载和内存管理的资源管理方案。它不仅可以服务于快速生产以及更有针对性的高效出包，更能在一些细微之处为你的项目保驾护航，具体请看：[XASSET Pro订阅指南](https://zhuanlan.zhihu.com/p/176988029)

# XASSET 4.0

为你提供更精简、高效、安全的Unity资源管理方案。

- 官网：<https://www.xasset.org>
- Github：<https://github.com/xasset/xasset>
- QQ群：[693203087](https://jq.qq.com/?_wv=1027&k=5DyV09a)

## 新特性

- Updater 支持大小包版本更新检查的程序，大包是把基于VFS技术生成的一种包含了所有资源文件的大文件，需要开启VFS才能使用，不开启或者已经下载过大包之后，只会进行小包更新。
- VFS 自定义格式的文件读写支持，实现了一种非常高效的资源加密方案，并且在测试的Android设备上改方案可以让资源加载的IO性能得到可观的提升。
- Versions 支持任意格式的资源文件的版本管理，可以非常方便的对Wwise、Fmod等自定义格式的文件进行版本控制。
- Downloader 支持断点续传、指纹校验、异常复原（下载失败后自动重新下载）和并发数量配置的资源下载组件。
- SearchPath 更智能的寻址机制。
- Lazy GC 更高效的资源回收策略。

## 主要特点

- 开发模式：编辑器下可以在不用构建AB的环境中使用，平常开发时可以无需为漫长的Build过程耗费大量的时间。
- 对协程无依赖：相对于高度依赖协程的方案，这种设计不但在性能上更有优势，同时，业务层可以用更少的Coding写出更优雅高效的并行异步加载业务。
- 用引用计数管理对象生命周期：避免重复加载与轻易卸载，让资源对象的生命周期得到妥善处理。并且没有使用WeakReference，可以更方便在跨语言环境中使用，例如避免Lua和C#的交叉引用导致C#这边需要等Lua先GC才能回收资源。
- 基于规则配置的打包策略，配置好打包规则后，底层会自动收集所有要打包的资源，并分析其冗余和冲突，再进行自动优化，可以有效的解决大部分非内建的资源的冗余情况。
- 非泛型接口设计: 对Lua更友好，可以更方便的在跨语言的环境中使用。

## 了解更多

- 知乎专栏：[XASSET 4.0发布预告](https://zhuanlan.zhihu.com/p/158040305)
- 知乎专栏：[XASSET 4.0入门指南](https://zhuanlan.zhihu.com/p/69410498)
- 知乎专栏：[XASSET Pro 订阅指南](https://zhuanlan.zhihu.com/p/176988029)

## 测试数据

| VFS在各个平台的IO+对象构建性能（毫秒）        | VFS     | BUILDIN |
| --------------------------------------------- | ------- | ------- |
| PC（Win10+i7=10700F CPU2.9GHz 64位 16GB内存） | 310.76  | 354.88  |
| Android（Sony XZs）                           | 2179.63 | 2740.27 |
| iOS（iPhone 7）                               | 629.10  | 593.74  |
| MacBook Pro（macOS15.5 2.9GHz 6核 i9 64位 32GB内存） | 180.29  | 181.90  |

*注*：大约读取了 662 张贴图资源

## 开发环境

- 引擎版本：Unity2017.4.34f1（已经支持2019）
- 语言环境：.net 3.5（支持.net4.x以及.net core）
- 操作系统：Win 10

## 贡献成员

- [yusjoel](https://github.com/yusjoel)
- [hemingfei](https://github.com/hemingfei)
- [veboys](https://github.com/veboys)
- [woshihuo12](https://github.com/woshihuo12)
- [CatImmortal](https://github.com/CatImmortal) 
- [ZhangDi](https://github.com/ZhangDi2018)
- [QuinShuai](https://github.com/QuinShuai)
- [songtm](https://github.com/songtm)
- [woodelfLee](https://github.com/woodelfLee)
- [LostEarth](https://github.com/LostEarth)
- [Coeur](https://github.com/Coeur)
- [XINCGer](https://github.com/XINCGer)
- [烟雨迷离半世殇](https://www.lfzxb.top/)
- [土豆](https://www.xasset.org/)
- [JasonXuDeveloper](https://github.com/JasonXuDeveloper)
- [大魔王有木桑](https://github.com/yomunsam)
- [suixin567](https://github.com/suixin567)
- [Sven](https://github.com/SvenCheung)
- [liufujingwen](https://github.com/liufujingwen)

## 鸣谢

感谢JetBrains公司提供的使用许可证！

<p><a href="https://www.jetbrains.com/?from=NKGMobaBasedOnET ">
<img src="https://images.gitee.com/uploads/images/2020/0722/084147_cc1c0a4a_2253805.png" alt="JetBrains的Logo" width="20%" height="20%"></a></p>

## 更多项目

- [ET](https://github.com/egametang/ET) Unity3D Client And C# Server Framework
- [QFramework](https://github.com/liangxiegame/QFramework) Your first K.I.S.S Unity 3D Framework
- [TinaX Framework](https://tinax.corala.space/) “开箱即用”的Unity独立游戏开发工具
- [LuaProfiler-For-Unity](https://github.com/ElPsyCongree/LuaProfiler-For-Unity) Lua Profiler For Unity支持 XLua、SLua、ToLua
- [JEngine](https://github.com/JasonXuDeveloper/JEngine) 一个基于XAsset&ILRuntime，精简好用的热更框架
