using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

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


        //openInNotepadd
        [RequestMapping("/openInNotepadd", "POST")]
        public void handleOpenInNotepaddRequest(HttpListenerContext context)
        {
            try
            {
                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);
                string result = "";
                string objectName = context.Request.QueryString[Resources.objectNameParam];
                string schemaName = context.Request.QueryString[Resources.schemaNameParam];
                string flPath = "Error";
                if (objectName != null && !string.IsNullOrEmpty(objectName) && objectName.Length > 2)
                {
                    result = msSqlRequestService.requestOpenInNotepadd(connParams.server, connParams.database, schemaName, objectName);

                    string DiffPath = ConfigurationManager.AppSettings["DiffPath"];
                    string NPPBatEditPath = DiffPath + "\\" + "edit.bat";
                    flPath = DiffPath + "\\" + $"{objectName}.sql";


                    FileUtils.DeleteOldSqlFile(DiffPath, "*.sql"); // housekeeping

                    FileUtils.WriteTextToFile(flPath, result);

                    if (File.Exists(flPath))
                    {
                        System.Diagnostics.Process.Start(NPPBatEditPath, $"\"{flPath}\"");
                        Console.WriteLine($"Notepad++ started for {flPath}");
                    }
                    else
                    {
                        Console.WriteLine($"File {flPath} not exist");

                    }

                    

                    
                }
                sendStaticResource(context.Response, $"Notepad++ started for {flPath}", "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
                return;
            }
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

                string leftIsFileOnDisk = context.Request.QueryString["leftIsFileOnDisk"];
                string rightIsFileOnDisk = context.Request.QueryString["rightIsFileOnDisk"];

                string objectType = context.Request.QueryString["objectType"];

                string diffLeft = context.Request.QueryString["diffLeft"];
                string diffRight = context.Request.QueryString["diffRight"];                
                             

                string diffLeftText = "";
                string diffRightText = "";

                if (leftIsFileOnDisk.Equals("true"))
                {
                    diffLeftText = FileUtils.ReadFileContent(diffLeft);
                }
                else 
                {
                    string[] diffLeftAry = diffLeft.Split('.');
                    diffLeftText = msSqlRequestService.requestDiff(objectType, diffLeftAry[0], diffLeftAry[1], diffLeftAry[2], diffLeftAry[3]);
                }


                if (rightIsFileOnDisk.Equals("true"))
                {
                    diffRightText = FileUtils.ReadFileContent(diffRight);
                }
                else
                {
                    string[] diffRightAry = diffRight.Split('.');
                    diffRightText = msSqlRequestService.requestDiff(objectType, diffRightAry[0], diffRightAry[1], diffRightAry[2], diffRightAry[3]);
                }
                


                string DiffPath = ConfigurationManager.AppSettings["DiffPath"];
                string DiffBat = DiffPath + "\\" + "diff.bat";

                FileUtils.WriteTextToFile(DiffPath + "\\" + "left.sql", diffLeftText);
                FileUtils.WriteTextToFile(DiffPath + "\\" + "right.sql", diffRightText);
                System.Diagnostics.Process.Start(DiffBat);

                Console.WriteLine("diffLeft:" + diffLeft + " diffRight:" + diffRight);

                DateTime now = DateTime.Now;
                result = "Diff is triggrered: " + now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
               
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
               
                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);

                List<string> sl = new List<string>();
                string spSearchText = context.Request.QueryString["spSearchText"];

                string searchSP = context.Request.QueryString["searchSP"];
                string searchTable = context.Request.QueryString["searchTable"];
                string searchColumn = context.Request.QueryString["searchColumn"];
                string searchView = context.Request.QueryString["searchView"];
                string searchSynonym = context.Request.QueryString["searchSynonym"];

                string searchAllDbs = context.Request.QueryString["searchAllDbs"];
                string searchAllServers = context.Request.QueryString["searchAllServers"];
                

                Console.WriteLine("searchSP:" + searchSP + " searchTable:" + searchTable + " searchColumn:" + searchColumn + " searchView:" + searchView + " searchSynonym:" + searchSynonym +  " searchAllDbs:" + searchAllDbs + " searchAllServers:" + searchAllServers);

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
                   
                }

                if (searchTable.Equals("true"))
                {
                    objTypeList.Add("table");                  
                }

                if (searchColumn.Equals("true"))
                {
                    objTypeList.Add("column");
                                    }

                if (searchView.Equals("true"))
                {
                    objTypeList.Add("view");
               
                }

                if (searchSynonym.Equals("true"))
                {
                    objTypeList.Add("synonym");

                }
                

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

        [RequestMapping("/HiLiteSearch", "POST")]
        public void handleHiLiteSearchRequest(HttpListenerContext context)
        {
            try
            {


                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);

                string result = "";
                string objName = context.Request.QueryString["objName"];


                Console.WriteLine($"HiLiteSearch objName: {objName}  received by server");

                string databaseLocal = connParams.database;

                if (objName.Contains("."))
                {
                    string[] parts = objName.Split('.');
                    objName = parts[parts.Length - 1].Replace("[", "").Replace("]", "").Trim();

                    if (parts.Length == 3) //indicates another server is used
                    {
                        databaseLocal = parts[0].Replace("[", "").Replace("]", "").Trim();                        
                    }
                }
                Console.WriteLine($"databaseLocal: {databaseLocal}");

                result = msSqlRequestService.requestObjectScript(connParams.server, databaseLocal, objName);
                
                sendStaticResource(context.Response, result, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
                return;
            }
        }

        [RequestMapping("/DependentSearch", "POST")]
        public void handleDependentSearchRequest(HttpListenerContext context)
        {
            try
            {


                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);

                string result = "";
                string pattern = ConfigurationManager.AppSettings["DependencyRegex"];//@"(?i)(EXEC|SP_EXECUTESQL)\s[!-~]+\s";
                bool useheader = true;

                string objDependentName = context.Request.QueryString["objDependentName"];
                string regexDependent = context.Request.QueryString["regexDependent"];

                string localregexDependent = FileUtils.DecodeFromBase58(regexDependent);

                if (!string.IsNullOrEmpty(localregexDependent) && localregexDependent.Length > 0)
                {
                    Console.WriteLine($"Will use front end regex: {localregexDependent}");
                    pattern = localregexDependent;
                }

                Console.WriteLine($"objDependentName: {objDependentName}  received by server");

                result = msSqlRequestService.requestDependentInfo(objDependentName, connParams.database, pattern, useheader);

                sendStaticResource(context.Response, result, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sendStaticResourceWithCode(context.Response, JObject.FromObject(new { errorMessage = ex.Message }).ToString(), "application/json", 406);
                return;
            }
        }

        [RequestMapping("/GetTableColumns", "POST")]
        public void handleGetTableColumns(HttpListenerContext context)
        {
            try
            {


                ConnectionDto connParams = readResponseAsJson<ConnectionDto>(context.Request);
                SqlRequestService msSqlRequestService = new SqlRequestService(connParams.server, connParams.database);

                string result = "";
           
                string tableName = context.Request.QueryString["tableName"];
           
                Console.WriteLine($"Will execute GetTableColumns() tableName: {tableName}  received by the server");

                result = msSqlRequestService.requestGetTableColumns(tableName);

                sendStaticResource(context.Response, result, "text/html");
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

                sqlquery = FileUtils.DecodeFromBase58(sqlquery);
                Console.WriteLine("sqlquery received by server");
                Console.WriteLine(sqlquery);
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
                //sendStaticResource(context.Response, JArray.FromObject(databaseList).ToString(), "application/javascript");
                string result = JArray.FromObject(databaseList).ToString();
                sendStaticResource(context.Response, result, "application/javascript");

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
