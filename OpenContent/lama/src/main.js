import Vue from 'vue'
//import App from './App.vue'
import UserRoleField from './components/UserRoleField.vue'
import CollectionField from './components/CollectionField.vue'

// eslint-disable-next-line no-unused-vars
import { App, Builder, Lama } from "lamavue";

Vue.config.productionTip = false
Lama.registerFieldComponent("userrole", UserRoleField);
Lama.registerFieldComponent("collection", CollectionField);

/*
new Vue({
  render: h => h(App),
}).$mount('#app')
*/