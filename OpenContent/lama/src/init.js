import Vue from 'vue'
import App from './InitApp.vue'
Vue.config.productionTip = false


//new Vue({
//  render: h => h(App),
//}).$mount('#app')

let initOpenContent = {

    mount(elementOrSelector, config) {
        let app = new Vue({
            data: {
                config: config
            },
            render: h => h(App),
            mounted() {
            },
        }).$mount(elementOrSelector);
        return app;
    },
};

if (typeof window !== 'undefined') {
    window.OpenContent = window.OpenContent || {};
    window.OpenContent.init = initOpenContent;
    //Vue.use(Lama);
}