# [xasset](https://xasset.github.io) · ![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)

xasset 是开箱即用的 Unity 分包，加密和热更框架。

使用 xasset 可以加快你的 Unity 项目的迭代速度和减少用户等待时间，并为程序运行时的流畅度和稳定性带来更多保障。**[了解更多 »](https://xasset.github.io)**

这里是最新的 xasset 的 Example 工程的演示，编辑器开启增量模式，体验真机加载和局部热更环境，涵盖了循环依赖加载，异步转同步等各种极端示例。全程 profiler，**进多少、出多少**：


<video width="100%" height="100%" playsinline autoplay muted loop>
	<source src="https://xasset.github.io/video/example.mp4" type="video/mp4" /> Your browser does not support the video tag.
</video>


如果视频不能显示出来，可以点击这里前往观看：https://xasset.github.io/video/example.mp4

从发布至今，xasset 项目已经持续迭代了近 5 年，并始终在为化繁为简，节省时间的目标发力，也收获了：

| 星标关注 | 付费订阅 | 公司资助 |
| -------- | -------- | -------- |
| 1500+    | 200+     | 50+      |

非常感谢新老用户的支持和鼓励！你们的成功，才是我们的成功！

## xasset 7

这里是最新的 xasset 7 的开源版本，可以使用以下 3 种开发模式进行快速开发和迭代：

- **编辑器仿真模式**：只需设置好打包分组、可以跳过打包直接运行。
- **编辑器预加载模式**：在编辑器调试真机的加载环境，需要先打包才能运行，不会触发更新。
- **编辑器增量模式**：在编辑器调试真机一样的热更加载环境，需要先打包并部署服后才能运行，会触发更新。

另外，xasset 还提供了离线模式，可以让程序在真机上一键关闭更新，方便提审需要。

比较简明的是，不论在编辑器、还是真机、不论在安装包内还是更新目录、不论在本地还是服务器，这个版本的 xasset 都是统一使用资源在工程的相对路径加载资源，同时，还提供了：

- **基于只读清单配置的全局资源增量更新技术**：只读意味着不易变，不易变的东西自然最可靠。
- **加载资源自动更新技术**：本地没有的资源自动去服务器下载，下载后自动加载，避免业务中断。
- **自动管理依赖**：不论是加载还是更新，都不用为各种（循环依赖，重复加载或更新等）依赖处理问题烦恼。
- **基于引用计数的内存管理技术**：Profiler 测试具备**进多少，出多少**的稳定性。

开源版本提供了快速对选中资源进行按文件夹或文件进行打包分组的编辑器工具，但没有提供分布式打包工具，以及自动分析依赖、自动优化冗余、自动解决冲突的支持。可以结合 Unity 的 [AssetBundleBrowser](https://github.com/Unity-Technologies/AssetBundles-Browser) 可视化的创建资源的 AssetBundle 打包分组。

对个人，我们提供免费的开源版，甚至可以在商业项目中使用。

对于公司，我们提供功能更强大的付费团队订阅版本，需要获得授权才能使用。通过你们的支持, 我们不断为每个人改进 xasset。

阅读[版本比较](https://xasset.github.io/compares)可以比较细致的了解开源版本和团队订阅版本的差异。

## 订阅的优势

对比开源版本，订阅版本主要的优势在于：

- [分布式增量打包支持](https://xasset.github.io/docs/getting-started/buildbundles)：对于大体量的项目，可以根据一些规则把资源拆分为多个 Build 模块，然后选择局部内容构建，加快打包效率。
- [安装包资源加密支持](https://xasset.github.io/docs/advance/binarymode)：不仅可以防止安装包资源被 AssetStudio 之类的工具轻易提取，并且 Android 真机测试加载资源的耗时有 10% 左右的提升。
- [安装包资源分包](https://xasset.github.io/docs/getting-started/splitbuild)：可以预定义多组配置，按需分离安装包的资源，支持空包启动，最小包包体轻松控制到 30 MB。
- **局部资源下载更新功能**：可以根据资源加载路径或分组名字查询和下载更新，支持自动热重载，资源更新后无需重启。
- **提供了多线程下载工具**：支持限速，断点续传，网络异常自修复，文件指纹校验机制。
- [谷歌分包技术适配](https://xasset.github.io/docs/advance/pad)：适配了 PlayAssetDelivery 服务，安装包大小可以轻松突破 150MB 的限制。
- **专属对接群**：多位资深行业从业者，提供更迅捷、全面的技术支持服务。
- **XLua 打包加载支持**：基于 XLua 提供了 Lua 文件编辑器和真机环境打包和加载支持，轻松让 Lua 代码具备热更能力。

了解订阅价格和更多信息，可以前往这里查看：https://xasset.github.io/price

## 快速开始

### 系统需求

- 引擎版本：Unity 2018.4+
- 语言环境：.net 4.6+

### 操作步骤

1. 可以使用命令行把仓库下载到本地：
	```sh
	git clone https://github.com/xasset/xasset.git
	```

2. 用 Unity 打开下载下来的工程后，执行资源打包的编辑器菜单命令：

   - **Versions/Build Bundles** 打包资源

3. 打开 Startup 场景，点击运行，或者执行后启动 exe：

   - **Versions/Build Player** 打包播放器

### 更多资料

运行时 API 可以参考团队版的文档：

- https://xasset.github.io/docs/getting-started/coreapi

如何为资源分配 AssetBundle 可以参考：

- https://docs.unity3d.com/cn/current/Manual/AssetBundles-Workflow.html

## 文档

前往 https://xasset.github.io/ 可以了解 xasset 的来龙去脉。

## 授权许可

请注意，xasset 具有特殊许可证，并且在某些情况下需要获得公司许可证。阅读 [LICENSE](LICENSE.md) 文档以获取更多信息。

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
