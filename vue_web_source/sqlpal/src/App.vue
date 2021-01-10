<template>
  <div id="app">    
    <router-view />
  </div>
</template>

<script>
import SqlMain from './components/SqlMain.vue'

export default {
  name: 'app',
  components: {
    SqlMain
  },
  data() {
    return {
      title: 'Sql Pal'
    }
  }
}


</script>
<style>
#app {
  font-family: "Avenir", Helvetica, Arial, sans-serif;
  /* -webkit-font-smoothing: antialiased; */
  /* -moz-osx-font-smoothing: grayscale; */
  /* text-align: center; */
  color: #2c3e50;
}
#nav {
  padding: 30px;
}

#nav a {
  font-weight: bold;
  color: #2c3e50;
}

#nav a.router-link-exact-active {
  color: #42b983;
}

/* 
#sqlcode ol {
    margin: 0;
    padding: 0;
    list-style: none;
    counter-reset: line_number;
}

#sqlcode ol li {
  counter-increment: line_number;
}

#sqlcode ol li::before {
  content: counter(line_number) " ";
  color: lightgray;  

  margin-top: 0px;
  margin-bottom: 0px;
  margin-right: 16px;
  margin-left: 0px;

} */


.olclass {
    list-style-type: none;
    /* margin: 8px 0 0; */
    padding-left: 0;
    counter-reset: line;
}

.olclass li {
    position: relative;
    display: block;
    /* clear: both; */
    width: 100%;
    /* font-size: 13px; */
    line-height: 12px; 
    white-space: pre-wrap;
}

.olclass li .line {
    display: inline-block;
    /* float: right; */
    width: calc(100% - 70px);
}

.olclass li::before {
    content: counter(line);
    display: inline-block;
    /* float: left; */
    width: 40px;
    margin-right: 20px;
    margin-top: 10px;
    color: gray;
    text-align: right;
    font-style: normal;
    counter-increment: line;
}


.preMargin {
  margin-top: 0;
  margin-bottom: 0;
  margin-right: 0;
  margin-left: 8px;
  padding: 0p;  
  white-space: pre-wrap; 
  word-break: break-word;

}


</style>
