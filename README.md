# 关于

xasset 是专业 Unity 资源系统。 

xasset 使用负载均衡技术让程序更流畅。 xasset 提供开箱即用的打包、分包、加密、边玩边下和跨平台加载等技术让Unity项目的开发更快更轻松！

- 官网：https://xasset.cc
- 文档：https://xasset.cc/docs/getstarted

![example](https://xasset.cc/img/example.gif)

## xasset-2022.2 发布说明

- 文档和特性：https://xasset.cc/docs/next
- 主要变化：https://xasset.cc/docs/next/change-log
- 发布状态：2022年9月1日（已发布）

## xasset-2022.1 开源版

[这里](https://github.com/xasset/xasset)是 xasset-2022.1 的开源版本，开源版本提供了以下功能特性：

- 强大的代码运行模式。编辑器既可以无缝调试真机热更加载过程，也可以跳过打包快速运行。
- 高效可靠的版本管理机制。采用只读的方式管理打包后的资源的版本信息，不进行动态写入稳定性更高，文件名自带版本信息，增量部署快速校验效率更高。
- 统一使用相对路径加载资源或场景，可以自定义别名，预加载或边玩边下，自动更新不在本地的资源。
- 基于引用计数的内存管理技术，并自动管理依赖，未完成的异步加载可以立即同步完成。
- 异步更新，动态计算 CPU 负荷自动调整更新时机，通过分而治之，减少卡顿。

开源版本未提供支持可视化且支持自动优化打包质量的分布式打包工具，但提供了快速对选中资源进行按文件夹或文件进行打包分组的编辑器工具，可以结合 Unity 的 [AssetBundleBrowser](https://github.com/Unity-Technologies/AssetBundles-Browser) 可视化地创建资源的 AssetBundle 打包分组。

> 注：xasset-2022.1 版本需要 Unity 2018.4+ 才能运行。

### 接入流程

请参考以下步骤来接入这个版本的 xasset 到你的 Unity 项目中：

- 第一步：下载仓库中最新的 unitypackage 文件到本地。
- 第二步：把下载的 unitypackage 导入到你的 Unity 项目。**注意：** 在导入新版本的 xasset 前，你应该删除旧版本的 xasset。
- 第三步：在 Unity 编辑器中，选择使用 **xasset>Build Bundles** 打包资源。
- 第四步：打开 Startup 场景，并让 Unity 进入播放模式，体验示例的运行效果。
- 第五步：在 Unity 编辑器中，选择使用 **xasset>Build Player** 可以一键打包安装包。

### 用法说明

运行时 API 可以参考团队版的文档：

- https://xasset.cc/docs/api/versions

如何为资源分配 AssetBundle 可以参考：

- https://docs.unity3d.com/cn/current/Manual/AssetBundles-Workflow.html

## 为什么订阅？

个人或 3 人以下的小团队可以使用免费的开源版。对于公司，需要获得我们的授权许可才能使用，我们的授权有专门的[用户协议](https://xasset.cc/license)，只有接受用户协议的条款才能订阅。通过你们的支持，我们不断为大家改进 xasset。相对免费开源版本，付费订阅的版本还具有以下优势：

- 分布式增量打包：相互独立的资源分批次提交，减少算力浪费，加快打包速度。
- 按需配置自动分组：根据引用关系生成按需加载的最优分组，减少打包冗余，快速优化打包质量。
- 实时预览打包粒度和依赖关系：帮助团队提前发现问题并解决问题。
- 万能打包模式：所有格式的资源全部能够打包，并参与版本管理。
- [高效资源加密](https://xasset.cc/docs/encryption)：不仅可以防止资源被轻易破解，而且几乎不损耗程序运行的性能（2022.1版本WebGL不能加密，2022.2版本WebGL也能加密）。
- [安装包资源分包](https://xasset.cc/docs/splitbuild)：使用配置把控 app 安装大小，自动处理依赖关系并剥离包体资源，并且适配了谷歌分包技术，可以节省大量业务对接时间。
- 按需加载：整体或局部按需更新，边玩边下自动热重载。
- 多线程下载工具：支持限速，断点续传，网络异常自修复，文件指纹校验机制。
- 丰富的工具链：提供了版本管理、打包管理、清单管理、加载管理等工具，可以有效帮助团队提前发现问题和解决问题。
- 专属对接群：多位资深行业从业者，提供更迅捷、全面的技术支持服务。 

了解订阅价格和更多信息，可以前往 [xasset.cc](https://xasset.cc/price) 查看。

**注：xasset 订阅版本默认都是一次付费，长期使用，1年免费更新支持。且我们只为注重时间价值，尊重知识产权的团队提供服务。**

## 文档

前往 https://xasset.cc/ 可以了解 xasset 的来龙去脉。

需要注意的是，该文档主要针对团队订阅用户，开源版可以参考核心接口使用部分。

## 许可

请注意，xasset 具有特殊许可证，并且在某些情况下需要获得公司许可证。阅读 [LICENSE](LICENSE.md) 文档以获取更多信息。

## 创作者

- [MoMo的奶爸](https://github.com/mmdnb)
- [马三小伙儿](https://github.com/XINCGer)

## 赞助

成为 xasset 的赞助商可以在这里添加自己的链接，可以带 LOGO 或名字：

- [马三小伙儿](https://github.com/XINCGer)（人民币4800元）
- 花花 （人民币3848元）
- [Jojohello](https://www.zhihu.com/people/jojohello)（人民币3000元）

如需赞助可以联系MoMo的奶爸的微信：vmakemore。

## 声望

- [Connor Aaron Roberts](https://github.com/c0nd3v):反馈有效问题或建议 +10
- [刘家君](https://github.com/suixin567)：反馈有效问题或建议 +2
- [李非莬](https://github.com/wynnforthework)：反馈有效问题或建议 +2
- [一念永恆](https://github.com/putifeng)：反馈有效问题或建议 +2
- [小魔女纱代酱](https://github.com/DumoeDss)：反馈有效问题或建议 +2
- [夜莺](https://github.com/killop)：反馈有效问题或建议 + 5


## 友链

- [ET](https://github.com/egametang/ET) Unity3D Client And C# Server Framework
- [TinaX Framework](https://tinax.corala.space/) “开箱即用”的Unity独立游戏开发工具
- [LuaProfiler-For-Unity](https://github.com/ElPsyCongree/LuaProfiler-For-Unity) Lua Profiler For Unity支持 XLua、SLua、ToLua
