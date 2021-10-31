---
id: quickstart
title: 快速使用
sidebar_position: 1
---

## 安装方式

在 Unity 项目中集成 xasset 的方式主要有这 3 种：

1. 导入包含源码的 unitypackage 文件。
2. 导入预编译的 dll 到项目的 Assets 目录下。
3. 把 github 仓库的 Source 文件夹复制到项目的根目录下，与 Assets 目录同级，然后打编译下源码工程，就会自动把 dll 输出到 Assets/Versions 目录下。

PS：不论是直接使用源码集成到 Unity，还是使用 dll 的方式都可以使用 Rider 调试。

## 项目结构

```sh
Project // Unity 项目的根目录
├── Assets
│   ├── Versions
│   └── Versions.Example
├── Source // 源码工程
│   ├── Dependencies // 源码工程依赖的 dll
│   ├── Versions.Editor // 编辑器源码工程
│   ├── Versions.Runtime // 运行时源码工程
│   └── Source.sln
└── Bundles // 资源打包后的输出目录。
```

## 系统需求

- 引擎版本：Unity 2018.4+
- 脚本运行版本: .NET 3.5
- 支持平台：Windows、OSX、Android、iOS、WebGL

## 技术支持

有任何问题或建议，可以创建 issues 或者在专属的对接群留言。