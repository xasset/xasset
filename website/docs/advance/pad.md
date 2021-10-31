# 谷歌 PAD 集成

PAD 是 PlayAssetDelivery 的简称，对于团队用户，xasset 7.0 提供了谷歌 PAD 的集成，可以帮助团队用户让程序更快更轻松的达成 Google Play 商店的上线标准。

xasset 7.0 对 PAD 的集成主要是提供了以下支持：

- 把安装包的资源生成 PAD 的 AssetPack，分发模式为 install-time，安装包大小可以突破 150 MB 限制
- 同步或异步加载 AssetPack 中包含的 Bundle，支持加密模式
- 补丁可以依旧使用 xasset 的补丁更新机制

在项目中集成 PAD 的流程是：

1. 下载 GooglePlay 资源分发插件：
    - [com.google.play.assetdelivery-1.5.0.unitypackage](https://github-releases.githubusercontent.com/248128393/d78d1f00-d8f1-11eb-91c3-90d60ff1d245?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIWNJYAX4CSVEH53A%2F20210811%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20210811T025949Z&X-Amz-Expires=300&X-Amz-Signature=7a0dd4941300e2df311cc163a0b4811a321312c4149d8679025e13bf51998107&X-Amz-SignedHeaders=host&actor_id=25072236&key_id=0&repo_id=248128393&response-content-disposition=attachment%3B%20filename%3Dcom.google.play.assetdelivery-1.5.0.unitypackage&response-content-type=application%2Foctet-stream)

2. 依次导入以下 unitypackage 到项目中：
    - [com.google.play.assetdelivery-1.5.0.unitypackage](https://github-releases.githubusercontent.com/248128393/d78d1f00-d8f1-11eb-91c3-90d60ff1d245?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIWNJYAX4CSVEH53A%2F20210811%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20210811T025949Z&X-Amz-Expires=300&X-Amz-Signature=7a0dd4941300e2df311cc163a0b4811a321312c4149d8679025e13bf51998107&X-Amz-SignedHeaders=host&actor_id=25072236&key_id=0&repo_id=248128393&response-content-disposition=attachment%3B%20filename%3Dcom.google.play.assetdelivery-1.5.0.unitypackage&response-content-type=application%2Foctet-stream)
    - cn.game4d.xasset.pad 7.0.4.unitypackage

3. 打包测试
    - 勾选 Unity 编辑器的 BuildSettings 的 Build App Bundle(Google Play)
    - 打包资源后，执行 Google/Build And Run 可以一键安装 aab 包到已经连接的 Android 设备
    - 打包资源后，执行 Google/Build Android App Bundle 可以生成用来提交到 GooglePlay 的 aab 包

如果对 PAD 不太了解，可以参考：

- https://developer.android.com/guide/playcore/asset-delivery
- https://developer.android.com/guide/app-bundle/asset-delivery/build-unity