<p align="center">
  <a href="https://game4d.cn/">
    <img src="https://game4d.cn/images/logo.png" alt="xasset logo" width="200" height="200">
  </a>
</p>

<h3 align="center">xasset</h3>

<p align="center">
  更快，更轻松的解决 Unity 项目的打包慢、包体大、资源更新、内存管理和运行卡顿之类的疑难杂症。
  <br>
  <a href="https://xasset.github.io"><strong>浏览文档 »</strong></a>
  <br>
  <br>
  <a href="https://github.com/xasset/xasset/issues/new?template=bug_report.md">报告问题</a>
  ·
  <a href="https://github.com/xasset/xasset/issues/new?template=feature_request.md">提交需求</a> 
</p>


## xasset 7.0

xasset 7.0 是全面可靠的 Unity 资源系统。从开源版本到商业化，xasset 已经持续迭代了近 5 年，通过持续不断的自我迭代和突破 xasset 项目达成了：

- 1.4k+ 星标的开源项目（1.0-至今）
- 150+ 个人付费订阅（5.1-6.1）
- 35+ 团队付费订阅（持续增长 6.1-至今）
- 1k+ 用户的行业内容交流群（持续增长）
- 15+ 次创作赞助扶持（持续投入）

非常感谢新老用户的支持和鼓励！到 100+ 团队订阅的时候，我们将和大家分享：

——普通技术，如何在没有 buff 的情况下，不通过项目背书、不通过商业炒作，和各种注册资金过亿、千万+、百万+ 的公司、团队或个人建立服务关系。

希望我们探索出来的路，可以为更多的人提供指引。

## 发行版本

目前，xasset 7.0 主要发布了以下几个版本：

1. **体验版本**
   - MIT授权：https://github.com/xasset/xasset
2. **团队订阅版本**
   - 普通授权：https://github.com/mmdnb/xasset-pro
   - 旗舰授权：https://github.com/mmdnb/xasset-ue
3. **个人订阅版本**
   - 长期用户：https://github.com/mmdnb/xasset-lts
   - 特殊用户：https://github.com/mmdnb/xasset-se
   - 普通用户：https://github.com/mmdnb/xasset-base

