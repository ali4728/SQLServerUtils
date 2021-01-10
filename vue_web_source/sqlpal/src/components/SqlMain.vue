<template>


                        
            <div id="SqlMain">
                
                <div>
                    <table>
                        <tr>
                            <td style="padding-left: 12px;">
                                <!-- Server: {{server}} -->
                                 <button @click="selectTableType" type="button" class="btn btn-secondary btn-sm">tbl</button> &nbsp;
                                 <button @click="selectSPType" type="button" class="btn btn-secondary btn-sm">sp</button> &nbsp;
                                 <button @click.stop.prevent="updateClip" type="button" class="btn btn-secondary btn-sm">cb</button>
                                  <input type="hidden" id="idclipbrd" :value=" '[' + this.server + '].' + this.database + '.' + this.selectedSechemaName + '.' + this.selectedObjectName">
                            </td>
                            <td style="padding-left: 12px;">                        
                                 <div v-show="progressFlag" class="spinner-border text-primary spinner-border-sm" role="status">
                                    <span class="sr-only">Loading...</span>
                                </div>
                            </td>
                            <td style="padding-left: 12px;" ref="fullObjectName">
                               <!-- Object: {{selectedSechemaName}}.{{selectedObjectName}} -->
                                [{{server}}].{{database}}.{{selectedSechemaName}}.{{selectedObjectName}}

                            </td>
                        </tr>
                        <tr>
                            
                            <td style="padding-left: 12px;">
                                <div class="input-group input-group-sm mb-3">
                                     <button @click="addServerName" type="button" class="btn btn-primary btn-sm">Add Server</button>
                                     &nbsp;                           
                                    <input  v-model="newServerName" type="text" class="form-control">
                                </div>                            
                            </td>
                    
                            <td style="padding-left: 12px;">
                                <div class="input-group input-group-sm mb-3">
                                    <span class="input-group-text">ServerList</span>
                                    <select class="form-select"  v-model="server" @change="serverSelected($event)">
                                        <option disabled value="">Please select server</option>
                                         <option v-for="srvr in serverList" v-bind:value="srvr">{{srvr}}</option>
                                    </select>                                    
                                </div>
                            </td>
                   
                            <td style="padding-left: 12px;">
                                <div class="input-group input-group-sm mb-3">
                                    <span class="input-group-text">Database</span>
                                    <select class="form-select" v-model="database" @change="dbSelected($event)">
                                        <option disabled value="">Please select database</option>
                                        <option v-for="db in databaseList" v-bind:value="db">{{db}}</option>
                                    </select>
                                </div>

                            </td>
                        </tr>
                    </table>

                    <div id="errorMessage">{{errorMessage}}</div>
                </div>


                <div>
                    <b-tabs content-class="mt-3">
                        <b-tab title="Object Browser" active>


                                <section>
                                    <div id="objectNavigator">
                                        <table>
                                            <tr>

                                                <!-- <td style="width: 80px;"> <label for="objectTypeComboBox"><small>Object type</small></label></td>
                                                <td style="width: 50px;"> <b-link @click="selectSPType" href="#">sp</b-link>&nbsp;<b-link @click="selectTableType" href="#">tb</b-link></td>
                                                <td>

                                                    <select class="form-select" v-model="objectType" @change="objectTypeChange($event)">
                                                        <option disabled value="">-----Please select object type-----</option>
                                                        <option v-for="ot in objectTypeList" v-bind:key="ot">
                                                            {{ot}}
                                                        </option>
                                                    </select>
                                                </td> -->

                                                <td colspan="3">
                                                    <div class="input-group mb-3">
                                                        <span class="input-group-text">Object Type</span>

                                                        <select class="form-select" v-model="objectType" @change="objectTypeChange($event)">
                                                            <option disabled value="">-----Please select object type-----</option>
                                                            <option v-for="ot in objectTypeList" v-bind:key="ot">
                                                                {{ot}}
                                                            </option>
                                                        </select>
                                                        
                                                    </div>
                                                </td>

                                            </tr>
                                            <tr>

                                                <!-- <td style="width: 80px;"> <label for="objFilter"><small>Object filter</small></label></td>
                                                <td style="width: 50px;"></td>
                                                <td> -->
                                                    <!-- <input v-model="objFilter" v-on:keyup="filterCurrentObjects"> -->
                                                     <!-- <b-form-input  v-model="objFilter" v-on:keyup="filterCurrentObjects"></b-form-input> -->
                                                <!-- </td> -->

                                                <td colspan="3">
                                                    <div class="input-group mb-3">
                                                        <span class="input-group-text">Object Filter</span>
                                                        <input type="text" class="form-control" v-model="objFilter" v-on:keyup="filterCurrentObjects">
                                                    </div>
                                                </td>
                                                


                                            </tr>
                                        </table>

                                        <div id="objectListContainer">
                                            <ol id="objectList">
                                                <li v-for="object in currentObjectList" v-bind:key="object">
                                                    <!-- <a v-on:click.prevent="linkClick" v-bind:href="'#!/' + object.schema_name + '.' + object.name">{{object.schema_name}}.{{object.name}}</a> -->
                                                    <router-link :to="{ name: 'SqlMainId', params: {objectType: objectType, server: server, database: database, dbschema: object.schema_name, dbobject: object.name }}">{{object.schema_name}}.{{object.name}}</router-link>
                                                </li>
                                            </ol>
                                        </div>
                                    </div>

                                    <div id="textCtrl">
                                        <pre ref="refTblData" class="code" v-html="objectText"></pre>
                                        <br /><br />
                                        <pre id="text" v-html="objectTblData"></pre>
                                    </div>
                                </section>
            
                        </b-tab>

                        <b-tab title="SQL Editor">
                            <span>Enter AdHoc SQL Query Below:</span>

                            <br />
                            <textarea rows="10" cols="80" v-model="sqlQuery" placeholder="type sql command here"></textarea>
                            <br />
                            <button @click="QueryDatabase">Execute</button>
                            
                            <pre id="adhocsql" v-html="sqlQueryResponse"></pre>
                        </b-tab>
                        
                        <b-tab title="Search sys objects">
                            
                            <table>
                                <tr>
                                    <td style="width: 75px;"><div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchSP" true-value="true" false-value="false"> <label class="form-check-label" >SP</label></div></td>
                                    <td style="width: 75px;"><div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchTable" true-value="true" false-value="false"> <label class="form-check-label" >Tbl</label></div></td>
                                    <td style="width: 75px;"><div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchColumn" true-value="true" false-value="false"><label class="form-check-label" >Col</label></div></td>
                                    <td style="width: 75px;"><div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchView" true-value="true" false-value="false"><label class="form-check-label" >View</label></div></td>
                                    <td style="width: 120px;"><div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchAllDbs" true-value="true" false-value="false"><label class="form-check-label" >All Dbs</label></div></td>
                                    <td style="width: 120px;"><div class="form-check"><input class="form-check-input" type="checkbox" v-model="searchAllServers" true-value="true" false-value="false"><label class="form-check-label" >All Servers</label></div></td>
                                </tr>
                            </table>   
                        
                            <!-- <textarea rows="10" cols="80" v-model="spSearchText" placeholder="type text here to search"></textarea> -->
                            <div class="form-floating">
                                <textarea class="form-control" v-model="spSearchText" placeholder="type text here to search" style="height: 100px"></textarea>
                                <label>type text here to search</label>
                            </div>

                            <br />
                            <button @click="fnSpSearch">Search</button>                            

                                <div id="spSearchResponseId">
                                    <ul>
                                        <li v-for="object in spSearchResponse" v-bind:key="object">
                                            {{object}}
                                        </li>
                                    </ul>
                                </div>


                        </b-tab>
                        


                        <b-tab title="Diff">                            

                            
                           
                            <table>
                                <tr>
                                    <td>
                                        <div style="margin-bottom: 10px;">                                            
                                            <button @click="fnLeftDiff" type="button" class="btn btn-secondary">&lt;&lt;</button>
                                                &nbsp; 
                                            <button @click="fnRightDiff" type="button" class="btn btn-secondary">&gt;&gt;</button>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <div class="input-group mb-3">        
                                             <span class="input-group-text">Left</span>                            
                                            <input type="text" class="form-control" v-model="diffLeft" size="140">
                                        </div>                         
                                    </td>
                                </tr>

                                <tr>
                                    <td>
                                        <div class="input-group mb-3">    
                                             <span class="input-group-text">Right</span>                                    
                                            <input type="text" class="form-control" v-model="diffRight" size="140">
                                        </div>                                                                 
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <button @click="fnDiffCompare" type="button" class="btn btn-primary ">Compare</button>
                                    </td>
                                </tr>  
                                <tr>
                                    <td>
                                        {{diffMessage}}
                                    </td>
                                </tr>                                
                            </table>

                        </b-tab>




                    </b-tabs>
                </div>





            
            
            
            
            </div>





