import userService from "../../services/user.service";

const state = {
    test: 0,
    isLoggedIn: false
}

const mutations = {
    isLoggedIn(state, value) {
        state.isLoggedIn = value;
    },
    setUserInfo(state, value) {
        state.userInfo = value;
    }
}

const actions = {
    async isLoggedIn({ commit, dispatch }, value) {
        commit('isLoggedIn', value);
        if (!value) {
            return;
        }

        const userInfo = await userService.getUserInfo();
        commit('setUserInfo', userInfo);

        dispatch('updateFieldsMetadata', null, {root:true});
    }
}

export default {
    namespaced: true,
    state,
    mutations,
    actions
}
