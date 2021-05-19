import Vue from 'vue'
import Vuex from 'vuex'
import auth from './modules/auth'
import metadataService from "../services/metadata.service";

Vue.use(Vuex)

const state = {
    fieldsMetadata: undefined
}

const actions = {
    async updateFieldsMetadata({ commit }) {
        const metadata = await metadataService.getFieldsMetadata();
        commit('setFieldsMetadata', metadata);
    }
}

const mutations = {
    setFieldsMetadata(state, value) {
        state.fieldsMetadata = value;
    }
}

const store = new Vuex.Store({
    modules: {
        auth
    },
    state: state,
    actions: actions,
    mutations:  mutations
})

export default store
