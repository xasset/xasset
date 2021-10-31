# 打包资源

打包资源的流程主要分为 3 步：

## 1.创建一个 Build 配置：

在 Unity 的 Project 窗口中右键选中以下菜单并执行，可以创建一个 Build 配置：

- Assets/Create/Versions/Build

以下是示例中叫 Arts 的已经创建好的 Build 配置的 Inspector 视图：

![build](/img/build.png)

## 2.为 Build 添加分组

当 Build 创建好后，可以在 Unity 的 Project 窗口中，批量选中资源文件（含目录），拖拽到 Build 对象的 Inspector 视图的 Drag and Drop selection to this box! 内来添加要打包的分组，如下图：

![build-add-group-by-drag](/img/build-add-group-by-drag.png)

## 3.执行打包命令

在 Unity 编辑器中，可以通过执行这个菜单命令一键打包：

- Assets/Versions/Build Bundles

执行打包后，默认会把所有打包的资源输出到项目目录的 Bundles/{PlatformName} 目录下，每一个 Build 都会根据对应的名字生成一份 json 清单文件和对应的版本文件，如下图：

![build-bundles](/img/build-bundles.png)

默认，打包后的文件都是以这种格式命名：

```
文件名_文件内容哈希值
```

这种格式的好处是，文件名自带文件的版本号，天生不会出现 CDN 同名缓存问题。