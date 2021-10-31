# 打包安装包

打包安装包主要用来发布目标平台的可执行程序，例如 apk，exe，ipa 等。

在 Unity 编辑器中，除了可以使用 Unity 编辑器自带的 File/BuildXXX 菜单来打包安装包外， 也还可以通过执行这个菜单命令一键打包安装包：

- Assets/Versions/Build Player

不论以何种方式打包安装包，默认，xasset 会通过打包预处理和后处理事件，自动根据 Settings 的配置把相应资源复制到 StreamingAssets 下，并且在打包完成后自动删除。

为什么要删除复制到StreamingAssets的资源呢？

这和 Unity 的一个反人类的设计有关：复制到 StreamingAssets 的资源，正常来说应该不需要导入，但是如果不删除，有可能会触发漫长的 Reimport，删除后才能让编辑器更快的切到正常的工作状态。