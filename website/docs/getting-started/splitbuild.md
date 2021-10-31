# 安装包资源分包

分包，是一种把游戏内容按需进行切割的技术。

不论是为了让用户更快的体验程序功能，还是减少运营成本，对项目而言，分包技术将成为不可或缺的技术。

xasset 7.0 的分包技术，主要达成了：

- 最小化安装包大小，可以让 App 不带资源启动，示例的 Apk 不超过 30 MB。
- 自动管理依赖，只需配置加载路径，打包安装包的时候，打包系统会自动按需把相关资源和其依赖的资源复制到 StreamingAssets。

使用 xasset 7.0 进行分包打包的主要流程是：

1. 在 Settings 中创建 PlayerConfigs；

2. 在 打包安装包前修改 Settings 的 BuildPlayerConfigIndex 属性，指定要使用的分包配置；

3. 通过 批处理、文件菜单、Assets 菜单等任意方式，进行安装包打包。

PlayerConfigs 可以预定义多组安装包资源配置，BuildPlayerConfigIndex 决定打包的时候启用的配置。以下是示例中的配置：

![splitbuild](/img/splitbuild.png)

上图中，主要包含了三份预定义的分包配置：

- Preload：表示安装包只包含 Preload 分组的资源
- Completed：表示安装包包含所有内容
- Empty：表示安装包不含内容

注：对于团队用户，xasset 7.0 以及对 Android App Bundle 进行了完美适配，有需要的团队用户可以在对应的技术支持群发起对接需求。