// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'xasset',
  tagline: '全面可靠的 Unity 资源管理神器 🔥',
  url: 'https://xasset.github.io',
  baseUrl: '/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/logo.png',
  organizationName: 'xasset', // Usually your GitHub org/user name.
  projectName: 'xasset', // Usually your repo name.

  presets: [
    [
      '@docusaurus/preset-classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          editUrl: 'https://github.com/xasset/xasset/edit/main/website/',
        },
        blog: {
          showReadingTime: true,
          // Please change this to your repo.
          editUrl:
            'https://github.com/xasset/xasset/edit/main/website/blog/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({ 
      // algolia: {
      //   contextualSearch: true,
      // },
      navbar: {
        title: 'xasset',
        logo: {
          alt: 'xasset logo',
          src: 'img/logo.png',
        },
        items: [
          {
            type: 'doc',
            docId: 'intro',
            position: 'left',
            label: '文档',
          }, 
          {
            type: 'doc',
            docId: 'price',
            position: 'left',
            label: '订阅',
          }, 
          { to: '/blog', label: '博客', position: 'left' },
          {
            href: 'https://github.com/xasset/xasset',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: '文档',
            items: [
              {
                label: '教程',
                to: '/docs/intro',
              },
            ],
          },
          {
            title: '社区',
            items: [
              {
                label: 'Stack Overflow',
                href: 'https://stackoverflow.com/questions/tagged/xasset',
              },
              {
                label: 'QQ 群',
                href: 'https://jq.qq.com/?_wv=1027&k=I2gC9wKu',
              },
            ],
          },
          {
            title: '更多',
            items: [
              {
                label: '博客',
                to: '/blog',
              },
              {
                label: 'GitHub',
                href: 'https://github.com/xasset/xasset',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} xasset.`,
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
        additionalLanguages: ['csharp'],
      }, 
    }),
};

module.exports = config;
