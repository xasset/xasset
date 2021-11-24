# [xasset](https://xasset.github.io)

xasset 是开箱即用的 Unity 分包、加密热更框架。

xasset 专注于解决Unity项目包体大、打包慢、运行卡顿等问题。使用 xasset 可以让你的 Unity 项目具备**快速打包**和**流畅运行**的能力。[了解更多 »](https://xasset.github.io) 

从发布至今，已经持续迭代了 5 年+，并始终在为 化繁为简，节省时间 的目标发力，也收获了：

- 1600+ 星标关注
- 200+ 付费订阅
- 50+ 团队信赖

xasset 的理念是用户的成功才是 xasset 的成功。

观看这个现场视频，可以了解 xasset 团队订阅版示例工程在编辑器运行真机资源热更加载的全过程：

如果视频不能显示出来，可以点击这里前往观看：https://xasset.github.io/video/example.mp4


<video width="100%" height="100%" playsinline autoplay muted loop>
	<source src="https://xasset.github.io/video/example.mp4" type="video/mp4" /> Your browser does not support the video tag.
</video>
场景加载，循环依赖加载，异步转同步，局部更新，自动更新，全程 Profiler，进多少，出多少...视频所演示的这些就是 xasset 的根基。

## 功能特点

这里是最新的 xasset 的开源版本，可以使用以下 3 种开发模式进行快速开发和迭代：

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

## 订阅的优势

对比开源版本，订阅版本主要的优势在于：

- [分布式增量打包支持](https://xasset.github.io/docs/getting-started/buildbundles)：支持自动分组，更快更轻松的解决打包慢和资源冗余的问题。
- [安装包资源加密支持](https://xasset.github.io/docs/advance/binarymode)：不仅可以防止资源被破解，而且 Android 真机测试有 ~10% 的性能提升。
- [安装包资源分包](https://xasset.github.io/docs/getting-started/splitbuild)：使用配置把控 app 安装大小，自动处理依赖，优化包体，减少用户等待。
- **局部资源下载更新功能**：支持自动热重载，资源更新后无需重启。
- **提供了多线程下载工具**：支持限速，断点续传，网络异常自修复，文件指纹校验机制。
- [谷歌分包技术适配](https://xasset.github.io/docs/advance/pad)：适配了 PlayAssetDelivery 服务，安装包大小可以轻松突破 150MB 的限制。
- **专属对接群**：多位资深行业从业者，提供更迅捷、全面的技术支持服务。
- **xLua 打包加载支持**：基于 xLua 的 Lua 文件打包和加载支持，快速让 Lua 代码具备热更能力。
- 资源加载分析工具：统计运行时资源加载的耗时和帧数，方便分析资源制作的合理性。
- 资源依赖分析工具：统计编辑器打包后的资源的大小和依赖关系，方便分析和打包粒度的合理性。

阅读[版本比较](https://xasset.github.io/compares)可以比较细致的了解开源版本和团队订阅版本的差异。了解订阅价格和更多信息，可以前往这里查看：https://xasset.github.io/price

对个人，我们提供免费的开源版，甚至可以在商业项目中使用。

对于公司，我们提供功能更强大的付费团队订阅版本，需要获得授权才能使用。

通过你们的支持, 我们不断为每个人改进 xasset。

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

需要注意的是，该文档主要针对团队订阅用户，开源版可以参考核心接口使用部分。

## 许可

请注意，xasset 具有特殊许可证，并且在某些情况下需要获得公司许可证。阅读 [LICENSE](LICENSE.md) 文档以获取更多信息。

## 创作者

- [MoMo的奶爸](https://github.com/mmdnb)
- [马三小伙儿](https://github.com/XINCGer)

## 赞助

成为 xasset 的赞助商可以在这里添加自己的链接，可以带 LOGO 或名字：

- [Jojohello](https://www.zhihu.com/people/jojohello)（人民币3000元）
- [马三小伙儿](https://github.com/XINCGer)（人民币1000元）

如需赞助可以联系MoMo的奶爸的微信：vmakemore。

## 声望

- [刘家君](https://github.com/suixin567)：反馈有效问题或建议 +2
- [李非莬](https://github.com/wynnforthework)：反馈有效问题或建议 +2
- [一念永恆](https://github.com/putifeng)：反馈有效问题或建议 +2
- [小魔女纱代酱](https://github.com/DumoeDss)：反馈有效问题或建议 +2
- [夜莺](https://github.com/killop)：反馈有效问题或建议 + 5


## 友链

- [ET](https://github.com/egametang/ET) Unity3D Client And C# Server Framework
- [TinaX Framework](https://tinax.corala.space/) “开箱即用”的Unity独立游戏开发工具
- [LuaProfiler-For-Unity](https://github.com/ElPsyCongree/LuaProfiler-For-Unity) Lua Profiler For Unity支持 XLua、SLua、ToLua
