const path = require('path')

function resolve(dir) {
    return path.join(__dirname, dir)
}


const port = process.env.port || process.env.npm_config_port || 9527

module.exports = {
    publicPath: '/',
    outputDir: 'dist',
    // assetsDir: 'static',
    lintOnSave: process.env.NODE_ENV === 'development',
    productionSourceMap: false,
    configureWebpack: {
        // provide the app's title in webpack's name field, so that
        // it can be accessed in index.html to inject the correct title.
        resolve: {
            alias: {
                '@': resolve('src')
            }
        }
    }
}