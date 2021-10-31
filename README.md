<p align="center">
  <a href="https://game4d.cn/">
    <img src="https://game4d.cn/images/logo.png" alt="xasset logo" width="200" height="200">
  </a>
</p>

<h3 align="center">xasset</h3>

<p align="center">
  <br>
  专治Unity项目打包慢、包体大、边玩边下和运行卡顿之类的疑难杂症。
  <br>
  <a href="https://xasset.github.io"><strong>浏览文档（仅限订阅版本） »</strong></a>
  <br>
  <br>
  <a href="https://github.com/xasset/xasset/issues/new?template=bug_report.md">报告问题</a>
  ·
  <a href="https://github.com/xasset/xasset/issues/new?template=feature_request.md">提交需求</a> 
</p>






## xasset 7.0

这里是 xasset 7.0 的体验版本。对比上一个开源版本（4.x），7.0 最大的变化是：

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

从发布至今已经持续迭代了近 5 年，我们收获了 1500+ 星标、180+ 订阅用户，并始终在为化繁为简，节省时间的目标发力。

非常感谢新老用户的支持和鼓励！你们的成功，才是我们的成功！

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
- **[安装包资源加密](https://xasset.github.io/guide/binarymode.html)**：不仅可以防止安装包资源被 AssetStudio 之类的工具轻易提取，并且 Android 真机测试加载资源的耗时有 10% 左右的提升。
- **[安装包资源分包](https://xasset.github.io/guide/splitbuild.html)**：可以预定义多组配置，按需分离安装包的资源，支持空包启动，最小包包体轻松控制到 30 MB。
- **局部资源下载更新功能**：可以根据资源加载路径或分组名字查询和下载更新，支持自动热重载，资源更新后无需重启。
- **提供了多线程下载工具**：支持限速，断点续传，网络异常自修复，文件指纹校验机制。
- **谷歌分包技术适配**：适配了 PlayAssetDelivery 服务，安装包大小可以轻松突破 150MB 的限制。
- **专属对接群**：多位资深行业从业者，提供更迅捷、全面的技术支持服务。
- **XLua 打包加载支持**：基于 XLua 提供了 Lua 文件编辑器和真机环境打包和加载支持，轻松让 Lua 代码具备热更能力。

有关 xasset 的疑问或建议，请加作者微信号 vmakemore 反馈。

## 快速开始

要体验 xasset 的体验版本可以：

- 克隆仓库：https://github.com/xasset/xasset.git
- 打包资源：**Assets/Versions/Build Bundles**
- 打包安装包：**Assets/Versions/Build Player**

运行时 API 可以参考：

- https://xasset.github.io/guide/coreapi.html

如何为资源分配 AssetBundle 可以参考：

- https://docs.unity3d.com/cn/current/Manual/AssetBundles-Workflow.html

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
