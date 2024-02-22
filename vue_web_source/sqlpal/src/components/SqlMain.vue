<template>
  <div id="SqlMain" class="flx-body">
    <div class="flx-header">
      <header>
        <div>
          <table>
            <tr>

              <td style="padding-left: 12px">
                <!-- Server: {{server}} -->
                <button @click="selectTableType" type="button" class="btn btn-secondary btn-sm">tbl</button>
                &nbsp;
                <button @click="selectSPType" type="button" class="btn btn-secondary btn-sm">sp</button>
                &nbsp;
                <button @click.stop.prevent="updateClip('#idclipbrd')" type="button" class="btn btn-secondary btn-sm">cb</button>
                &nbsp;
                <button @click.stop.prevent="updateClip('#idclipbrdNoServer')" type="button" class="btn btn-secondary btn-sm">cb2</button>
                &nbsp;
                <button @click.stop.prevent="updateClip('#idclipbrdSelectStar')" type="button" class="btn btn-secondary btn-sm">s*</button>
                &nbsp; &nbsp;&nbsp;
                <button @click="openInNotepadd" type="button" class="btn btn-secondary btn-sm">oNPP</button>
                &nbsp;
                <button @click="fngetHighlightedText" type="button" class="btn btn-secondary btn-sm">Def</button>
                &nbsp;
                <button @click="fnGetTableColumns" type="button" class="btn btn-secondary btn-sm">Cols</button>
                &nbsp;
                <input type="hidden" id="idclipbrd" :value="'[' + this.server + '].' + this.database + '.' + this.selectedSechemaName + '.' + this.selectedObjectName" />
                <input type="hidden" id="idclipbrdNoServer" :value="this.database + '.' + this.selectedSechemaName + '.' + this.selectedObjectName" />
                <input type="hidden" id="idclipbrdSelectStar" :value="'SELECT TOP 100 * FROM ' + this.database + '.' + this.selectedSechemaName + '.' + this.selectedObjectName" />
              </td>

              <td colspan="2" style="padding-left: 12px" ref="fullObjectName">
                <!-- Object: {{selectedSechemaName}}.{{selectedObjectName}} -->
                <span style="background-color: lightsalmon;">[{{ server }}]</span>
                <span>.</span>
                <span style="background-color: lightblue;">{{ database }}</span>
                <span>.</span>
                <span style="background-color: lightgreen;">{{ selectedSechemaName }}.{{ selectedObjectName }}</span>
                <!-- [{{ server }}].{{ database }}.{{ selectedSechemaName }}.{{ selectedObjectName }} -->
              </td>

              <td style="padding-left: 12px">
                <div v-show="progressFlag" class="spinner-border text-primary spinner-border-sm" role="status">
                  <span class="sr-only">Loading...</span>
                </div>
              </td>

            </tr>
            
            <tr>


              <td style="padding-left: 12px">
                <div class="input-group mb-3">
                  <span class="input-group-text">ServerList</span>
                  <button @click="clearServerFilter" type="button" class="btn btn-secondary btn-sm">x</button>
                  <input style="margin-left:1px; margin-right:1px; width:60px;" v-model="serverFilter" type="text" class="form-control" v-on:keyup="doServerListFiltered" />
                  <select class="form-select" v-model="server" @change="serverSelected($event)" style="min-width: 240px;">
                    <option disabled value="">Please select server</option>
                    <option v-for="(srvr, index) in serverListFiltered" v-bind:value="srvr" v-bind:key="index">
                      {{ srvr }}
                    </option>
                  </select>                  
                </div>
              </td>

              <td style="padding-left: 12px">
                <div class="input-group mb-3"> 
                  <span class="input-group-text">Database</span>
                  <button @click="clearDatabaseFilter" type="button" class="btn btn-secondary btn-sm">x</button>
                  <input style="margin-left:1px; margin-right:1px; width:60px;" v-model="databaseFilter" type="text" class="form-control" v-on:keyup="doDatabaseListFiltered" />
                  <select class="form-select" v-model="database" @change="dbSelected($event)" style="min-width: 240px;">
                    <option disabled value="">Please select database</option>
                    <option v-for="(db, index) in databaseListFiltered" v-bind:value="db" v-bind:key="index">
                      {{ db }}
                    </option>
                  </select>                  
                </div>
              </td>

              <td style="padding-left: 12px">
                <div class="input-group mb-3">
                  <button @click="addServerName" type="button" class="btn btn-primary btn-sm">Add Server</button>
                  &nbsp;
                  <input v-model="newServerName" type="text" class="form-control" />
                </div>
              </td>


              <td style="padding-left: 12px; min-width: 200px;">
                <div id="errorMessage">{{ errorMessage }}</div>
              </td>

            </tr>
          </table>
        </div>
      </header>
    </div>

    <div class="flx-content">
      <!-- Left Pane -->
      <div class="flx-pane flx-left">
        <div id="objectNavigator">
          <table>
            <tr>
              <td colspan="3">
                <div class="input-group mb-3" >
                  <span class="input-group-text">Object Type</span>

                  <select class="form-select" v-model="objectType" @change="objectTypeChange($event)">
                    <option disabled value="">-----Please select object type-----</option>
                    <option v-for="ot in objectTypeList" v-bind:key="ot">
                      {{ ot }}
                    </option>
                  </select>
                </div>
              </td>
            </tr>
            <tr>
              <td colspan="3">
                <div class="input-group mb-3">
                  <span class="input-group-text">Object Filter</span>
                  <input id="input_box_obj_filter" type="text" class="form-control" v-model="objFilter" v-on:keyup="filterCurrentObjects" />
                </div>
              </td>
            </tr>
          </table>

          <div id="objectListContainer">
            <ol id="objectList">
              <li v-for="object in currentObjectList" v-bind:key="object">
                <!-- <a v-on:click.prevent="linkClick" v-bind:href="'#!/' + object.schema_name + '.' + object.name">{{object.schema_name}}.{{object.name}}</a> -->
                <router-link
                  :to="{
                    name: 'SqlMainId',
                    params: {
                      objectType: objectType,
                      server: server,
                      database: database,
                      dbschema: object.schema_name,
                      dbobject: object.name,
                    },
                  }"
                  >{{ object.schema_name }}.{{ object.name }}</router-link
                >
              </li>
            </ol>
          </div>
        </div>
      </div>
      <!-- End Left Pane -->

      <div class="resizer"></div>

      <!-- Right Pane -->
      <div class="flx-pane flx-right">
        <main>
          <div>
            <b-tabs content-class="mt-3">
              <b-tab title="Object Browser" active>
                <section>
                  <div id="textCtrl">
                    <pre ref="refTblData" class="code" v-html="objectText" style="overflow: unset"></pre>
                    <br /><br />
                    <pre id="text" v-html="objectTblData" style="overflow: unset"></pre>
                  </div>
                </section>
              </b-tab>

              <b-tab title="SQL Editor">
                <!-- <span>Enter AdHoc SQL Query below:</span> -->

                <br />
                <!-- <textarea rows="10" cols="80" v-model="sqlQuery" placeholder="type sql command here" id="sqleditor1"></textarea> -->
                <textarea ref="sqleditor1"></textarea>
                <br />
                <button @click="QueryDatabase" type="button" class="btn btn-primary btn-sm">Execute</button>&nbsp; &nbsp;
                <button @click="formatSql" type="button" class="btn btn-primary btn-sm">Format SQL</button>
                <br />
                <pre id="adhocsql" v-html="sqlQueryResponse" style="overflow: unset"></pre>
              </b-tab>

              <b-tab title="Search sys objects">
                <table>
                  <tr>
                    <td style="width: 75px">
                      <div class="form-check">
                        <input class="form-check-input" type="checkbox" v-model="searchSP" true-value="true" false-value="false" />
                        <label class="form-check-label">SP</label>
                      </div>
                    </td>
                    <td style="width: 75px">
                      <div class="form-check">
                        <input class="form-check-input" type="checkbox" v-model="searchTable" true-value="true" false-value="false" />
                        <label class="form-check-label">Tbl</label>
                      </div>
                    </td>
                    <td style="width: 75px">
                      <div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchColumn" true-value="true" false-value="false" /><label class="form-check-label">Col</label></div>
                    </td>
                    <td style="width: 75px">
                      <div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchView" true-value="true" false-value="false" /><label class="form-check-label">View</label></div>
                    </td>
                    <td style="width: 120px">
                      <div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchSynonym" true-value="true" false-value="false" /><label class="form-check-label">Synonym</label></div>
                    </td>
                    <td style="width: 120px">
                      <div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchAllDbs" true-value="true" false-value="false" /><label class="form-check-label">All Dbs</label></div>
                    </td>
                    <td style="width: 120px">
                      <div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchAllServers" true-value="true" false-value="false" /><label class="form-check-label">All Servers</label></div>
                    </td>
                  </tr>
                </table>

                <!-- <textarea rows="10" cols="80" v-model="spSearchText" placeholder="type text here to search"></textarea> -->
                <div class="form-floating">
                  <textarea class="form-control" v-model="spSearchText" placeholder="type text here to search" style="height: 200px"></textarea>
                  <label>type text here to search</label>
                </div>

                <br />
                <button @click="fnSpSearch" type="button" class="btn btn-primary btn-sm">Search</button>
                <br />
                <div id="spSearchResponseId">
                  <ul>
                    <li v-for="object in spSearchResponse" v-bind:key="object">
                      {{ object }}
                    </li>
                  </ul>
                </div>
              </b-tab>

              <b-tab title="Diff">
                <table>
                  <tr>
                    <td>
                      <div style="margin-bottom: 10px">
                        <button @click="fnLeftDiff" type="button" class="btn btn-secondary">&lt;&lt;</button>
                        &nbsp;
                        <button @click="fnRightDiff" type="button" class="btn btn-secondary">&gt;&gt;</button>
                      </div>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <div class="input-group mb-3">
                        <div class="form-check">
                          <input class="form-check-input" type="checkbox" v-model="leftIsFileOnDisk" true-value="true" false-value="false" />
                          <label class="form-check-label">fl</label>
                        </div>
                        &nbsp;
                        <span class="input-group-text">Left</span>
                        <input type="text" class="form-control" v-model="diffLeft" size="140" />
                      </div>
                    </td>
                  </tr>

                  <tr>
                    <td>
                      <div class="input-group mb-3">
                        <div class="form-check">
                          <input class="form-check-input" type="checkbox" v-model="rightIsFileOnDisk" true-value="true" false-value="false" />
                          <label class="form-check-label">fl</label>
                        </div>
                        &nbsp;
                        <span class="input-group-text">Right</span>
                        <input type="text" class="form-control" v-model="diffRight" size="140" />
                      </div>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <button @click="fnDiffCompare" type="button" class="btn btn-primary">Compare</button>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      {{ diffMessage }}
                    </td>
                  </tr>
                </table>
              </b-tab>

              <b-tab title="ObjDependency">
                <table>
                  <tr>
                    <td>
                      <div style="margin-bottom: 10px">
                        <button @click="fnDependentFillName" type="button" class="btn btn-secondary">Get Object Name</button>
                      </div>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <div class="input-group mb-3">
                        <span class="input-group-text">Object Name</span>
                        <input type="text" class="form-control" v-model="objDependent" size="140" />
                      </div>
                    </td>
                  </tr>

                  <tr>
                    <td>
                      <div class="input-group mb-3">
                        <span class="input-group-text">RegEx</span>
                        <input type="text" class="form-control" v-model="regexDependent" size="140" />
                      </div>
                    </td>
                  </tr>

                  <tr>
                    <td>
                      <button @click="fnDependentSearch" type="button" class="btn btn-primary">Search Dependency</button>
                    </td>
                  </tr>
                </table>

                <div id="IdDependentResponse">
                  <br /><br />
                  <pre id="text" v-html="objDependentResponse" style="overflow: unset"></pre>
                </div>
              </b-tab>

              <b-tab title="Formatter">
                <div class="form-floating">
                  <textarea class="form-control" v-model="codeToFormat" placeholder="enter code" style="height: 600px"></textarea>
                </div>

                <br />
                <button @click="formatColumns" type="button" class="btn btn-primary btn-sm">Format Columns</button>
              </b-tab>
            </b-tabs>
          </div>
        </main>
      </div>
      <!-- End Right Pane -->
    </div>

    <Modal :show="showModal" :content="modalContent" @close="showModal = false" />

    <!-- <div class="flx-footer">SqlPal &copy; Copyleft 2023</div> -->
  </div>
