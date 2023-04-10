# 关于

xasset 是跨平台 Unity 资源系统。

xasset 提供开箱即用的分包、加密和边玩边下等技术解决方案，可以实现更快的开发效率和更流畅的用户体验。

- 官网：https://xasset.cc
- GitHub：https://github.com/xasset/xasset
- UOS CDN：https://uos.unity.cn/partner/xasset

了解更多 xasset 的产品特性，可以前往 xasset 的[官网](https://xasset.cc) 查看。

另外，Unity 中国官方为 xasset 用户提供的专属的开发阶段免费的 CDN 服务，了解相关服务的使用，可以打开 [UOS CDN](https://uos.unity.cn/partner/xasset) 的链接查看。

> 提示：使用 UOS CDN 的时候，可以把 Bundles 和 updateinfo.json 放到不同的存储桶。Bundles 下的数据无需使用 Badge 来处理内容分发，updateinfo.json 可以用 Badge 来处理开发测试或线上正式环境的内容分发。

## 最近更新

### 2023.03.27

#### 公众号

- [近10倍IO性能优化的过程和原理](https://mp.weixin.qq.com/s/X0Tc6-UKVqfEXrzSEY17Zw)

### 2023.02.26

#### 开源版（2023）

- 增加强更示例，获取更新大小增加读条过程

#### 专业版（2023）

- 增加强更示例，获取更新大小增加读条过程
- 适配 UOS CDN
- RawAsset 适配二次打包数据读取逻辑

注：更多修改细节，可以阅读源码深入了解，任何疑问或建议，欢迎提交 Issues 反馈。

## 开源版

[这里](https://github.com/xasset/xasset)是 2023 版本的开源版，这个版本主要提供了以下功能特性：

- 分布式打包：配置驱动，支持自动优化打包质量，支持为分组的资源设置寻址模式。
- 跨平台加载：Unity资源和场景加载的业务代码可以一次编码，多处运行，支持Android、iOS、PC、OSX等平台。
- 自动切片：可以更优雅的对单帧堆积较密集业务进行自动切片处理，让程序更平滑的运行。
- 文件下载：支持断点续传和并发下载数量控制的下载模块，每个下载请求都能单独统计下载进度和速度，提供暂停/恢复和取消机制。
- 自动回收：提供先进先出队列缓存，可以更轻松的管理资源内存。
- 丰富的例子：提供带有场景管理、资源加载、文件下载的示例，涵盖了循环依赖加载、子资源加载、异步转同步、叠加场景管理等功能相关的演示代码。

> 请注意，个人或 3 人以下的小团队可以使用免费的开源版。对于公司，需要获得我们的授权许可才能使用，我们的授权有专门的[用户协议](https://xasset.cc/license)，只有接受用户协议的条款才能订阅。通过你们的支持，我们不断为大家改进 xasset。

## 用法

### 一、安装

可以直接把 Assets 目录下的 xasset 文件夹复制到 Unity 工程使用。

### 二、打包

在 Unity 编辑器中，选择 xasset>Build Bundles 可以针对已经创建好的 Build 配置中的资源进行打包。

- 没有选中 Build 配置的时候，会针对所有 Build 配置相关的资源进行打包。
- 选中 Build 配置的时候，只会针对选中（支持多选）的 Build 配置相关的资源进行打包。

创建 Build 配置的过程大致可以参考：[打包资源](https://xasset.cc/docs/examples#%E6%89%93%E5%8C%85%E8%B5%84%E6%BA%90) 的说明。

### 三、加载

加载资源前需要先对 xasset 进行初始化，这里是初始化 xasset 的示例代码。

```csharp
var initializeAsync = Assets.InitializeAsync();
yield return initializeAsync;
```

初始化后，可以使用 Asset.Load(Async) 来加载 Unity 中的资源。

```csharp
// 加载 prefab
var path = "Assets/Prefabs/Cube.prefab"; 
var request = Asset.LoadAsync(path, typeof(GameObject));
yield return request;
var prefab = request.asset;
var go = Object.Instantiate(prefab);
// 回收 prefab，在回收前，需要先把 实例化的 go 销毁。
// Object.DestroyImmediate(go);
request.Release();
```

另外，还可以使用 Scene.Load(Async) 来加载 Unity 中的场景。具体用法可以参考：[加载 Unity 中的场景](https://xasset.cc/docs/examples#加载-unity-中的场景) 的说明。

### 四、其他

#### 输出目录

输出目录是打包后输出文件的目录。

- Bundles 目录为运行时使用的数据目录，可以直接放到 CDN。
- Bundles Cache 目录为编辑器数据缓存目录，主要用来给引擎增量打包使用。
- Streaming Assets > Bundles 安装包数据目录。打包安装包的时候 xasset 会自动根据分包配置将安装包内的资源复制到此目录。

> 注：Bundles 和 Bundles Cache 目录没事都不要删除。

#### 仿真模式

在 Unity 编辑器中，选择使用 xasset>Simulation Mode 可以开启/关闭「仿真模式」。

- 开启后，可以跳过打包运行。
- 未开启，需要先打包资源才能运行。

#### 离线模式

Settings 配置的 Offline Mode 选项开启后，不论是编辑器还是运行时都会禁用更新。

#### 仿真下载

Settings 配置的 Simulation Download 开启后，编辑器下在打包后，关闭「仿真模式」 和「离线模式」可以不用把资源部署到 CDN 体验真机资源更新加载过程。

更多说明后续会补充在 [xasset.cc](https://xasset.cc/) 中。

## 订阅版

相对免费开源版本，付费订阅的版本还具有以下优势：

### 专业版

专业版主打的特性是分包、加密和边玩边下，比较适合需要让产品快速打包、快速发布、快速运行的团队：

- 更完善的打包工具链，可以实时预览资源的打包粒度和依赖关系，提前发现问题提前处理。
- 运行时资源加载记录工具，可以实时统计资源的加载信息，会包含加载资源的耗时/帧数、加载场景、卸载场景、IO次数等信息，并支持数据导出，可以导出到分包配置或者csv文件中使用。
- 原始格式打包和更新加载支持：可以让非 Unity 内建格式的资源轻松进行版本管理。
- [高效资源加密](https://xasset.cc/docs/encryption)：不仅可以防止资源被轻易破解，而且几乎不损耗程序运行的性能。
- [安装包资源分包](https://xasset.cc/docs/splitbuild)：使用配置灵活把控 APP的安装大小，自动处理依赖关系并剥离包体资源。
- 谷歌分包插件支持：适配了 PlayAssetDelivery 插件，可以快速构建符合 GooglePlay 上架标准的 Android App Bundle。
- 优化过的边玩边下机制：可以按需指定需要更新下载的内容，通过局部更新，让用户更快体验游戏。同时还提供了完善的编辑器工具，可以对运行时加载数据进行采用，快速导出生成资源包数据，另外提供散文件合并支持，可以自动根据配置生成大小更均匀的自定义格式资源包，并在运行时自动选择是更新资源包还是更新散文件，IO效率可以得到客观的提升。（注：生成资源包需要额外的CDN空间，但是否生成资源包的是可选的，资源包的生成可以减少网络IO次数同时有加密功效，可以按需使用）
- 专属对接群：提供更私密的技术对接支持，工程师会在对接群远程照顾项目一年。

### 顾问版

顾问版包含专业版的所有功能，同时提供更周到的服务，进行更全面的保驾护航：

- 微信小程序适配支持：帮助用户的项目团队更快、更轻松的完成微信小程序的适配工作。
- 项目内存和渲染瓶颈分析排查和优化，协助团队建立高品质内容生产管线（流程-文档-工具链），走出先污染后治理的研发流程。
- 工程师会照顾项目开发全周期，并提供7天驻场服务，可以提供技术培训或者极端问题排查服务，可以提供各种 SDK 接入支持。

了解订阅的价格和更多信息，可以前往 [xasset.cc/price](https://xasset.cc/price) 查看。

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
- mingjava(人民币5000元)
- [马三小伙儿](https://github.com/XINCGer)（人民币4800元）
- 花花 （人民币3848元）
- [Jojohello](https://www.zhihu.com/people/jojohello)（人民币3000元）

如需赞助可以发邮件咨询：xasset@qq.com。

## 声望

- [Connor Aaron Roberts](https://github.com/c0nd3v):反馈有效问题或建议 +10
- [xxgamecom](xxgamecom):反馈有效问题或建议 +10
- [roki007](https://github.com/roki007):反馈有效问题或建议 +5
- [EasierLu](https://github.com/EasierLu):反馈有效问题或建议 +5
- [Contra](): 反馈有效问题或建议 +5
- [郑昊](https://github.com/plussign): 反馈有效问题或建议 +5
- [MR.汪](https://github.com/youyouhx):反馈有效问题或建议 +5
- [刘家君](https://github.com/suixin567)：反馈有效问题或建议 +2
- [李非莬](https://github.com/wynnforthework)：反馈有效问题或建议 +2
- [一念永恆](https://github.com/putifeng)：反馈有效问题或建议 +2
- [小魔女纱代酱](https://github.com/DumoeDss)：反馈有效问题或建议 +2
- [夜莺](https://github.com/killop)：反馈有效问题或建议 + 5
- [MrJLY](https://github.com/MrJLY)：反馈有效问题或建议 +5
- [Leo](https://github.com/liyanlong0885)：反馈有效问题或建议 +5
- [jakeyluo](https://github.com/jakeyluo)：反馈有效问题或建议 +5


## 友链

- [ET](https://github.com/egametang/ET) Unity3D Client And C# Server Framework
- [TinaX Framework](https://tinax.corala.space/) “开箱即用”的Unity独立游戏开发工具
- [LuaProfiler-For-Unity](https://github.com/ElPsyCongree/LuaProfiler-For-Unity) Lua Profiler For Unity支持 XLua、SLua、ToLua