</template>

<script>
import axios from 'axios';

export default {
  name: 'SqlMain',
  props: {
    msg: String
  },
  data() {
    return {
      server: '',
      serverList: [],
      database: '',
      databaseList: [],
      serverobjectlist: [],
      objectTypeList: [],
      objectType: '',
      currentObjectList: [],
      selectedObjectName: '',
      selectedSechemaName: '',
      objectText: '',
      objFilter: '',
      errorMessage: '',
      newServerName: '',
      objectTblData: '',
      sqlQuery: '',
      sqlQueryResponse: '',
      spSearchText: '',
      spSearchResponse: [],
      searchSP: 'false',
      searchTable: 'false',
      searchColumn: 'false',
      searchView: 'false',
      searchAllDbs: 'false',
      searchAllServers: 'false',      
      progressFlag: true,
      diffLeft: '',  
      diffRight: '',
      diffMessage: '',

    }
  },
    methods: {

        resetObjects: function () {
            this.errorMessage = '';
            this.databaseList = [];
            this.database = '';
            this.selectedSechemaName = '';
            this.selectedObjectName = '';
            this.objectType = 'objectType';
            this.objectTypeList = [];
            this.currentObjectList = [];
        },

        fnLeftDiff: function() {            
            this.diffLeft = this.server + '.' + this.database + '.' + this.selectedSechemaName + '.' +  this.selectedObjectName;
        },

        fnRightDiff: function() {
            this.diffRight = this.server + '.' + this.database + '.' + this.selectedSechemaName + '.' +  this.selectedObjectName;
        },


        fnDiffCompare: function() {
            this.progressFlag = true;
            
            axios.post('/diff?diffLeft=' + this.objectType + '.' + this.diffLeft + '&diffRight=' + this.objectType + '.'  + this.diffRight, { 'server': this.server, 'database': this.database })
                .then((response) => {
                    this.diffMessage = response.data;
                    this.progressFlag = false;
                })
                .catch(function (error) {
                    console.log(error);
                    this.diffMessage = error;
                    this.progressFlag = false;
                });
        },


        updateClip: function () {  
            //this.progressFlag = !this.progressFlag;             
            
            let clipElem = document.querySelector('#idclipbrd')
            clipElem.setAttribute('type', 'text') 
            clipElem.select()

            try {
                var successful = document.execCommand('copy');
                var msg = successful ? 'successful' : 'unsuccessful';
                console.log('Testing code was copied ' + msg);
            } catch (err) {
                console.log('Oops, unable to copy');
            }

            /* unselect the range */
            clipElem.setAttribute('type', 'hidden')
            window.getSelection().removeAllRanges()


        },

        // toggleProgress: function() {
        //     //this.progressFlag = !this.progressFlag; 
        //     this.toggleProgress2();
        // },


        fnSpSearch: function() {  
            this.progressFlag = true;
            this.spSearchResponse = [];
            //?sqlquery=' + this.sqlQuery, { 'server': this.server, 'database': this.database })
            axios.post('/querySysObjects?spSearchText=' + this.spSearchText + '&searchSP=' + this.searchSP + '&searchTable=' + this.searchTable + '&searchColumn=' + this.searchColumn + '&searchView=' + this.searchView + '&searchAllDbs=' + this.searchAllDbs + '&searchAllServers=' + this.searchAllServers, { 'server': this.server, 'database': this.database })
                .then((response) => {
                    this.spSearchResponse = response.data;
                    this.progressFlag = false;
                })
                .catch(function (error) {
                    console.log(error);
                    this.progressFlag = false;
                });

            

        },

        filterCurrentObjects: function (event) {
            this.objFilter = event.target.value;
            console.log('objFilter: ' + this.objFilter);

            if (this.objFilter.length < 1) {
                this.objectTypeReset(this.objectType);
                return;
            }

            this.objectTypeReset(this.objectType);

            if(this.objFilter.indexOf("|") > -1) {
                
                var pipeAry = this.objFilter.split("|");
                var pipeList = [];

                for (var i = 0; i < pipeAry.length; i++) {
                    
                    var pipeElem = pipeAry[i];

                    if(pipeElem.length > 0) {
                        console.log('pipe filter: ' + pipeElem);

                        var self = this.currentObjectList; 
                        var tempPipeAry  = self.filter((obj) => {
                        return obj.name.toLowerCase().indexOf(pipeElem.toLowerCase()) > -1
                        });

                        pipeList = pipeList.concat(tempPipeAry);
                        tempPipeAry = [];
                    }

                }

                if(pipeList.length >0 ) {
                    var uniqPipeList = [...new Set(pipeList)];
                    this.currentObjectList = uniqPipeList;
                }

            } else {
                var filterAry = this.objFilter.split("*");

                for (var i = 0; i < filterAry.length; i++) {
                    var curFilter = filterAry[i];
                    
                    if(curFilter.length > 0) {
                        console.log('star ** filter: ' + curFilter);

                        var self = this.currentObjectList;            
                        this.currentObjectList = self.filter((obj) => {
                            return obj.name.toLowerCase().indexOf(curFilter.toLowerCase()) > -1
                        });

                    }

                }     
            }

            // var self = this.currentObjectList;            
            // this.currentObjectList = self.filter((obj) => {
            //     return obj.name.toLowerCase().indexOf(this.objFilter.toLowerCase()) > -1
            // });



            this.objectText = '';

        },

        getServerNameList: function () {
            this.progressFlag = true;
            this.server = '';
            this.serverList = [];
            
            this.resetObjects();
            this.databaseList = [];
            
            axios.post('/servernamelist')
                .then((response) => {
                    this.serverList = response.data;
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
            this.objectText = '';
            
            axios.post('/databaselist', { 'server': this.server })
                .then((response) => {                    
                    this.databaseList = response.data;
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
            this.objectText = '';
            //console.log('objectTypeReset issues');
            this.objectType = objectTypeValue;
            var self = this.serverobjectlist;
            this.currentObjectList = self.filter((obj) => {
                return obj.type_desc === this.objectType
            })[0].objects;
            //$cookies.set('objectType', this.objectType);
        },



        objectTypeChange: function (event) {
            this.objectTypeReset(event.target.value);
        },

        selectSPType: function() {
            console.log('select stored proc type button clicked');

            for (var i = 0; i < this.objectTypeList.length; i++) {
                //console.log(this.objectTypeList[i]);
                if(this.objectTypeList[i] === "SQL_STORED_PROCEDURE") {
                    this.objectType = this.objectTypeList[i];
                    console.log("SQL_STORED_PROCEDURE is set as objectType");
                    this.objectTypeReset(this.objectType);
                    break;
                }
                
            }            
        },

        selectTableType: function() {
            console.log('select table type button clicked');

            for (var i = 0; i < this.objectTypeList.length; i++) {
                //console.log(this.objectTypeList[i]);
                if(this.objectTypeList[i] === "USER_TABLE") {
                    this.objectType = this.objectTypeList[i];
                    console.log("USER_TABLE is set as objectType");
                    this.objectTypeReset(this.objectType);
                    break;
                }
                
            }
        },

        addServerName: function () {
            console.log('New SERVER: ' + this.newServerName);
            if (this.newServerName.trim().length < 2) {
                console.log('New SERVER is less than 2 chars: ' + this.newServerName);
                return;
            }

            this.progressFlag = true;
            axios.post('/updateservernamelist?addservername=' + this.newServerName)
                .then((response) => {

                    this.server = '';
                    this.serverList = [];
                    this.resetObjects();
                    this.databaseList = [];
                    this.serverList = response.data;

                    this.newServerName = '';
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

            axios.post('/serverobjectlist', { 'server': this.server, 'database': this.database })
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
            this.objectText = '';
            this.selectedObjectName = '';
            this.selectedSechemaName = '';
            this.currentObjectList = [];
            this.getServerObjectList();
        },

        loadObjectText: function () {

            this.progressFlag = true;

            axios.post('/objtext?sch=' + this.selectedSechemaName + '&obj=' + this.selectedObjectName, { 'server': this.server, 'database': this.database })
                .then((response) => {
                    this.objectText = response.data;                   
                })
                .catch(function (error) {
                    console.log(error);                   
                });
          
            axios.post('/tbldata?sch=' + this.selectedSechemaName + '&obj=' + this.selectedObjectName, { 'server': this.server, 'database': this.database })
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
            axios.post('/objtext?sch=' + selectedSechemaName + '&obj=' + selectedObjectName, { 'server': server, 'database': database })
                .then((response) => {
                    this.objectText = response.data;
                    
                })
                .catch(function (error) {
                    console.log(error);
                    
                });

           
            axios.post('/tbldata?sch=' + selectedSechemaName + '&obj=' + selectedObjectName, { 'server': server, 'database': database })
                .then((response) => {
                    this.objectTblData = response.data;
                    this.progressFlag = false;
                })
                .catch(function (error) {
                    console.log(error);
                    this.progressFlag = false;
                });

            


        },
        QueryDatabase: function () {

            this.progressFlag = true;

            this.sqlQueryResponse = '';
            axios.post('/querydatabase?sqlquery=' + this.sqlQuery, { 'server': this.server, 'database': this.database })
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
            this.selectedSechemaName = objname.split('.')[0];
            this.selectedObjectName = objname.split('.')[1];            
            this.loadObjectText();
        },

        linkClick2: function (obj) {
            console.log('linkClick2 obj is:' + obj);
            var objname = obj.replace('#!/', '');
            
            this.selectedSechemaName = objname.split('.')[0];
            this.selectedObjectName = objname.split('.')[1];
            this.loadObjectText();
        },
        linkClick3: function (p_objectType,p_server,p_database,p_dbschema,p_dbobject) {  

            this.selectedSechemaName = p_dbschema;
            this.selectedObjectName = p_dbobject;

            //this.loadObjectText();            
            this.loadObjectTextUsingParams(p_dbschema, p_dbobject, p_server, p_database)

        }


    },

    mounted: function() {
        this.$nextTick(function () {
            this.getServerNameList();
        });

        let self  = this ;
        this.$refs['refTblData'].addEventListener('click', function(event) {            
           
            //console.log('clicked: ', event.target);
            if (event.target.tagName.toLowerCase() === 'a') {

                if (event.target.getAttribute("data-val")) {
                    self.linkClick2(event.target.getAttribute("href"));
                }
            } 
           
            event.preventDefault();
        })
        

      
    },
    watch: {
      $route(to, from, next) {
          console.log('in watch');
          console.log(to.params.dbschema);
          console.log(to.params.dbobject);   
          
          this.linkClick3(to.params.objectType,to.params.server,to.params.database,to.params.dbschema,to.params.dbobject);

      }
    },


}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>



#SqlMain {
  margin: 10px;
}

#objectNavigator {
    float: left;
    width: 25%;
    height: 800px;
    overflow: auto;
    background: #F8F8F8;
    padding: 20px;
}

#textCtrl {
    float: left;
    padding: 20px;
    width: 70%;
    height: 800px;
    /* overflow: hidden; */
    background-color: white;
}

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

#objectTypeComboBox, #objFilter, #database, #server {
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
    #objectNavigator, #textCtrl {
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
    background: rgba(0,0,0,0.6);
}

.modal-content {
    padding: 20px 30px;
    width: 400px;
    position: relative;
    min-height: 150px;
    margin: 5% auto 0;
    background: #fff;
}

    .modal-content label, .modal-content input, .modal-content select {
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
