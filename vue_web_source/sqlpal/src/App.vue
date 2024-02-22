<template>
  <div id="app">
    <router-view />
  </div>
</template>

<script>
import SqlMain from "./components/SqlMain.vue";

export default {
  name: "app",
  components: {
    SqlMain
  },
  data() {
    return {
      title: "Sql Pal"
    };
  }
};
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

/* generic */
/* .tblclass {
  border-spacing: 0;
  border-collapse: collapse;
}

* {
  box-sizing: border-box;
} */

.tblclass tr td {
  white-space: nowrap;
}

/*
<td id="L1" class="blob-num js-line-number" data-line-number="1"></td>
*/

.blob-num {
  width: 1%;
  min-width: 50px;
  padding-right: 10px;
  padding-left: 10px;
  font-family: SFMono-Regular, Consolas, Liberation Mono, Menlo, monospace;
  font-size: 12px;
  line-height: 20px;
  /* line-height: 1.0; */
  /* color: var(--color-diff-blob-num-text); */
  color: gray;
  text-align: right;
  white-space: nowrap;
  vertical-align: bottom;
  cursor: pointer;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  user-select: none;
}

.blob-num:before {
  content: attr(data-line-number);
}

/* <td id="LC1" class="blob-code blob-code-inner js-file-line"></td>*/

.blob-code {
  position: relative;
  padding-right: 10px;
  padding-left: 10px;
  line-height: 20px;
  /* line-height: 1.0; */
  vertical-align: bottom;
}

.blob-code-inner {
  overflow: visible;
  font-family: SFMono-Regular, Consolas, Liberation Mono, Menlo, monospace;
  font-size: 12px;
  /* color: var(--color-text-primary); */
  word-wrap: normal;
  white-space: pre;
}
.blob-code-inner::selection,
.blob-code-inner ::selection {
  background-color: #ccddff;
}
</style>
