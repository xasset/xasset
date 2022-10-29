# 关于

xasset 是专业 Unity 资源系统。

xasset 提供了开箱即用的打包、分包、加密、边玩边下和负载均衡机制。借助 xasset 可以让 Unity 资源管理更轻松！

- 官网：https://xasset.cc
- 文档：https://xasset.cc/docs/next/getting-started

进一步了解 xasset的更多特性，可以前往 xasset 的[官网](https://xasset.cc)查看。

![example](https://xasset.cc/img/example.gif)

## xasset-2022.2开源版

[这里](https://github.com/xasset/xasset)是xasset-2022.2的开源版本，和过去的版本相比，这个开源版本主要提供了以下功能特性：

- 基于配置驱动的分布式打包工具，支持自动分组机制，可以快速优化打包质量。
- 新的下载模块，基于UnityWebRequest实现，支持断点续传和并发下载数量控制，提供了单文件和多文件批量下载的请求，支持本地仿真模式，编辑器下可以跳过资源部署到CDN的流程直接模拟服务器下载资源的过程，每个下载请求都能单独统计下载进度和速度，提供暂停/恢复和取消机制。
- 自动回收机制，提供把资源的生命周期和Unity对象的生命周期绑定的内存管理机制，当Unity对象销毁时，自动释放和其绑定的资源对象，另外提供先进先出队列缓存，可以更轻松的管理资源内存。
- 全新的负载均衡架构，异步实例化、异步渐进式回收，大部分常规业务，可以在底层自动进行切片处理，无需在业务开发的时候判断是否有足够的时间片，业务编码实现的时候会更优雅。

更多细节需要看代码才能感受到，还是和以前一样，个人或 3 人以下的小团队可以使用免费的开源版。对于公司，需要获得我们的授权许可才能使用，我们的授权有专门的[用户协议](https://xasset.cc/license)，只有接受用户协议的条款才能订阅。通过你们的支持，我们不断为大家改进 xasset。

## 订阅的优势

相对免费开源版本，付费订阅的版本还具有以下优势：

- 更完善的打包工具链，可以实时预览资源的打包粒度和依赖关系，提前发现问题提前处理。
- 原始格式打包和更新加载支持：可以让非 Unity 内建格式的资源轻松进行版本管理。
- [高效资源加密](https://xasset.cc/docs/encryption)：不仅可以防止资源被轻易破解，而且几乎不损耗程序运行的性能。
- [安装包资源分包](https://xasset.cc/docs/splitbuild)：使用配置灵活把控 app 安装大小，自动处理依赖关系并剥离包体资源，并且适配了谷歌分包技术，可以轻松快速的让项目满足GooglePlay的上架需要。
- 优化过的边玩边下机制：可以按需指定需要更新下载的内容，通过局部更新，让用户更快体验游戏，同时还提供了完善的编辑器工具，可以对运行时加载数据进行采用，快速导出生成资源包数据，另外提供散文件合并支持，可以自动根据配置生成大小更均匀的自定义格式资源包，并在运行时自动选择是更新资源包还是更新散文件，IO效率可以得到客观的提升。（注：生成资源包需要额外的CDN空间，但是否生成资源包的是可选的，资源包的生成可以减少网络IO次数同时有加密功效，可以按需使用）
- 丰富的工具链：提供了版本管理、打包管理、清单管理、加载管理等工具，可以有效帮助团队提前发现问题和解决问题。
- 专属对接群：付费用户至少提供2位资深工程师远程对接项目，团队版照顾项目团队1年，顾问版照顾项目开发全周期。
- 微信小程序适配：顾问版可以派工程师提供微信小程序适配工作。
- 7天驻场服务：顾问版可以派工程师对项目团队开发过程中遇到的极端问题进行排查优化，可以协助团队优化项目内存和渲染问题。

了解订阅价格和更多信息，可以前往 [xasset.cc/price](https://xasset.cc/price) 查看。

**注：xasset 订阅版本默认都是一次付费，长期使用，1年免费更新支持。且我们只为注重时间价值，尊重知识产权的团队提供服务。**

## 文档

前往 [xasset.cc](https://xasset.cc) 可以了解 xasset 的来龙去脉。

需要注意的是，该文档主要针对团队订阅用户，开源版可以参考核心接口使用部分。

## 许可

请注意，xasset 具有特殊许可证，并且在某些情况下需要获得公司许可证。阅读 [LICENSE](LICENSE.md) 文档以获取更多信息。

## 创作者

- [吉缘](https://github.com/mmdnb)
- [马三小伙儿](https://github.com/XINCGer)
- [寒晟](https://github.com/huangchaoqun)

前往[xasset.cc/about](https://xasset.cc/about)可以了解更多关于我们团队的介绍。 

## 赞助

成为 xasset 的赞助商可以在这里添加自己的链接，可以带 LOGO 或名字：

- Miss_Lynn(人民币50000元)
- [马三小伙儿](https://github.com/XINCGer)（人民币4800元）
- 花花 （人民币3848元）
- [Jojohello](https://www.zhihu.com/people/jojohello)（人民币3000元）

如需赞助可以发邮件给吉缘：xasset@qq.com。

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
