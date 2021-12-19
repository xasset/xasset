# 关于

xasset 是最先进的 Unity 资源热更框架。xasset 提供了开箱即用的分布式打包、分包、加密、按需加载和异步更新技术。各种注资百万+、千万+、甚至过亿的公司的团队都在使用 xasset 解决 Unity 项目的打包慢、包体大、运行卡顿等疑难杂症。

- 官网：https://xasset.pro
- 指南：https://xasset.pro/docs/getstarted
- 示例：https:/xasset.pro/img/example.gif

如果你喜欢 xasset, 请在 GitHub 给一个星标支持下！

## 最新变化

xasset 2022 团队版已经发布。

- 全新工具链：帮助团队提前发现问题和解决问题。
- 全面的文档：从是什么？为什么？如何做？出发，帮助团队少走弯路。
- 强大的特性：分布式打包、分包、加密、按需加载、热重载、异步更新、万能打包模式、自动分组、整体更新、离散更新、增量部署、跳过打包快速运行、一键切换真机热更加载环境、多线程并发下载、断点续传、异常恢复、快速校验、最高效可靠的版本管理机制统统都有。

了解更多，请参考：https://www.xasset.pro/docs/changes

## 开源版本

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

1-3 个人的小团队可以免费使用开源版，甚至可以商用。对于公司，需要获得授权才能使用 xasset，通过你们的支持，我们不断为每个人改进 xasset。同时，付费的订阅版本，也提供了更强大的技术支持：

- 分布式增量打包：相互独立的资源分批次提交，减少算力浪费，加快打包速度。
- 按需配置自动分组：根据引用关系生成按需加载的最优分组，减少打包冗余，快速优化打包质量。
- 实时预览打包粒度和依赖关系：帮助团队提前发现问题并解决问题。
- 万能打包模式：所有格式的资源全部能够打包，并参与版本管理。
- [安装包资源加密支持](https://www.xasset.pro/docs/binarymode)：不仅可以防止资源被破解，而且部分设备真机测试有 ~10% 的性能提升。
- [安装包资源分包](https://www.xasset.pro/docs/splitbuild)：使用配置把控 app 安装大小，自动处理依赖关系并剥离包体资源，并且适配了谷歌分包技术，可以节省大量业务对接时间。
- 按需加载：整体或局部按需更新，边玩边下自动热重载。
- 多线程下载工具：支持限速，断点续传，网络异常自修复，文件指纹校验机制。
- 丰富的工具链：提供了版本管理、打包管理、清单管理、加载管理等工具，可以有效帮助团队提前发现问题和解决问题。
- 全面的文档：从是什么？为什么？如何做？出发，帮助团队少走弯路。
- 专属对接群：多位资深行业从业者，提供更迅捷、全面的技术支持服务。 

阅读[版本比较](https://www.xasset.pro/compares)可以比较细致的了解开源版本和团队订阅版本的差异。了解订阅价格和更多信息，可以前往这里查看：https://www.xasset.pro/price

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

- https://www.xasset.pro/docs/api/versions

如何为资源分配 AssetBundle 可以参考：

- https://docs.unity3d.com/cn/current/Manual/AssetBundles-Workflow.html

## 文档

前往 https://www.xasset.pro/ 可以了解 xasset 的来龙去脉。

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
