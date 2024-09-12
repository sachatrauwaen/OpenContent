module.exports = {
  runtimeCompiler: true,
  css: {
    extract: true
  },
  filenameHashing: false,

  chainWebpack: config => {
    //config.plugins.delete('html')
    config.plugins.delete('preload')
    config.plugins.delete('prefetch')
    },
    pages: {
        app: 'src/main.js',
        init: 'src/init.js'
    }
}