using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;

namespace SQLServerUtils
{

    class TheHttpServer : SimpleHttpServer
    {
        class ConnectionDto
        {
            public string server;
            public string database;
        }

        public TheHttpServer(string url) : base(url)
        {            
        }
        
        [RequestMapping("/main.css", "GET")]
        public void handleMainCssRequest(HttpListenerContext context)
        {
            sendStaticResource(context.Response, Resources.main_css, "text/css");
        }

        [RequestMapping("/objectText.html", "GET")]
        public void handleObjectTextRequest(HttpListenerContext context)
        {
            sendStaticResource(context.Response, Resources.objectText_html, "text/css");
        }

        [RequestMapping("/vendor.js", "GET")]
        public void handleVueJsRequest(HttpListenerContext context)
        {
            sendStaticResource(context.Response, Resources.vendor_js, "application/javascript");
        }
        [RequestMapping("/vendor.css", "GET")]
        public void handleVendorCSSRequest(HttpListenerContext context)
        {
            sendStaticResource(context.Response, Resources.vendor_css, "text/css");
        }

        [RequestMapping("/vendor.map", "GET")]
        public void handleVendorMapRequest(HttpListenerContext context)
        {
            sendStaticResource(context.Response, Resources.vendor_map, "application/javascript");
        }

        [RequestMapping("/app.js", "GET")]
        public void handleAppJsRequest(HttpListenerContext context)
        {
            sendStaticResource(context.Response, Resources.app_js, "application/javascript");
        }

        [RequestMapping("/app.map", "GET")]
        public void handleAppMapRequest(HttpListenerContext context)
        {
            sendStaticResource(context.Response, Resources.app_map, "application/javascript");
        }


        [RequestMapping("/postConnectionString.js", "GET")]
        public void handlePostConnectionStringJsRequest(HttpListenerContext context)
        {
            sendStaticResource(context.Response, Resources.postConnectionString_js, "application/javascript");
        }

        [RequestMapping("/servernamelist", "POST")]
        public void handleServerNameList(HttpListenerContext context)
        {
            List<string> sl = SimpleHttpServer.getServerListPublic();
            string result = JArray.FromObject(sl).ToString();
            sendStaticResource(context.Response, result, "application/javascript");
        }

        [RequestMapping("/updateservernamelist", "POST")]
        public void handleUpdateServerNameList(HttpListenerContext context)
        {            
            string addservername = "addservername";
            string paddservername = context.Request.QueryString[addservername];
            List<string> sl = SimpleHttpServer.updateServerListPublic(paddservername);
            string result = JArray.FromObject(sl).ToString();
            sendStaticResource(context.Response, result, "application/javascript");
        }

        [RequestMapping("/serverobjectlist", "POST")]
        public void handleServerObjectList(HttpListenerContext context)
        {
            try
            {
                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server.ToString(), connParams.database.ToString());
                string result = JArray.FromObject(msSqlRequestService.requestAllServerObjectList()).ToString();
                sendStaticResource(context.Response, result, "application/javascript");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
                return;
            }
        }

        [RequestMapping("/", "GET")]
        public void handleIndexPageRequest(HttpListenerContext context)
        {            
            sendStaticResource(context.Response, Resources.index_html, "text/html");
        }

        [RequestMapping("/objtext", "POST")]
        public void handleGetObjTextRequest(HttpListenerContext context)
        {
            try
            {
                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);
                string result = "";
                string objectName = context.Request.QueryString[Resources.objectNameParam];
                string schemaName = context.Request.QueryString[Resources.schemaNameParam];
                if (objectName != null)
                {
                    result = msSqlRequestService.requestDatabaseObjectInfo(objectName, schemaName);
                }
                sendStaticResource(context.Response, result, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
                return;
            }
        }

