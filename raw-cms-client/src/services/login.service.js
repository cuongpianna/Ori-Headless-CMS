import store from "../store";
import axios from "axios";

class LoginService{
    _auth;

    get auth() {
        if (!this._auth && localStorage.getItem('auth') !== null) {
            this._auth = JSON.parse(localStorage.getItem('auth'));
        }

        return this._auth;
    }

    get isLoggedIn() {
        console.log(store.state.auth.isLoggedIn);
        if(!store.state.auth.isLoggedIn) {
            this._refreshLoginState();
        }

        return store.state.auth.isLoggedIn;
    }

    async login(username, password) {
        const params = new URLSearchParams();
        params.append('grant_type', 'password');
        params.append('scope', 'openid');
        params.append('client_id', 'raw.client');
        params.append('client_secret', 'raw.secret');
        params.append('password', password);
        params.append('username', username);

        console.log(username, password)
        return axios.post('http://localhost:5000/connect/token', params).then(res => {
            console.log(res);
            this._auth = res.data;
            localStorage.setItem('auth', JSON.stringify(res.data));
            this._refreshLoginState();
        })
    }

    logout() {

    }

    _refreshLoginState() {
        store.dispatch('auth/isLoggedIn', localStorage.getItem('auth') !== null);
    }
}

export const loginService = new LoginService();
export default loginService;