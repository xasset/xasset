**新版发布(主要面向团队提供订阅服务)**

# XASSET 6.1

xasset-6.1 是一个全新的分布式 Unity，主要为 Unity 项目的打包慢、包体大、局部更新、边玩边下、版本管理、内存管理、依赖管理、运行卡顿之类的疑难杂症提供全面可靠的解决方案。需要了解更多，请参考：

- 官网：https://game4d.cn/xasset.php
- 入门指南：https://zhuanlan.zhihu.com/p/369974901
- 特性说明：https://zhuanlan.zhihu.com/p/364058188
- 作者微信：vmakemore
- QQ群：[693203087](https://jq.qq.com/?_wv=1027&k=5DyV09a)

# XASSET 4.0.2（过时了，预计7月后更新）

精简实用、高效安全的Unity资源管理方案。
- Github：<https://github.com/xasset/xasset>

## 主要特点

- 开发模式：编辑器下可以在不用构建AB的环境中使用，平常开发时可以秒进游戏。
- 支持异步加载到同步加载的无缝切换，对协程无依赖：相对于高度依赖协程的方案，这种设计不但在性能上更有优势，同时，业务层可以用更少的Coding写出更优雅高效的并行异步加载业务。
- 用引用计数管理对象生命周期：避免重复加载与轻易卸载，让资源对象的生命周期得到妥善处理。并且没有使用WeakReference，可以更方便在跨语言环境中使用，例如避免Lua和C#的交叉引用导致C#这边需要等Lua先GC才能回收资源。
- 基于规则配置的打包策略，配置好打包规则后，底层会自动收集所有要打包的资源，并分析其冗余和冲突，再进行自动优化，可以有效的解决大部分非内建的资源的冗余情况。
- 非泛型接口设计: 对Lua更友好，可以更方便的在跨语言的环境中使用。

## 了解更多

- 知乎专栏：[XASSET 4.0发布预告](https://zhuanlan.zhihu.com/p/158040305)
- 知乎专栏：[XASSET 4.0入门指南](https://zhuanlan.zhihu.com/p/69410498)

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