module.exports = {
    css: {
      extract: true
    },
    filenameHashing:false,
    
    chainWebpack: config => {
      //config.plugins.delete('html')
      config.plugins.delete('preload')
      config.plugins.delete('prefetch')
    }
  }