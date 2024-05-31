
<template>
  <div id="app">
    <lama-vue v-if="false"></lama-vue>
    <hr />
    <div class="container-fluid">
      <builder ref="builder" v-model="model" :connector="connector"></builder>
    </div>
  </div>
</template>

<script>
//import HelloWorld from './components/HelloWorld.vue'

import { App, Builder, Lama } from "lamavue";
//import from 'lamavue'

export default {
  name: "App",
  data() {
    return {
      model: {
        //schema: {},
        options: {},
      },
      connector: Lama.getConnectorClass("default"),
    };
  },
  components: {
    LamaVue: App,
    Builder,
  },
};
</script>

<style>
</style>
