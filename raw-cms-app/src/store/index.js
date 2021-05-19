import Vue from 'vue'
import Vuex from 'vuex'
import auth from './auth/auth';

Vue.use(Vuex)

const store = new Vuex.Store({
  auth
})

export default store
