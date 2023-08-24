# 专业 Unity 资产系统

xasset 提供开箱即用的资产打包、分包、加密、热更和加载等技术方案。让 Unity 项目开发更轻松！

- 社区版：https://github.com/xasset/xasset

个人或 1-3 个人的小团队，可以免费使用社区版。

对于公司，需要[购买授权](https://xasset.cc/price)才能使用 xasset，并且购买授权后，可以获得功能更强大的专业版。

专业版和社区版本的主要区别是：

- 实用的打包工具和资产加载记录工具。
- 分组资产二次加密打包（可以优化网络IO次数）。
- 安装包资产二次加密打包（Android平台IO性能提升10%，特定场景谷歌商店包IO性能提升300%+）。
- 安装包资产分包和谷歌商店分包工具适配（可以灵活交付产品的安装大小，并轻松上架谷歌商店）。
- 专属对接群，10年以上手游开发经验的资深工程师照顾项目1年。
- 可选的顾问版升级服务，可以让资深工程师照顾项目开发全周期，为产品的性能稳定性提供更全面的保驾护航服务。

了解更多 xasset 的产品特性，请前往 [xasset.cc](https://xasset.cc) 查看。

## 最新动态

- 2023.08.14：[从运行模式的新变化看我们的荣誉和责任](https://mp.weixin.qq.com/s?__biz=MzU2Nzk0Njk4MQ==&mid=2247483849&idx=1&sn=86f07f213209de25ba5020830d0e1464&chksm=fc943a43cbe3b355369ef71c0ee482d2b95292ba5a6d7b0c091cb3ee694b630083d37854be3a&scene=0&xtrack=1&key=74c04ea2637745e8a08b219f630c6e07374d3c24c85808a9e3dd9e1b406c548f5f943febd8178535e50d2f53e8acc3fb2ef5db2a6fd79acd23174df1a585158dbceea365f5d4543c6cad16c19b920a18c5559b85a32a02b88fec7da0983c50b531164c165fde3c3b245ccc6124b9b678904d13c6908217b05159d535ee7a08bb&ascene=15&uin=NjE2NDc4NTAx&devicetype=iMac+MacBookPro14%2C1+OSX+OSX+13.4.1+build(22F82)&version=13080210&nettype=WIFI&lang=en&session_us=gh_b22cefc10b2d&countrycode=CN&fontScale=100&exportkey=n_ChQIAhIQvCdnVZp5aV7pC8%2BIgT8KHxKLAgIE97dBBAEAAAAAACl8JO%2Bf%2FOoAAAAOpnltbLcz9gKNyK89dVj0SHHBBxudWKfHSsoy2MYTWlfd8A7PslWGSDDO98y24W1i6ek61qcQ2xm3e919trTSf8H4QbICKBLwrQ1cfCecDWeuVwx7THgBzn6jfpfeWdPRiXJ1boHTxDT91iSBaErQ9r6hyotXCUDjGNFukY1Iyapis8L8yXChjtcK2L7LW0B1uas%2BgAzMlqN5UMwsHkKplhls5t%2BCTQN6SHhErj84tnn6L4xCj2dDtVH4YPV%2B1tUhGL5lbp2%2BtkzpzO%2FiYWf0DB7oEvvf75wB2vTk90%2BK8AjooJcbrpfek1MXtKTzpEO0KRq0vQ%3D%3D&acctmode=0&pass_ticket=KmIEtK%2By%2BwMpM1pOlrA2mthn3cTX87%2BWIJJKS2ACjjXroS60ot%2FZHWm6VQII7GSS&wx_header=0)
- 2023.04.24：[xasset 2023.1预览版发布，除了资产热重载外还有哪些改变？](https://mp.weixin.qq.com/s/H2HDtwnp1mG_F4v1TahVJg)
- 2023.03.27：[近10倍IO性能优化的过程和原理](https://mp.weixin.qq.com/s/X0Tc6-UKVqfEXrzSEY17Zw)

## 推荐服务：使用 UOS CDN 进行资源云端管理

由 xasset 团队和 Unity
团队联手打造的快速资源分发流程和工具，开发阶段可以免费使用。[>>> 前往体验](https://uos.unity.cn/partner/xasset)

<img src="https://uos.unity.cn/images/homepage/xasset-hp.png" alt="xasset-uos" style="zoom:50%;" />

> 提示：使用 UOS CDN 的时候，可以把 Bundles 和 updateinfo.json 放到不同的存储桶。Bundles 下的数据无需使用 Badge
> 来处理内容分发，updateinfo.json 可以用 Badge 来处理开发测试或线上正式环境的内容分发。

## xasset 的设计理念

xasset 的设计理念可以概括为以下几点。

- 简单至上。保持纯粹，不做俄罗斯套娃，让普通人都能一目了然。
- 物尽其用。尽可能挖掘每个对象存在的最大价值，用更少地投入获得更多地产出。
- 精益求精。不止于过去，追求极致并通过不断打磨和提炼来完成自我进化。

我们相信，了解产品的设计理念，有助于更好地理解和掌握产品的使用，期待可以获得更多的共鸣。

## 用法

### 打包资源

1、使用 `Assets>xasset>Create>Build` 创建打包配置。

2、根据游戏的生命周期节点在 Build 配置中添加 Build Group。

3、使用 `xasset>Generate Group Assets Menu Items` 为创建的 Build 配置中的 Groups 生成 Assets 菜单。

4、使用 3 生成的菜单 `Assets>Group To`，在 Unity 的 Project 中选中资产文件或文件夹添加到对应的 Group 中。

5、使用 `xasset>Build Bundles` 对 Build 配置中包含的资产进行打包。

### 运行示例

对于社区版用户，xasset 提供了包含如下场景功能的示例：

- Startup 初始化场景，不包含任何资产。
- Splash 闪屏界面。
- Opening Dialog 开场对话场景，增加一些仪式感。
- CheckForUpdate 检查更新的场景，提供了资产版本更新检查，网络下载异常处理和资产热重载等功能的示例。
- Menu 菜单场景，提供了局部场景内容更新等功能的演示。
- LoadAsset 加载资产场景，提供了同步/异步资产加载，异步实例化，循环依赖加载，子资产加载，资产释放等功能的演示。
- LoadAdditiveScene 加载附加场景，提供了附加场景加载、激活、卸载等功能的演示。
- Download 下载场景，提供了单文件下载、暂停下载、恢复下载等功能的演示。

对于专业版用户，xasset 在社区版的基础上增加了以下功能的示例：

- LoadRawAsset 加载原始资产场景，提供了使用 xasset 加载打包为原始二进制格式文件的功能演示。

和社区版不同，专业版可以优化细碎的散文件的IO次数，在更新资产的时候，底层会自动对资产数据进行检查，如果资产所在的分组中有启用二次加密打包，并且该分组的资产没有下载过，那么会优先下载二次加密打包后的资产组文件，反之，如果该分组的资产有下载过，那么只会下载细碎的散文件。

xasset 提供了以下几种运行示例的方式：

#### 一、不打包运行

1、勾选 `xasset>Play Mode>Fast Play Without Build`。

2、打开 Startup 场景让 Unity 进入播放模式。

#### 二、打包运行不更新

1、使用 `xasset>Build Bundles` 打包示例的资产(如果已经打包过可以跳过)

2、勾选 `xasset>Play Mode>Play Without Update`。

3、打开 Startup 场景让 Unity 进入播放模式。

#### 三、打包运行并开启更新使用仿真下载

1、使用 `xasset>Build Bundles` 打包示例的资产。(如果已经打包过可以跳过)

2、勾选 `xasset>Play Mode>Play With Update By Simulation`。

3、使用  `xasset>Build Player Assets` 构建安装包资产。选中一个 versions.json。 (如果已经打包过可以跳过)

4、打开 Startup 场景让 Unity 进入播放模式。

#### 四、打包运行并开启更新使用真机模式（从CDN下载打包后的资产）

1、使用 `xasset>Build Bundles` 打包示例的资产。(如果已经打包过可以跳过)

2、勾选 `xasset>Play Mode>Play With Update By Realtime`。

3、在 Settings 中配置好 CDN 的地址，并把打包后的资产部署到 CDN。

4、使用  `xasset>Build Player Assets` 构建安装包资产。选中一个 versions.json。 (如果已经打包过可以跳过)

5、打开 Startup 场景让 Unity 进入播放模式。

## 版本

阅读 [技术规格](https://xasset.cc/price#技术规格) 可以了解不同版本之间的差异。

## 文档

前往 [xasset.cc](https://xasset.cc) 可以了解 xasset 的来龙去脉。

- 2022.1：https://xasset.cc/docs/2022.1/about
- 2022.2：https://xasset.cc/docs/getting-started
- 2023.1：待完成

## 许可

请注意，xasset 具有特殊许可证，并且在某些情况下需要获得公司许可证。阅读 [许可](LICENSE.md) 文档以获取更多信息。

## 创作者

xasset 的主要创作者是：

- [吉缘](https://github.com/mmdnb)
- [马三小伙儿](https://github.com/XINCGer)
- [寒晟](https://github.com/huangchaoqun)

另外，xasset 的广大用户也提供了不少有价值的参考，感谢大家的反馈和建议！我们会继续努力，为大家提供更好的产品和服务。

## 赞助商

成为 xasset 的赞助商可以在这里添加自己的链接，可以带图标或名字：

- Miss_Lynn(人民币50000元)
- mingjava(人民币5000元)
- [马三小伙儿](https://github.com/XINCGer)（人民币4800元）
- 花花 （人民币3848元）
- [Jojohello](https://www.zhihu.com/people/jojohello)（人民币3000元）

赞助商提供的资金将用来扶持更多有创造力的创作，期待未来有更多有潜力的产品的出现。

## 贡献者

为 xasset 提供好的建议或反馈，可以成为 xasset 的贡献者：

- 袁晟铭：+10 点声望

- [EasierLu](https://github.com/EasierLu) +10 点声望
- [Connor Aaron Roberts](https://github.com/c0nd3v) +10 点声望
- [xxgamecom](xxgamecom) +10 点声望
- [roki007](https://github.com/roki007) +5 声望
- [Contra]() +5 点声望
- [郑昊](https://github.com/plussign) +5 点声望
- [MR.汪](https://github.com/youyouhx) +5 点声望
- [刘家君](https://github.com/suixin567) +2 点声望
- [李非莬](https://github.com/wynnforthework)  +2 点声望
- [一念永恆](https://github.com/putifeng) +2 点声望
- [小魔女纱代酱](https://github.com/DumoeDss)  +2 点声望
- [夜莺](https://github.com/killop) +5 点声望
- [MrJLY](https://github.com/MrJLY) +5 点声望
- [Leo](https://github.com/liyanlong0885) +5 点声望
- [jakeyluo](https://github.com/jakeyluo) +5 点声望

成为贡献者可以获得一定的声望，声望可以折现，声望和人民币的兑换比例是 1：20，也就是 1 点声望可以兑换 20 元人民币。

## 推荐框架

- [ET](https://github.com/egametang/ET) Unity3D Client And C# Server Framework
- [TinaX Framework](https://tinax.corala.space/) 开箱即用的Unity独立游戏开发工具
- [LuaProfiler-For-Unity](https://github.com/ElPsyCongree/LuaProfiler-For-Unity) Lua Profiler For Unity支持
  XLua、SLua、ToLua