        //axios.post('/diff?objectType=' + this.objectType + '&sch=' + this.selectedSechemaName + '&obj=' + this.selectedObjectName, { 'server': this.server, 'database': this.database })
        [RequestMapping("/diff", "POST")]
        public void handlediffRequest(HttpListenerContext context)
        {
            try
            {
                
                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);

                string result = "";

                string diffLeft = context.Request.QueryString["diffLeft"];
                string diffRight = context.Request.QueryString["diffRight"];                

                string[] diffLeftAry = diffLeft.Split('.');
                string[] diffRightAry = diffRight.Split('.');

                string diffLeftText = msSqlRequestService.requestDiff(diffLeftAry[0], diffLeftAry[1], diffLeftAry[2], diffLeftAry[3], diffLeftAry[4]);
                string diffRightText = msSqlRequestService.requestDiff(diffRightAry[0], diffRightAry[1], diffRightAry[2], diffRightAry[3], diffRightAry[4]);


                string DiffPath = ConfigurationManager.AppSettings["DiffPath"];
                string DiffBat = DiffPath + "\\" + "diff.bat";

                FileUtils.WriteTextToFile(DiffPath + "\\" + "left.sql", diffLeftText);
                FileUtils.WriteTextToFile(DiffPath + "\\" + "right.sql", diffRightText);
                System.Diagnostics.Process.Start(DiffBat);

                Console.WriteLine("diffLeft:" + diffLeft + " diffRight:" + diffRight);

                DateTime now = DateTime.Now;
                result = "Diff is triggrered: " + now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                // TODO get obj text and write to file for left and right and then kick of batch file
                sendStaticResource(context.Response, result, "text/html");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
                return;
            }
        }
        

        [RequestMapping("/querySysObjects", "POST")]
        public void handleQuerySysObjectsRequest(HttpListenerContext context)
        {
            try
            {
                // TO DO 
                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);

                List<string> sl = new List<string>();
                string spSearchText = context.Request.QueryString["spSearchText"];

                string searchSP = context.Request.QueryString["searchSP"];
                string searchTable = context.Request.QueryString["searchTable"];
                string searchColumn = context.Request.QueryString["searchColumn"];
                string searchView = context.Request.QueryString["searchView"];

                string searchAllDbs = context.Request.QueryString["searchAllDbs"];
                string searchAllServers = context.Request.QueryString["searchAllServers"];

                Console.WriteLine("searchSP:" + searchSP + " searchTable:" + searchTable + " searchColumn:" + searchColumn + " searchView:" + searchView + " searchAllDbs:" + searchAllDbs + " searchAllServers:" + searchAllServers);

                List<string> tempList = new List<string>();

                List<string> serverList = new List<string>();
                List<string> databaseList = new List<string>();
                List<string> objTypeList = new List<string>();

                if (!searchAllDbs.Equals("true"))
                {
                    databaseList.Add(connParams.database);
                }

                if (searchAllServers.Equals("true"))
                {
                    serverList = SimpleHttpServer.getServerListPublic();
                    databaseList.Clear(); //remove current because will need to search all dbs
                }
                else
                {
                    serverList.Add(connParams.server);
                }


                if (searchSP.Equals("true"))
                {
                    objTypeList.Add("sp"); 

                    //tempList = msSqlRequestService.requestSysObjectInfo(spSearchText, connParams.server, connParams.database, "sp", searchAllDbs, searchAllServers);
                    //if (tempList.Count > 0)
                    //{
                    //    sl.AddRange(tempList);
                    //}
                    //tempList.Clear();
                }

                if (searchTable.Equals("true"))
                {
                    objTypeList.Add("table");

                    //tempList = msSqlRequestService.requestSysObjectInfo(spSearchText, connParams.server, connParams.database, "table", searchAllDbs, searchAllServers);
                    //if (tempList.Count > 0)
                    //{
                    //    sl.AddRange(tempList);
                    //}
                    //tempList.Clear();
                }

                if (searchColumn.Equals("true"))
                {
                    objTypeList.Add("column");

                    //tempList = msSqlRequestService.requestSysObjectInfo(spSearchText, connParams.server, connParams.database, "column", searchAllDbs, searchAllServers);
                    //if (tempList.Count > 0)
                    //{
                    //    sl.AddRange(tempList);
                    //}
                    //tempList.Clear();
                }

                if (searchView.Equals("true"))
                {
                    objTypeList.Add("view");

                    //tempList = msSqlRequestService.requestSysObjectInfo(spSearchText, connParams.server, connParams.database, "view", searchAllDbs, searchAllServers);
                    //if (tempList.Count > 0)
                    //{
                    //    sl.AddRange(tempList);
                    //}
                    //tempList.Clear();
                }


                //connParams.server, connParams.database

                if (objTypeList.Count > 0 && serverList.Count > 0 && !string.IsNullOrEmpty(connParams.server) && !string.IsNullOrEmpty(connParams.database))
                {
                    sl = msSqlRequestService.requestSysObjectInfoConcurrent(spSearchText, serverList, databaseList, objTypeList);
                }
                else
                {
                    Console.WriteLine("objTypeList is empty and/or db is not selected. Select one of: sp table, column, view");
                }

                if (sl.Count == 0)
                {
                    sl.Add("No object found");
                }

                string result = JArray.FromObject(sl).ToString();
                sendStaticResource(context.Response, result, "application/javascript");                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
                return;
            }
        }

        [RequestMapping("/querydatabase", "POST")]
        public void handleQueryDatabaseRequest(HttpListenerContext context)
        {
            try
            {

              
                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);            

                string result = "";
                string sqlquery = context.Request.QueryString["sqlquery"];
                string schemaName = context.Request.QueryString[Resources.schemaNameParam];

                result = msSqlRequestService.requestAdHocSQl(sqlquery); //requestDatabaseObjectInfo(objectName, schemaName);
                
                sendStaticResource(context.Response, result, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
                return;
            }
        }

        [RequestMapping("/tbldata", "POST")]
        public void handleGetTblDataRequest(HttpListenerContext context)
        {
            try
            {
                string result = "";

                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);

                string objectName = context.Request.QueryString[Resources.objectNameParam];
                string schemaName = context.Request.QueryString[Resources.schemaNameParam];


                if (objectName != null)
                {
                    string ot = msSqlRequestService.requestObjectTypeDesc(objectName, schemaName);

                    if (ot == "USER_TABLE")
                    {
                        result = msSqlRequestService.requestDatabaseTableData(objectName, schemaName);
                    }
                    else
                    {
                        result = ot;
                    }
                }

                sendStaticResource(context.Response, result, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
                return;
            }
        }


        [RequestMapping("/testconnect", "POST")]
        public void handleConnectRequest(HttpListenerContext context)
        {
            try
            {
                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);
                sendAnswerWithCode(context.Response, 200);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message}).ToString(), "application/json", 406);
                return;
            }
        }

        [RequestMapping("/databaselist", "POST")]
        public void handleDatabaseListRequest(HttpListenerContext context)
        {            
            try
            {
                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, "master");
                List<string> databaseList = msSqlRequestService.requestDatabaseList(string.Format(Resources.connectionStringTemplate, connParams.server, "master"));
                sendStaticResource(context.Response, JArray.FromObject(databaseList).ToString(), "application/javascript");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
            }
        }

        T readResponseAsJson<T>(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                var tmp = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(tmp);
            }
        }
    }
}
