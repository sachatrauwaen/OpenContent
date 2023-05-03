import Vue from 'vue'
//import App from './App.vue'
import UserRoleField from './components/UserRoleField.vue'

// eslint-disable-next-line no-unused-vars
import { App, Builder, Lama } from "lamavue";

Vue.config.productionTip = false
Lama.registerFieldComponent("userrole", UserRoleField);
/*
new Vue({
  render: h => h(App),
}).$mount('#app')
*/