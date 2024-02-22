<template>
  <div v-if="show" class="mymodal">
    <div class="mymodal-content">
      <span class="close" @click="$emit('close')">&times;</span>
      <!-- <p>{{ content }}</p> -->
      <!-- <div ref="sqleditor2"></div> -->
      <!-- <textarea  ref="sqleditor2" v-model="content" ></textarea> -->

      <!-- <textarea class="form-control" v-model="content"  style="height: 400px"></textarea> -->

      <pre id="modalContentId" v-html="content"></pre>
    </div>
  </div>
</template>

<script>
// import CodeMirror from 'codemirror';
// import 'codemirror/mode/sql/sql.js';
// import 'codemirror/lib/codemirror.css';
// import 'codemirror/theme/ssms.css';

export default {
  name: "Modal",
  props: ["show", "content"],
  data() {
    return {
      editorModal: null,
      localContent: this.content
    };
  }

  // mounted() {

  //   this.editorModal = CodeMirror.fromTextArea(this.$refs.sqleditor2, {
  //     mode: "text/x-mssql",
  //     theme: "ssms",
  //     lineNumbers: true,
  //     readOnly: true,
  //   });

  //   //this.editorModal.setValue(this.localContent);

  //   this.editorModal.setSize("100%", "500px");
  //   this.editorModal.refresh();

  // },

  // beforeDestroy() {
  //   if (this.editorModal) {
  //     this.editorModal.toTextArea();
  //   }
  // },

  // methods: {
  //   setValue(value) {
  //     if (this.editorModal) {
  //       this.editor.setValue(value);
  //       //this.editorModal.setValue(this.localContent);
  //     }
  //   },
  //   getValue() {
  //     if (this.editorModal) {
  //       return this.editorModal.getValue();
  //     }
  //     return '';
  //   },
  // },
};
</script>

<style scoped>
.mymodal {
  display: block; /* Hidden by default */
  position: fixed; /* Stay in place */
  z-index: 1; /* Sit on top */
  left: 0;
  top: 0;
  width: 100%; /* Full width */
  height: 100%; /* Full height */
  overflow: auto; /* Enable scroll if needed */
  background-color: rgb(0, 0, 0); /* Fallback color */
  background-color: rgba(0, 0, 0, 0.4); /* Black w/ opacity */
}
.mymodal-content {
  background-color: #fefefe;
  margin: 15% auto; /* 15% from the top and centered */
  padding: 20px;
  border: 1px solid #888;
  width: 80%; /* Could be more or less, depending on screen size */
}

.close {
  color: #aaa;
  float: right;
  font-size: 28px;
  font-weight: bold;
}

.close:hover,
.close:focus {
  color: black;
  text-decoration: none;
  cursor: pointer;
}
</style>
