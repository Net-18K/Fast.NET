import { createRouter, createWebHistory } from "vue-router";
import NProgress from "nprogress";
import "nprogress/nprogress.css";
import { whiteRoutes } from "./modules/whiteRoute";
import { loading } from "@/hooks/loading";
import langAutoLoadMap from "@/lang/autoLoad";
import { mergeMessage } from "@/lang/index";
import { useConfig } from "@/stores/config";

const constantRoutes = [...whiteRoutes];

const router = createRouter({
    history: createWebHistory(import.meta.env.VITE_PUBLIC_PATH),
    routes: constantRoutes,
});

/**
 * 配置 NProgress
 */
NProgress.configure({
    // 动画方式
    easing: "ease",
    // 递增进度条的速度
    speed: 500,
    // 是否显示加载ico
    showSpinner: false,
    // 自动递增间隔
    trickleSpeed: 200,
    // 初始化时的最小百分比
    minimum: 0.3,
});

/**
 * 路由加载前
 */
router.beforeEach((to, from, next) => {
    // 开启进度条
    NProgress.start();

    // 判断是否已经存在加载动画
    if (!window.existLoading) {
        // 显示加载动画
        loading.show();
        window.existLoading = true;
    }

    // 按需动态加载页面的语言包
    let loadPath: string[] = [];

    // 判断当前路由是否存在按需加载的语言包
    if (to.path in langAutoLoadMap) {
        loadPath.push(...langAutoLoadMap[to.path as keyof typeof langAutoLoadMap]);
    }

    const configStore = useConfig();

    const prefix = `./${configStore.lang.defaultLang}/`;

    for (const key in loadPath) {
        // 替换语言
        loadPath[key] = loadPath[key].replaceAll("${lang}", configStore.lang.defaultLang);
        // 判断是否存在语言包句柄中
        if (loadPath[key] in window.loadLangHandle) {
            window.loadLangHandle[loadPath[key]]().then((res: { default: anyObj }) => {
                // 这里默认删除 ./${lang}/ 前缀
                const pathName = loadPath[key].slice(prefix.length, loadPath[key].lastIndexOf("."));
                /**
                 * 这里处理 index.ts 文件后缀的问题
                 * 原：utils.axios.index.
                 * 处理后：utils.axios.
                 */
                const prefixName = pathName.endsWith("index") ? pathName.substring(0, pathName.lastIndexOf("/")) : pathName;
                mergeMessage(res.default, prefixName);
            });
        }
    }

    next();
});

/**
 * 路由加载后
 */
router.afterEach(() => {
    if (window.existLoading) {
        loading.hide();
    }
    NProgress.done();
});

export default router;
