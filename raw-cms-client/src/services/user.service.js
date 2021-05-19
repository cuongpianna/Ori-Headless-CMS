import { apiClient } from './api-client';


class UserService{
    async getUserInfo() {
        try {
            const res = await apiClient.get(`/connect/userinfo`);
            return res.data;
        } catch (e) {
            // snackbarService.showMessage({
            //     color: 'error',
            //     message: e,
            // });
        }
    }
}

export const userService = new UserService();
export default userService;
