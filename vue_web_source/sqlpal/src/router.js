import Vue from "vue";
import Router from "vue-router";
import SqlMain from './components/SqlMain.vue'

Vue.use(Router);

export default new Router({
  routes: [
    {
      path: "/",
      name: "SqlMain",
      component: SqlMain
    },   
    {
      path: "/:objectType/:server/:database/:dbschema/:dbobject", 
      name:"SqlMainId",     
      component: SqlMain
    }, 
  ]
});
