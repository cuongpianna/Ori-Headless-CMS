import Vue from 'vue'
import App from './App.vue'
import router from './plugins/router'
import store from "./store";
import Cookies from 'js-cookie'

// Import all plugins
import Element from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'
import viLang from 'element-ui/lib/locale/lang/vi'
import "./plugins/vuetify"

Vue.use(Element, {
  size: Cookies.get('size') || 'medium',
  locale: viLang
})

Vue.config.productionTip = false

new Vue({
  render: h => h(App),
  router,
  store
}).$mount('#app')
