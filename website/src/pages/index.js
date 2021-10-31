import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import styles from './index.module.css';

function HomepageHeader() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <header className={clsx('hero hero', styles.heroBanner)}>
      <div className="container">
        <h1 className="hero__title">{siteConfig.title}</h1>
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        <div className={styles.buttons}>
          <Link
            className="button button--primary button--lg"
            to="docs/intro">
            快速入门&nbsp;&nbsp;→
          </Link>
        </div>
      </div>
    </header>
  );
}

function FeaturesHeader() {
  return (<div className={styles.sectionDark}>
    <div className="container padding-vert--md">
      <div className="row">
        <div className="col col--8 col--offset-2">
          <div className="margin-vert--lg text--center">
            <h2 className={styles.sectionDarkTitle}>
              5年打磨，1500+星标，190+订阅，45+公司团队的选择...不断进行自我迭代，专治 Unity 项目包体大、打包慢、边玩边下、运行卡顿等疑难杂症。
              {/* <a
          className={styles.sectionDarkLink}
          href="https://docusaurus.io"
          rel="noreferrer noopener"
          target="_blank">
          xasset 7.0
        </a> */}
            </h2>
          </div>
        </div>
      </div>
    </div>
  </div>)
}

function FeatureItems() {
  return (<p className="padding-vert--xl">
    <p className="container">
      <p className="row">
        <p className="col col--10 col--offset-1">
          <div className="row margin-vert--lg">
            <div className="col">
              <h3>快速打包和迭代</h3>
              <p>
                10万贴图10分钟完成打包，支持自动分组可以有效避免95%的常规资源打包冗余问题。支持仿真模式，编辑器下可以跳过打包快速运行，支持增量模式，编辑器下可以模拟和真机一样的版本管理环境。
              </p>
            </div>
            <div className="col">
              <h3>灵活把控安装包大小</h3>
              <p>
                提供配置驱动的资源分包技术，可以针对不同的应用场景，预定于多组安装包资源配置，安装包最小可以轻松控制到 30 MB。适配了最新的 Google 的 AAB 和 PAD 分包技术，安装包可以轻松突破 150MB 的限制。
              </p>
            </div>
            <div className="col">
              <h3>按需处理边玩边下</h3>
              <p>
                既可以全量更新也可以局部更新，并且使用只读的物理文件数据管理资源的版本信息，可以让版本管理的稳定性和效率得到前所未有的提升，支持断点续传，支持限速下载，可以轻松获取下载进度、速度和大小。
              </p>
            </div>
          </div>
          <div className="row margin-vert--lg">
            <div className="col">
              <h3>简明加载</h3>
              <p>
                统一使用相对路径加载资源或场景，自动处理依赖关系，支持异步转同步，支持自定义加载路径，使用引用计数进行内存管理，可以有效避免重复加载和回收障碍，Profiler 测试具备进多少、出多少的稳定性。
              </p>
            </div>
            <div className="col">
              <h3>安全高效</h3>
              <p>
                修改一个设置选项就能自动对安装包的资源进行加密处理，不仅可以防止资源轻易被 AssetStudio 之类的工具破解，并且 Android 真机测试资源的加载耗时可以达到 ～10% 左右的提升。</p>
            </div>
          </div>
        </p>
      </p>
    </p>
  </p>)
}

export default function Home() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <Layout
      title={`${siteConfig.tagline}`}
      description="Description will go into a meta tag in <head />">
      <HomepageHeader />
      <main>
        <FeaturesHeader />
        <FeatureItems />
      </main>
    </Layout>
  );
}