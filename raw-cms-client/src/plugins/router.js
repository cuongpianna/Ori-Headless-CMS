import Vue from 'vue'
import Router from 'vue-router'
import Layout from '@/components/layout/layout'
import loginService from "../services/login.service";
import {optionalChain} from "../utils/object.util";

Vue.use(Router)

export const constantRoutes = [
    {
        path: '/login',
        name: 'Login',
        component: () => import('@/views/login/login'),
        meta: {requiresAuth: false}
    },
    {
        path: '/schemas',
        redirect: '/',
        component: Layout,
        children: [
            {
                path: '',
                name: 'SchemaList',
                component: () => import('@/views/schema/schema-view')
            }
        ]
    }
]

const createRouter = () => new Router({
    scrollBehavior: () => ({y: 0}),
    routes: constantRoutes
})

const router = createRouter()

router.beforeEach((to, from, next) => {
    if (to.matched.some(r => !optionalChain(() => r.meta.requiresAuth, {fallbackValue: true}))) {
        next();
        return;
    }

    if (loginService.isLoggedIn) {
        next()
        return;
    }

    next('/login')

    next({
        path: '/login',
        params: {nextUrl: to.fullPath},
    });
})

export function resetRouter() {
    const newRouter = createRouter()
    router.matcher = newRouter.matcher // reset router
}

export default router