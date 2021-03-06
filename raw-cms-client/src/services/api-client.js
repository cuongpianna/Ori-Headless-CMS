import axios from 'axios'
import loginService from "./login.service";
import {optionalChain} from "../utils/object.util";

const service = axios.create({
    baseURL: `http://localhost:5000`,
});

service.interceptors.request.use(request => {
    if (loginService.isLoggedIn) {
        request.headers.common['Authorization'] = `Bearer ${loginService.auth.access_token}`;
    }
    return request;
});

service.interceptors.response.use(
    response => response,
    error => {
        const isUnauthorized = optionalChain(() => error.response.status, { fallbackValue: 0 }) === 401;
        if (isUnauthorized) {
            loginService.logout();
        } else {
            Promise.reject(error);
        }
    }
);

export const apiClient = service;
export default apiClient;