XAsset为Unity项目提供了简便的资源管理环境，对于资源到包XAsset目前仅需按需配置对应的BuildRule，XAsset就会自动把所有规则中的公共资源剥离出来，先对公共资源进行打包然后再把非公共资源按当前规则进行独立打包。

通过剥离公共资源，可以降同一个低资源在不同AssetBundle中的冗余，为尽可能的减少包体数量，公共资源也会按其所在的路径和数量把相同路径（非递归）下的资源打进同一个AssetBundle，所以用XAsset进行资源打包的前，我们要尽可能把会在游戏中同时用到的资源放到同一个目录下。

需要注意的是对于Unity项目，不要把需要打包AssetBundle的资源放到Resources目录下，否则会造成项目中AssetBundle和Resources的资源冗余。另外，对于图集，同一个图集所引用的贴图必须要放在同一个AssetBundle不然图集也会冗余。

默认情况下只要开启了BUILD_ATLAS宏，XAsset就会自动根据项目中所有被打包的贴图的AssetBundleName进行图集打包，所以对于使用了XAsset的Unity项目而言，依赖打包、图集打包都是非常“简便”的事情。最后，要实现比较理想的资源打包方案，主要需要做到以下几点：

1. 尽可能的把游戏中同时使用的资源放到同一个目录，不同类型的可以区别对待。
2. 资源目录规划要做动静分离处理，不变的遵从1，会变的分到另外一个目录，内部也按1对资源进行目录安置。
3. 在rules.ini中为需要打包的资源配置好相应的构建规则，prefab通常可以按文件名打包，但是如果把1，2做好可以按路径进行打包。

> PS: 如果你觉得XAsset好用，请上GitHub Star一个，<https://github.com/fengjiyuan/XAsset>，如果有什么建议或需要技术支持请加QQ群交流：693203087

**附：对BuildRule的补充说明**

- 关于BuildRule的用法，XAsset主要包括以下BuildRule：

  1. BuildAssetsWithAssetBundleName

     粒度大，适用于无依赖的切体积小的资源

  2. BuildAssetsWithDirectroyName

     粒度较大，适用于按1，2归类好的资源

  3. BuildAssetsWithFilename 

     粒度小，适用于依赖多，易变的资源，例如Prefab


- BuildRule的配置格式为：

  [RuleName]                                        // 目前仅对上面的BuildRule有效

  searchPath=Assets/SampleAssets // 搜索路径

  searchPattern=*.prefab                   // 通配符

  searchOption=AllDirectories           // 是否递归，不递归填TopDirectoryOnly

  bundleName=                                    // 仅仅针对规则BuildAssetsWithAssetBundleName有效