</template>

<script>
import axios from "axios";
import bs58 from "bs58";
import * as CodeMirror from "codemirror";
import "codemirror/mode/sql/sql.js";
import "codemirror/lib/codemirror.css";
import "codemirror/theme/ssms.css";
import Modal from "./Modal.vue";
import { format } from "sql-formatter";

export default {
  name: "SqlMain",
  components: {
    Modal,
  },
  props: {
    msg: String,
  },
  data() {
    return {
      server: "",
      serverList: [],
      serverFilter: "",
      serverListFiltered: [],
      database: "",
      databaseList: [],
      databaseFilter: "",
      databaseListFiltered: [],
      serverobjectlist: [],
      objectTypeList: [],
      objectType: "",
      currentObjectList: [],
      selectedObjectName: "",
      selectedSechemaName: "",
      objectText: "",
      objFilter: "",
      errorMessage: "",
      newServerName: "",
      objectTblData: "",
      sqlQuery: "",
      sqlQueryResponse: "",
      cmEditor1: null,
      formattedSqlQuery: "",
      spSearchText: "",
      spSearchResponse: [],
      searchSP: "false",
      searchTable: "false",
      searchColumn: "false",
      searchView: "false",
      searchSynonym: "false",
      searchAllDbs: "false",
      searchAllServers: "false",
      progressFlag: true,
      diffLeft: "",
      diffRight: "",
      diffMessage: "",
      leftIsFileOnDisk: "false",
      rightIsFileOnDisk: "false",
      objDependent: "",
      objDependentResponse: "",
      regexDependent: "(?i)(EXEC|SP_EXECUTESQL)((\\s)+[!-~]+){1,3}",
      highlightedText: "",
      showModalhighlightedText: false,
      showModal: false,
      modalContent: "",
      codeToFormat: "",
    };
  },
  methods: {
    
    clearServerFilter() {
      this.serverFilter = "";     
      this.doServerListFiltered(); 
    },

    clearDatabaseFilter() {
      this.databaseFilter = "";  
      this.doDatabaseListFiltered();
    },

    formatSql() {
      console.log("formatSql button clicked");
      this.cmEditor1.setValue(format(this.cmEditor1.getValue(), { language: "tsql" }));
    },

    formatColumns() {
      console.log("formatColumns button clicked");
      this.codeToFormat = this.formatSqlColumns(this.codeToFormat, true, 120);
    },

    formatSqlColumns(columnString, removeSquareBrackets = false, maxLineLength = 120) {
      const columns = columnString.split(",");
      let formattedString = "";
      let currentLine = "";

      columns.forEach((column, index) => {
        column = column.trim();
        if (removeSquareBrackets) {
          column = column.replace(/\[|\]/g, "");
        }
        // Add a comma back except for the last column
        if (index < columns.length - 1) {
          column += ",";
        }
        if (currentLine.length + column.length + 1 > maxLineLength) {
          // Append the current line to the formatted string and start a new line
          formattedString += currentLine + "\n";
          currentLine = column;
        } else {
          // Add a space before the column if it's not the start of a new line
          if (currentLine.length > 0) {
            currentLine += " ";
          }
          currentLine += column;
        }
      });

      // Add the last line to the formatted string
      formattedString += currentLine;

      return formattedString;
    },

    fnGetTableColumns: function () {
      this.progressFlag = true;

      axios
        .post("/GetTableColumns?tableName=" + this.selectedObjectName, {
          server: this.server,
          database: this.database,
        })
        .then((response) => {
          this.codeToFormat = response.data;
          this.progressFlag = false;
        })
        .catch((error) => {
          console.log(error);
          this.codeToFormat = error;
          this.progressFlag = false;
        });
    },

    doServerListFiltered: function () {
      console.log("doServerListFiltered: " + this.serverFilter);

      if (this.serverFilter.trim().length < 1) {
        this.serverListFiltered = this.serverList;
        return;
      }

      this.serverListFiltered = this.serverList.filter((server) => {
        return server.toLowerCase().indexOf(this.serverFilter.trim().toLowerCase()) > -1;
      });
    },

    doDatabaseListFiltered: function () {
      console.log("doDatabaseListFiltered: " + this.databaseFilter);
      if (this.databaseFilter.trim().length < 1) {
        this.databaseListFiltered = this.databaseList;
        return;
      }

      this.databaseListFiltered = this.databaseList.filter((database) => {
        return database.toLowerCase().indexOf(this.databaseFilter.trim().toLowerCase()) > -1;
      });
    },

    resetObjects: function () {
      this.errorMessage = "";
      this.databaseList = [];
      this.databaseListFiltered = [];
      this.database = "";
      this.selectedSechemaName = "";
      this.selectedObjectName = "";
      this.objectType = "objectType";
      this.objectTypeList = [];
      this.currentObjectList = [];
    },

    fngetHighlightedText: function () {
      if (window.getSelection) {
        this.highlightedText = window.getSelection().toString();
      } else if (document.selection && document.selection.type != "Control") {
        this.highlightedText = document.selection.createRange().text;
      }
      console.log("highlightedText: " + this.highlightedText);
      this.fnHiLiteSearch();
    },

    fnLeftDiff: function () {
      this.diffLeft = this.server + "." + this.database + "." + this.selectedSechemaName + "." + this.selectedObjectName;
    },

    fnRightDiff: function () {
      this.diffRight = this.server + "." + this.database + "." + this.selectedSechemaName + "." + this.selectedObjectName;
    },

    fnDiffCompare: function () {
      this.progressFlag = true;

      axios
        .post("/diff?objectType=" + this.objectType + "&diffLeft=" + this.diffLeft + "&diffRight=" + this.diffRight + "&leftIsFileOnDisk=" + this.leftIsFileOnDisk + "&rightIsFileOnDisk=" + this.rightIsFileOnDisk, { server: this.server, database: this.database })
        .then((response) => {
          this.diffMessage = response.data;
          this.progressFlag = false;
        })
        .catch((error) => {
          console.log(error);
          this.diffMessage = error;
          this.progressFlag = false;
        });
    },

    selectElementById: function (elementId) {
      const myElement = document.getElementById(elementId);
      myElement.focus();
      myElement.select();
    },

    updateClip: function (idclipbrd) {
      //this.progressFlag = !this.progressFlag;

      let clipElem = document.querySelector(idclipbrd); //'#idclipbrd'
      clipElem.setAttribute("type", "text");
      clipElem.select();

      try {
        var successful = document.execCommand("copy");
        var msg = successful ? "successful" : "unsuccessful";
        console.log("Testing code was copied " + msg);
      } catch (err) {
        console.log("Oops, unable to copy");
      }

      /* unselect the range */
      clipElem.setAttribute("type", "hidden");
      window.getSelection().removeAllRanges();
    },

    // toggleProgress: function() {
    //     //this.progressFlag = !this.progressFlag;
    //     this.toggleProgress2();
    // },

    fnSpSearch: function () {
      this.progressFlag = true;
      this.spSearchResponse = [];
      //?sqlquery=' + this.sqlQuery, { 'server': this.server, 'database': this.database })
      axios
        .post(
          "/querySysObjects?spSearchText=" +
            this.spSearchText +
            "&searchSP=" +
            this.searchSP +
            "&searchTable=" +
            this.searchTable +
            "&searchColumn=" +
            this.searchColumn +
            "&searchView=" +
            this.searchView +
            "&searchSynonym=" +
            this.searchSynonym +
            "&searchAllDbs=" +
            this.searchAllDbs +
            "&searchAllServers=" +
            this.searchAllServers,
          { server: this.server, database: this.database }
        )
        .then((response) => {
          this.spSearchResponse = response.data;
          this.progressFlag = false;
        })
        .catch(function (error) {
          console.log(error);
          this.progressFlag = false;
        });
    },

    fnDependentFillName: function () {
      if (this.selectedObjectName.length > 0) {
        this.objDependent = this.selectedObjectName;
        this.objDependentResponse = "";
      } else {
        console.log("fnDependentFillName: selectedObjectName is empty");
      }
    },

    fnHiLiteSearch: function () {
      if (this.highlightedText.length < 1) {
        console.log("fnHiLiteSearch: highlightedText is empty");
        return;
      }
      this.progressFlag = true;
      this.highlightedText = this.highlightedText.trim();
      this.modalContent = "";

      axios
        .post("/HiLiteSearch?objName=" + this.highlightedText, {
          server: this.server,
          database: this.database,
        })
        .then((response) => {
          this.modalContent = response.data;
          this.progressFlag = false;
          this.showModal = true;
        })
        .catch(function (error) {
          console.log(error);
          this.modalContent = error;
          this.progressFlag = false;
          this.showModal = true;
        });
    },

    fnDependentSearch: function () {
      if (this.objDependent.length < 1) {
        console.log("fnDependentSearch: objDependent is empty");
        return;
      }
      this.progressFlag = true;
      this.objDependentResponse = "";

      console.log("objDependent: " + this.objDependent + " regexDependent: " + this.regexDependent);

      var lclRegexDependent = this.EncodeToBase58(this.regexDependent);

      axios
        .post("/DependentSearch?objDependentName=" + this.objDependent + "&regexDependent=" + lclRegexDependent, { server: this.server, database: this.database })
        .then((response) => {
          this.objDependentResponse = response.data;
          this.progressFlag = false;
        })
        .catch(function (error) {
          console.log(error);
          this.objDependentResponse = error;
          this.progressFlag = false;
        });
    },

    filterCurrentObjects: function (event) {
      this.objFilter = event.target.value;
      console.log("objFilter: " + this.objFilter);

      if (this.objFilter.length < 1) {
        this.objectTypeReset(this.objectType);
        return;
      }

      this.objectTypeReset(this.objectType);

      if (this.objFilter.indexOf("|") > -1) {
        var pipeAry = this.objFilter.split("|");
        var pipeList = [];

        for (var i = 0; i < pipeAry.length; i++) {
          var pipeElem = pipeAry[i];

          if (pipeElem.length > 0) {
            console.log("pipe filter: " + pipeElem);

            var self = this.currentObjectList;
            var tempPipeAry = self.filter((obj) => {
              return obj.name.toLowerCase().indexOf(pipeElem.toLowerCase()) > -1;
            });

            pipeList = pipeList.concat(tempPipeAry);
            tempPipeAry = [];
          }
        }

        if (pipeList.length > 0) {
          var uniqPipeList = [...new Set(pipeList)];
          this.currentObjectList = uniqPipeList;
        }
      } else {
        var filterAry = this.objFilter.split("*");

        for (var i = 0; i < filterAry.length; i++) {
          var curFilter = filterAry[i];

          if (curFilter.length > 0) {
            console.log("star ** filter: " + curFilter);

            var self = this.currentObjectList;
            this.currentObjectList = self.filter((obj) => {
              return obj.name.toLowerCase().indexOf(curFilter.toLowerCase()) > -1;
            });
          }
        }
      }

      // var self = this.currentObjectList;
      // this.currentObjectList = self.filter((obj) => {
      //     return obj.name.toLowerCase().indexOf(this.objFilter.toLowerCase()) > -1
      // });

      this.objectText = "";
    },

    getServerNameList: function () {
      this.progressFlag = true;
      this.server = "";
      this.serverList = [];

      this.resetObjects();
      this.databaseList = [];
      this.databaseListFiltered = [];

      axios
        .post("/servernamelist")
        .then((response) => {
          console.log("/servernamelist received by client", response.data);
          this.serverList = response.data;
          this.serverListFiltered = this.serverList;
          this.progressFlag = false;
        })
        .catch(function (error) {
          console.log(error);
          this.progressFlag = false;
        });
    },

    serverSelected: function (event) {
      this.resetObjects();
      //console.log(this.server);
      //console.log('event.target.value: ' + event.target.value);
      this.getDBList();
    },

    getDBList: function () {
      this.progressFlag = true;
      this.objectText = "";

      axios
        .post("/databaselist", { server: this.server })
        .then((response) => {
          this.databaseList = response.data;
          this.databaseListFiltered = this.databaseList;
          this.progressFlag = false;
        })
        .catch((error) => {
          //console.log(error);
          console.log(JSON.stringify(error));
          this.errorMessage = error;
          this.progressFlag = false;
        });
    },

    objectTypeReset: function (objectTypeValue) {
      this.objectText = "";
      //console.log('objectTypeReset issues');
      this.objectType = objectTypeValue;
      var self = this.serverobjectlist;
      this.currentObjectList = self.filter((obj) => {
        return obj.type_desc === this.objectType;
      })[0].objects;
      //$cookies.set('objectType', this.objectType);
    },

    objectTypeChange: function (event) {
      this.objectTypeReset(event.target.value);
    },

    selectSPType: function () {
      console.log("select stored proc type button clicked");

      for (var i = 0; i < this.objectTypeList.length; i++) {
        //console.log(this.objectTypeList[i]);
        if (this.objectTypeList[i] === "SQL_STORED_PROCEDURE") {
          this.objectType = this.objectTypeList[i];
          console.log("SQL_STORED_PROCEDURE is set as objectType");
          this.objectTypeReset(this.objectType);
          break;
        }
      }
    },

    selectTableType: function () {
      console.log("select table type button clicked");

      for (var i = 0; i < this.objectTypeList.length; i++) {
        //console.log(this.objectTypeList[i]);
        if (this.objectTypeList[i] === "USER_TABLE") {
          this.objectType = this.objectTypeList[i];
          console.log("USER_TABLE is set as objectType");
          this.objectTypeReset(this.objectType);
          break;
        }
      }
    },

    openInNotepadd: function () {
      if (this.selectedObjectName.length < 3) {
        console.log("openInNotepadd: selectedObjectName is empty");
        return;
      }
      console.log("openInNotepadd button clicked");
      axios
        .post("/openInNotepadd?sch=" + this.selectedSechemaName + "&obj=" + this.selectedObjectName, { server: this.server, database: this.database })
        .then((response) => {
          console.log(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    },

    addServerName: function () {
      console.log("New SERVER: " + this.newServerName);
      if (this.newServerName.trim().length < 2) {
        console.log("New SERVER is less than 2 chars: " + this.newServerName);
        return;
      }

      this.progressFlag = true;
      axios
        .post("/updateservernamelist?addservername=" + this.newServerName)
        .then((response) => {
          this.server = "";
          this.serverList = [];
          this.resetObjects();
          this.databaseList = [];
          this.databaseListFiltered = [];
          this.serverList = response.data;
          this.serverListFiltered = this.serverList;

          this.newServerName = "";
          this.progressFlag = false;
        })
        .catch(function (error) {
          console.log(error);
          this.progressFlag = false;
        });
    },

    getServerObjectList: function () {
      this.progressFlag = true;
      //this.getServerNameList();

      axios
        .post("/serverobjectlist", {
          server: this.server,
          database: this.database,
        })
        .then((response) => {
          this.serverobjectlist = response.data;
          var otl = [];
          this.serverobjectlist.forEach(function (objList) {
            otl.push(objList.type_desc);
            //console.log(objList.type_desc);
          });

          this.objectTypeList = otl;
          //this.objectType = otl[0];
          //console.log(otl);
          //this.objectTypeChange();
          this.progressFlag = false;
        })
        .catch(function (error) {
          console.log(error);
          this.progressFlag = false;
        });
    },

    dbSelected: function (event) {
      this.objectText = "";
      this.selectedObjectName = "";
      this.selectedSechemaName = "";
      this.currentObjectList = [];
      this.getServerObjectList();
    },

    loadObjectText: function () {
      this.progressFlag = true;

      axios
        .post("/objtext?sch=" + this.selectedSechemaName + "&obj=" + this.selectedObjectName, { server: this.server, database: this.database })
        .then((response) => {
          this.objectText = response.data;
        })
        .catch(function (error) {
          console.log(error);
        });

      axios
        .post("/tbldata?sch=" + this.selectedSechemaName + "&obj=" + this.selectedObjectName, { server: this.server, database: this.database })
        .then((response) => {
          this.objectTblData = response.data;
          this.progressFlag = false;
        })
        .catch(function (error) {
          console.log(error);
          this.progressFlag = false;
        });
    },

    loadObjectTextUsingParams: function (selectedSechemaName, selectedObjectName, server, database) {
      this.progressFlag = true;
      axios
        .post("/objtext?sch=" + selectedSechemaName + "&obj=" + selectedObjectName, { server: server, database: database })
        .then((response) => {
          this.objectText = response.data;
        })
        .catch(function (error) {
          console.log(error);
        });

      axios
        .post("/tbldata?sch=" + selectedSechemaName + "&obj=" + selectedObjectName, { server: server, database: database })
        .then((response) => {
          this.objectTblData = response.data;
          //this.progressFlag = false;
        })
        .catch(function (error) {
          console.log(error);
          this.progressFlag = false;
        });

      if (this.selectedObjectName.length > 0) {
        console.log("will run Dependent object search");
        this.objDependent = this.selectedObjectName;
        this.objDependentResponse = "";

        console.log("objDependent: " + this.objDependent + " regexDependent: " + this.regexDependent);

        var lclRegexDependent = this.EncodeToBase58(this.regexDependent);

        axios
          .post("/DependentSearch?objDependentName=" + this.objDependent + "&regexDependent=" + encodeURIComponent(lclRegexDependent), { server: this.server, database: this.database })
          .then((response) => {
            this.objDependentResponse = response.data;
          })
          .catch(function (error) {
            console.log(error);
            this.objDependentResponse = error;
          });
      } else {
        console.log("will not run Dependent object search");
      }

      if (this.progressFlag) {
        this.progressFlag = false;
      }
    },
    DecodeFromBase58: function (input) {
      const bytes = bs58.decode(input);
      return bytes.toString("utf8");
    },
    EncodeToBase58: function (input) {
      const bytes = Buffer.from(input, "utf8");
      return bs58.encode(bytes);
    },
    QueryDatabase: function () {
      this.sqlQuery = this.cmEditor1.getValue();
      console.log("sqlQuery plane");
      console.log(this.sqlQuery);

      var lclSqlQuery = this.EncodeToBase58(this.sqlQuery);
      console.log("lclSqlQuery encoded");
      console.log(lclSqlQuery);
      this.progressFlag = true;

      this.sqlQueryResponse = "";
      axios
        .post("/querydatabase?sqlquery=" + encodeURIComponent(lclSqlQuery), {
          server: this.server,
          database: this.database,
        })
        .then((response) => {
          this.sqlQueryResponse = response.data;
          this.progressFlag = false;
        })
        .catch(function (error) {
          console.log(error);
          this.progressFlag = false;
        });
    },

    linkClick: function (event) {
      var objname = event.target.textContent;
      //console.log('linkClick-event.target.value: ' + objname);
      this.selectedSechemaName = objname.split(".")[0];
      this.selectedObjectName = objname.split(".")[1];
      this.loadObjectText();
    },

    linkClick2: function (obj) {
      console.log("linkClick2 obj is:" + obj);
      var objname = obj.replace("#!/", "");

      this.selectedSechemaName = objname.split(".")[0];
      this.selectedObjectName = objname.split(".")[1];
      this.loadObjectText();
    },
    linkClick3: function (p_objectType, p_server, p_database, p_dbschema, p_dbobject) {
      this.selectedSechemaName = p_dbschema;
      this.selectedObjectName = p_dbobject;

      //this.loadObjectText();
      this.loadObjectTextUsingParams(p_dbschema, p_dbobject, p_server, p_database);
    },
  },

  mounted: function () {
    this.$nextTick(function () {
      this.getServerNameList();
    });

    let self = this;
    this.$refs["refTblData"].addEventListener("click", function (event) {
      //console.log('clicked: ', event.target);
      if (event.target.tagName.toLowerCase() === "a") {
        if (event.target.getAttribute("data-val")) {
          self.linkClick2(event.target.getAttribute("href"));
        }
      }

      event.preventDefault();
    });

    this._keyListener = function (e) {
      console.log(e.key);

      if ((e.key === "s" || e.key === "S") && e.ctrlKey && e.altKey) {
        e.preventDefault();
        // present "Save Page" from getting triggered.
        console.log("shortkey pressed: Ctrl + Shift + v");
        this.selectSPType();
        this.selectElementById("input_box_obj_filter");
      } else if ((e.key === "t" || e.key === "T") && e.ctrlKey && e.altKey) {
        e.preventDefault();
        // present "Save Page" from getting triggered.
        console.log("shortkey pressed: Ctrl + Alt + T");
        this.selectTableType();
        this.selectElementById("input_box_obj_filter");
      } else if (e.key === "0" && e.ctrlKey && e.altKey) {
        e.preventDefault();

        console.log("shortkey pressed: Ctrl + Alt + 0");
        this.objFilter = "";
        this.selectElementById("input_box_obj_filter");
      } else if ((e.key === "c" || e.key === "C") && e.ctrlKey && e.altKey) {
        e.preventDefault();
        //updateClip
        console.log("shortkey pressed: Ctrl + Alt + c");
        this.updateClip();
      }
    };

    document.addEventListener("keydown", this._keyListener.bind(this));

    this.cmEditor1 = CodeMirror.fromTextArea(this.$refs.sqleditor1, {
      mode: "text/x-mssql",
      theme: "ssms",
      lineNumbers: true,
      readOnly: false,
    });

    this.cmEditor1.setSize("100%", "100%");    
    this.cmEditor1.refresh();

    // resize start

    const resizer = document.querySelector(".resizer");
    const leftPane = document.querySelector(".flx-left");
    const rightPane = document.querySelector(".flx-right");

    let mousedown = false;

    resizer.addEventListener("mousedown", () => {
      mousedown = true;
    });

    window.addEventListener("mousemove", (e) => {
      if (!mousedown) return;

      e.preventDefault();

      // let x = e.pageX - leftPane.offsetLeft;
      // let y = e.pageY - leftPane.offsetTop;

      // let leftWidth = x;
      // let rightWidth = window.innerWidth - leftWidth;

      // leftPane.style.width = `${leftWidth}px`;
      // rightPane.style.width = `${rightWidth}px`;

      let x = e.pageX - leftPane.getBoundingClientRect().left; // get the x position of the cursor relative to the left pane
      let totalWidthVw = window.innerWidth / 100; // calculate the width of 1vw
      let leftWidthVw = x / totalWidthVw; // calculate the width of the left pane in vw
      let rightWidthVw = 100 - leftWidthVw; // calculate the width of the right pane in vw

      leftPane.style.width = `${leftWidthVw}vw`;
      rightPane.style.width = `${rightWidthVw}vw`;
    });

    window.addEventListener("mouseup", () => {
      mousedown = false;
    });

    //resize end
  },
  beforeDestroy() {
    document.removeEventListener("keydown", this._keyListener);
  },
  watch: {
    $route(to, from, next) {
      console.log("in watch");
      console.log(to.params.dbschema);
      console.log(to.params.dbobject);

      this.linkClick3(to.params.objectType, to.params.server, to.params.database, to.params.dbschema, to.params.dbobject);
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
body {
  height: 100vh;
  margin: 0;
}
.CodeMirror {
    resize: both; /* This enables resizing both horizontally and vertically */
    overflow: auto; /* Adds scrollbars when content overflows */
}
.flx-body {
  display: flex;
  flex-direction: column;
  margin: 0;
}
.flx-header {
  flex: 0 0 auto;
  background: #f8f8f8;
  padding: 4px;
  height: 10vh;
}
.flx-footer {
  flex: 0 0 auto;
  background: #f8f8f8;
  padding: 4px;
  height: 5vh;
}

.flx-content {
  flex: 1 1 auto;
  display: flex;
  flex-direction: row;
  background: #ffffff;
  height: 88vh;
}
.flx-pane {
  padding: 10px;
  border: 1px solid #ced4da;
}
.flx-left {
  margin-right: 3px;
  /* resize: horizontal;						 */
  overflow: auto;
  overscroll-behavior: contain;
  white-space: nowrap;
  width: 25vw;
}
.flx-right {
  margin-left: 3px;
  flex-grow: 1;

  overflow: auto;
  overscroll-behavior: contain;
  white-space: nowrap;
  width: 75vw;
}

.resizer {
  width: 4px;
  background: #fff; /*#a9a9a9; */
  cursor: col-resize;
}
/* 
header {
    position: fixed;
    top: 0;
    
    position: sticky;
    width: 100%;
    background-color: #F8F8F8;   
    padding: 10px;    
     z-index: 1000;
  }  */

/* main {
    margin: 6px;
   
  } */

/* #SqlMain {
    margin-left: 10px;
    margin-right: 10px;

   margin: 1px; 
} 
*/
/* 
#objectNavigator {
    float: left;
    width: 25%;   
    background: #F8F8F8;
    padding: 10px;
} */

/* #textCtrl {
    float: left;
    padding: 10px;
    width: 72%;  
    background-color: white;
} */

#connectionString {
  color: purple;
  font-size: small;
}

#currentObjName {
  color: red;
  font-size: small;
}

section:after {
  content: "";
  display: table;
  clear: both;
}

#objectList {
  width: 250px;
  list-style-type: none;
  padding: 0;
  padding-left: 3px;
}

#objectTypeComboBox,
#objFilter,
#database,
#server {
  width: 245px;
  float: left;
  margin-left: 3px;
  margin-right: 3px;
}

#tbldata {
  font-family: monospace;
  font-size: small;
}

@media (max-width: 600px) {
  #objectNavigator,
  #textCtrl {
    width: 100%;
    height: auto;
  }
}

.is-hidden {
  display: none;
}

.overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.6);
}

.modal-content {
  padding: 20px 30px;
  width: 400px;
  position: relative;
  min-height: 150px;
  margin: 5% auto 0;
  background: #fff;
}

.modal-content label,
.modal-content input,
.modal-content select {
  display: inline-block;
}

.inline {
  display: inline-block;
}

.modal-content label {
  width: 20%;
  text-align: right;
}

.modal-content label + input {
  width: 70%;
  margin-left: 4%;
}

.modal-content label + select {
  width: 70%;
  margin-left: 4%;
  height: 21px;
}

.modal-content button {
  float: right;
  margin-right: 5%;
  width: 20%;
}

#errorMessage {
  color: red;
  display: inline-block;
  margin-top: 10px;
}

.modal-content #errorMessage {
  color: red;
  display: inline-block;
  margin-top: 10px;
}
</style>