阅读[版本比较](https://xasset.github.io/docs/#/compares)可以比较细致的了解体验开源版本和团队订阅版本的差异，而订阅版本中，个人相对团队主要是剥离了以下功能：

- 高性能资源加密功能
- Google Play 分包适配
- XLua 适配

通常，一般项目用开源版本足够了，而对于体量比较大，或是对更新机能、安全性以及性能等方面有更高标准的项目来说，订阅团队版本，不仅可以帮老板省钱，也能帮自己节省时间。

## 新的改进

对比上一个开源版本（4.x），7.0 最大的变化是：

- 编辑器和运行时高度剥离，代码结构更精炼和模块化。
- 使用只读的物理文件数据进行版本管理，版本检测稳定性和效率得到前所未有的提高。 
- 打包后的文件的文件名自带文件内容的版本信息，天生可以避免CDN缓存问题以及一些其他的冲突。
- 全新的多线程文件下载组件，真机环境比之前 UnityWebRequest 版本更稳定。
- 自动分帧机制为程序运行的流畅度提供保障。
- 加载资源默认支持自动更新。

> 注：开源版本中，同步加载如果触发自动更新可能会报错，订阅版本会更强大一些。

对比上一个订阅版本（6.1），7.0 最大的变化是：

- 全新的分布式构建系统，可以更快更灵活地打包。
- 编辑器+运行时的代码量减少了近 3000 行，程序结构更精炼，且更好扩展。
- Android App Bundle + Play Asset Delivery 适配（仅限团队版本）。
- 基于 XLua 的 Lua 文件打包加载适配（仅限团队版本）。
- 高性能安装包资源加密（仅限团队版本）。
- 自动热重载技术。

## 最近更新

### 7.0.5(2021年8月13日)

**新特性**

- 自动热重载，已经加载的 AB 更新后，再次加载时自动卸载旧的，然后再加载新的版本
- Versions.GetDownloadSizeAsync 支持使用 不带 hash 的 bundle 名字，资源路径依旧有效
- 编辑器菜单层级优化，增加查看文档、提交问题等编辑器工具
- 命令行打包工具支持输入版本号

**其他**

- RawAsset 去掉 bytes 属性，建议使用 savePath 自行按需加载数据

### 7.0.4(2021年8月12日)

**新特性**

- 母包资源加密支持（仅限团队版本，含 PlayAssetDelivery 部分）
- 独立 Unity 的源码工程，可以制作 dll（仅限团队版本，参考 Source 文件夹）

**其他**

- 原 AAB 包文件夹和名字空间改成 PAD
- AssetDatabase.FindAssets 调用优化，使用完整名字空间避免同名类型资源出现冲突。

> 阅读[修改记录](https://xasset.github.io/docs/#/changes)可以了解更多历史内容。

## 功能特性

xasset-7.0 体验版提供了以下功能特性：

- 增量打包（Unity构建管线自带机制），输出文件默认以追加hash的方式命名，天生不会有CDN同名缓存问题。
- 全量更新、加载资源自动更新，最稳定可靠的只读模式的版本管理机制。
- 简便的跨平台资源（同步和异步）、场景（异步）加载能力、~~支持异步转同步~~。
- 仿真模式：编辑器有效，只需设置好打包分组、可以跳过打包直接运行。
- 预加载模式：编辑器有效，需要先打包才能运行，直接加载最新的打包数据，不会触发更新。
- 增量模式：需要先打包才能运行，可以在编辑器下模拟和真机一样的版本更新流程。
- 离线模式：真机有效、开启后不会触发更新。

> 注：体验版没有提供分布式打包工具，以及自动分析依赖、自动优化冗余、自动解决冲突的支持。可以结合 Unity 的 [AssetBundleBrowser](https://github.com/Unity-Technologies/AssetBundles-Browser) 可视化的创建资源的 AssetBundle 打包分组。

对比体验版本，xasset-7.0 订阅版本主要的优势在于：

- **分布式增量打包**：对于大体量的项目，可以根据一些规则把资源拆分为多个 Build 模块，然后选择局部内容构建，加快打包效率。
- **[安装包资源加密](https://xasset.github.io/docs/#/binarymode)**：不仅可以防止安装包资源被 AssetStudio 之类的工具轻易提取，并且 Android 真机测试加载资源的耗时有 10% 左右的提升。
- **[安装包资源分包](https://xasset.github.io/docs/#/splitbuild)**：可以预定义多组配置，按需分离安装包的资源，支持空包启动，最小包包体轻松控制到 30 MB。
- **局部资源下载更新功能**：可以根据资源加载路径或分组名字查询和下载更新，支持自动热重载，资源更新后无需重启。
- **提供了多线程下载工具**：支持限速，断点续传，网络异常自修复，文件指纹校验机制。
- **谷歌分包技术适配**：适配了 PlayAssetDelivery 服务，安装包大小可以轻松突破 150MB 的限制。
- **专属对接群**：多位资深行业从业者，提供更迅捷、全面的技术支持服务。
- **XLua 打包加载支持**：基于 XLua 提供了 Lua 文件编辑器和真机环境打包和加载支持，轻松让 Lua 代码具备热更能力。

有关 xasset 的疑问或建议，请加作者微信号 vmakemore 反馈。

## 系统需求

- 引擎版本：Unity2018.4+
- 语言环境：.net 4.x

## 创作成员

**MoMo的奶爸**

- https://github.com/mmdnb
- https://game4d.cn/

**马三小伙儿**

- https://github.com/XINCGer
- https://www.cnblogs.com/msxh/

## 赞助

成为 xasset 项目的赞助商可以在这里添加自己的链接，可以带 LOGO 或名字：

- [Jojohello](https://www.zhihu.com/people/jojohello)（人民币3000元）
- [马三小伙儿](https://github.com/XINCGer)（人民币1000元）

如需赞助可以联系作者微信：vmakemore。

## 声望

**刘家君**

- https://github.com/suixin567
- 反馈有效问题或建议 +2

**李非莬**

- https://github.com/wynnforthework
- 反馈有效问题或建议 +2

**一念永恆**

- https://github.com/putifeng

- 反馈有效问题或建议 +2

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
- [JEngine](https://github.com/JasonXuDeveloper/JEngine) 一个基于XAsset&ILRuntime，精简好用的热更框架（JEngine 目前是 4.x 版本的 xasset 慎用）
