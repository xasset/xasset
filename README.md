# xasset-7.0 体验版

xasset-7.0 体验版是 xasset-7.0 订阅版的简化版本。

xasset-7.0 的订阅版本主要面向团队提供技术支持，体验版本和订阅版的差异可以参考：

- 版本比较 https://xasset.github.io/#/compare-plans

xasset-7.0 的订阅版本主要为 Unity 项目的包体大、打包慢、版本更新、边玩边下、运行卡顿等疑难杂症提供全面有效的解决方案。如需订阅可以联系:

- 微信：vmakemore
- 邮箱：xasset@qq.com

从开源版本到商业化，xasset 已经持续迭代了近 5 年，通过持续不断地自我迭代和突破，xasset 项目达成了：

- 1.4k+ 星标的开源项目（1.0-7.0）
- 150+ 个人付费订阅（5.1-6.1）
- 30+ 团队付费订阅（持续增长 6.1 + 7.0）
- 1k+ 用户的行业内容交流群（持续增长）
- 15+ 次创作赞助扶持（持续投入）

非常感谢新老用户的支持和鼓励！到 100+ 团队订阅的时候，我们将和大家分享：普通程序员，如何在没有 buff 的情况下，不通过项目背书、不通过商业炒作，和各种注册资金过亿、千万+、百万+ 的公司、团队或个人建立服务关系。希望我们探索出来的路，可以为更多的人提供指引。此间心路历程都是真实且没有包装的。

## 功能特性

xasset-7.0 体验版提供了以下功能特性：

- 增量打包（Unity构建管线自带机制），输出文件默认以追加hash的方式命名，天生不会有CDN同名缓存问题。
- 全量更新、加载资源自动更新，最稳定可靠的只读模式的版本管理机制。
- 简便的跨平台资源（同步和异步）、场景（异步）加载能力、~~支持异步转同步~~。
- 仿真模式：编辑器有效，只需设置好打包分组、可以跳过打包直接运行。
- 预加载模式：编辑器有效，需要先打包才能运行，直接加载最新的打包数据，不会触发更新。
- 增量模式：需要先打包才能运行，可以在编辑器下模拟和真机一样的版本更新流程。
- 离线模式：真机有效、开启后不会触发更新。

体验版没有提供分布式打包工具，以及自动分析依赖、自动优化冗余、自动解决冲突的支持。可以结合 Unity 的 [AssetBundleBrowser](https://github.com/Unity-Technologies/AssetBundles-Browser) 可视化的创建资源的 AssetBundle 打包分组。

了解 xasset-7.0 订阅版的功能特性可以参考：https://xasset.github.io/

## 环境需求

- 引擎版本：Unity2018.4+
- 语言环境：.net 4.x

## 赞助

成为 xasset 项目的赞助商可以在这里添加自己的链接，可以带 LOGO 或名字：

- [Jojohello](https://www.zhihu.com/people/jojohello)（人民币3000元）
- [马三小伙儿](https://github.com/XINCGer)（人民币1000元）

如需赞助可以联系作者微信：vmakemore。

## 声望

**小魔女纱代酱**

- https://github.com/DumoeDss
- 反馈有效问题或建议 +2

**夜莺**

- https://github.com/killop
- 反馈有效问题或建议 + 5


## 友情链接

- [ET](https://github.com/egametang/ET) Unity3D Client And C# Server Framework
- [TinaX Framework](https://tinax.corala.space/) “开箱即用”的Unity独立游戏开发工具
- [LuaProfiler-For-Unity](https://github.com/ElPsyCongree/LuaProfiler-For-Unity) Lua Profiler For Unity支持 XLua、SLua、ToLua
- [JEngine](https://github.com/JasonXuDeveloper/JEngine) 一个基于XAsset&ILRuntime，精简好用的热更框架
