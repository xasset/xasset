**新版发布(需要订阅才能获取源码)**

# XASSET 5.1

快速强大的Unity资源系统。

一种最轻便高效和灵活的Unity资源打包，更新，加载，释放方式。

- 官网：https://game4d.cn

- 演示视频：https://www.zhihu.com/people/xasset/zvideos

## 主要特点

- 一键打包，收集资源后自动分析依赖，自动优化冗余，自动解决冲突。大小包出包，配置可控，快速切换，自动按需复制资源到包体，最小包控制在36MB以内，操作简单，快速上手。
- 按需更新，批量下载，断点续传，版本校验，异常复原，自动管理依赖，已经下载的内容无需重复下载，下载后就能使用，轻松查询下载内容的大小进度和速度，功能齐全、使用方便。
- 智能寻址，不论资源是在本地还是服务器，不论资源是在包内还是包外，不论资源是在iOS，Android，Windows，Mac OS等平台，加载资源和场景的逻辑代码，一次编写，到处运行。
- 敏捷开发，编辑器下提供了开发模式支持，无需打包也能快速运行项目，并支持录制模式，通过自动采集资源加载记录，即使成千上万的资源要按需更新，也可，精确定位，轻松处理。
- 稳定可靠，底层采用引用计数进行内存管理，自动管理依赖的加载和释放，即使处理复杂的循环依赖，也能在Profiler中，看到“进多少，出多少”的数据采样表现，根基稳健，更耐考验。

## 为什么订阅
### 大厂大作用户

*“（订阅之前）其实我大概花了有半个月从头写整理项目的资源管理打包这一块然后越搞越多 每个想到遇到的问题 xasset几乎都有现成的并且后续需要分包 剥离resource 也立刻支持了都很符合我项目的需求 我就先花1-2天直接接进来跑给pm看后面就直接说服了 全面转xasset管理资源这一块了”*——来自大厂大作的订阅用户寒晟的真实反馈。

### 独立游戏用户

*“曾经自己写过使用AssetBundle实现热更的框架，发现有各种各样的内存问题。接入XASSET之后。这一切的问题都轻而易举的解决了； 我曾尝试过5w一年的热更新解决方案，和XASSET在热更环节体验上简直没有区别， 但是XASSET提供了更完善的Unity资源加载和内存管理环境，真的非常棒！”*——来自独立游戏的订阅用户Jason的真实反馈。

## 订阅和非订阅的区别

简单来说4.x趋向于Demo，而5.1趋向于Ready to use.

功能特性上，5.1更适合对按需更新以及大小包出包稳定性有更高追求的项目。

最大的价值是，订阅可以更快获得可以参考或直接使用的更好的解决方案。

## 订阅的权益

源码+1年更新支持，1年后不再提供更新支持但是依旧可以在项目中使用，订阅是单项目授权，所以没有取得特殊授权的时候，一份订阅只能在一个商业项目中使用。

了解更多请前往：https://game4d.cn.

# XASSET 4.0.2

精简实用、高效安全的Unity资源管理方案。
- Github：<https://github.com/xasset/xasset>
- QQ群：[693203087](https://jq.qq.com/?_wv=1027&k=5DyV09a)

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
