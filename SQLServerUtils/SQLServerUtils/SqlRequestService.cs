using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using System.Xml.Xsl;
using SqlScriptParser;
using Dapper;
using System.Threading;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace SQLServerUtils
{
    class DbObject
    {
        public string name { get; set; }
        public string schema_name { get; set; }
        public string type_desc { get; set; }
        public string object_text { get; set; }
    }

    class DbObjectType
    {
        public string type { get; set; }
        public string type_desc { get; set; }
    }

    class DbDependentObject
    {
        public string referenced_entity_name { get; set; }
        public string type_desc { get; set; }
        public string base_object_name { get; set; }
        public string schema_name { get; set; }
        public int num { get; set; }

        public string buildSqlServerObjectLink()
        {
            return $"<a data-val='syn' href='#!/{schema_name}.{referenced_entity_name}' title='{type_desc}'>{referenced_entity_name}<a>";
        }
    }

    class SqlRequestService
    {
        HashSet<string> keywords1;
        HashSet<string> keywords2;
        XslCompiledTransform xslTranCompiler;
        string connectionString;
        string server;
        string database;
        public SqlRequestService(string server, string database)
        {
            this.server = server;
            this.database = database;
            string tmpConnectionString = string.Format(Resources.connectionStringTemplate, server, database);
            try
            {
                using (var sqlConn = new SqlConnection(tmpConnectionString))
                {
                    sqlConn.Open();
                    connectionString = tmpConnectionString;
                }
            } catch (Exception ex)
            {
                connectionString = "";
                throw ex;
            }
            keywords1 = new HashSet<string>(Resources.keywords1.Split(' '));
            keywords2 = new HashSet<string>(Resources.keywords2.Split(' '));
            xslTranCompiler = new XslCompiledTransform();
            var xslDoc = new XmlDocument();
            xslDoc.LoadXml(Resources.table2html_xslt);
            xslTranCompiler.Load(xslDoc);
        }

        public List<string> requestDatabaseList(string connectionString)
        {
            using (var sqlConn = openConnection(connectionString))
            {
                return sqlConn.Query<String>(Resources.queryDatabaseList_sql).ToList();
            }
        }

        public List<object> requestAllServerObjectList()
        {
            List<object> allServerObjects = new List<object>();
            try
            {
                using (var sqlConn = openConnection(connectionString))
                {
                    var objectTypeList = sqlConn.Query<DbObjectType>(Resources.queryObjectTypes_sql).ToList();
                    foreach (DbObjectType objType in objectTypeList)
                    {
                        var objectList = sqlConn
                            .Query<DbObject>(Resources.queryAllObjects_sql, new { type = objType.type })
                            .Select(o => new { name = o.name, schema_name = o.schema_name })
                            .ToList();
                        if (objectList.Count > 0)
                        {
                            allServerObjects.Add(new { type_desc = objType.type_desc, objects = objectList });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
            return allServerObjects;
        }

        public string requestDatabaseObjectInfo(string objectName, string schemaName)
        {
            try
            {
                using (var sqlConn = openConnection(connectionString))
                {
                    DbObject dbObject = null;
                    try
                    {
                        dbObject = sqlConn.Query<DbObject>(Resources.queryObjectInfo_sql, new { objectName = objectName, schemaName = schemaName }).Single();
                    } catch(InvalidOperationException)
                    {
                        return $"object '{schemaName}.{objectName}' not exists";
                    }

                    if (dbObject.type_desc == "USER_TABLE")
                    {
                        var objXml = sqlConn.Query<String>(Resources.queryTableXml_sql, new { objectName = objectName, schemaName = schemaName }).ToList();
                        var xmlSource = new XmlDocument();
                        xmlSource.LoadXml(String.Join("", objXml));
                        var htmlDest = new StringBuilder();
                        xslTranCompiler.Transform(xmlSource, XmlWriter.Create(htmlDest));

                        string idexinfo = FileUtils.ListTableIndexes(this.server, this.database, objectName);
                        string idxhtml = $"<div>{idexinfo}</div>";
                        
                        
                        return (idxhtml + htmlDest.ToString());
                    } else if (dbObject.type_desc == "SYNONYM")
                    {
                       // Console.WriteLine(dbObject.object_text);
                        return FileUtils.ScriptSynonym(this.server, this.database, objectName);                        
                        
                    }

                    if (objectName != "")
                    {



                        //Dictionary<string, string> depList = sqlConn
                        //    .Query<DbDependentObject>(Resources.queryObjectDependancies_sql, new { objectFullName = $"{schemaName}.{objectName}" })
                        //    .Where(dep => dep.num == 1)
                        //    .ToDictionary(dep => dep.referenced_entity_name.ToLower(), dep => dep.buildSqlServerObjectLink());

                        //var dependencyProcessor = new DependencyProcessor(depList);
                        //var keywordProcessor1 = new KeywordProcessor(dependencyProcessor, keywords1, "<b style='color:blue'>{0}</b>");
                        //var keywordProcessor2 = new KeywordProcessor(keywordProcessor1, keywords2, "<span style='color:magenta'>{0}</span>");
                        //var tempTableProcessor = new BlockProcessor(keywordProcessor2, @"@{1,2}\w+", "<span style='color:dimgray'>{0}</span>");
                        //var varableProcessor = new BlockProcessor(tempTableProcessor, @"\#{1,2}\w+", "<span style='color:dimgray'>{0}</span>");
                        //var singleCommentProcessor = new BlockProcessor(varableProcessor, @"--.*[\r\n]", "<b style='color:green'>{0}</b>");
                        //var commentAndStringProcessor = new CommentAndStringProcessor(singleCommentProcessor);
                        //return commentAndStringProcessor.Process(dbObject.object_text);

                        TSql_Parser tparser = new TSql_Parser();
                        return tparser.ExecuteTable(dbObject.object_text.Trim());
                    }
                }
                return "unknown type of object";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                return $"Exception: {ex}";
            }
        }

        public string requestObjectTypeDesc(string objectName, string schemaName)
        {
            try
            {
                using (var sqlConn = openConnection(connectionString))
                {
                    DbObject dbObject = null;
                    try
                    {
                        dbObject = sqlConn.Query<DbObject>(Resources.queryObjectInfo_sql, new { objectName = objectName, schemaName = schemaName }).Single();
                    }
                    catch (InvalidOperationException)
                    {
                        return $"object '{schemaName}.{objectName}' not exists";
                    }

                    if (dbObject.type_desc == "USER_TABLE")
                    {
                        return "USER_TABLE";
                    }
                    else
                    {
                        return dbObject.type_desc;

                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                return $"Exception: {ex}";
            }
        }


        public string requestDatabaseTableData(string objectName, string schemaName)
        {
            string connString = connectionString; //@"Server =SCSQLD3; Database = TFSDW; Trusted_Connection = True;";

            string tbl = schemaName + "." + objectName;

            string retVal = "No data found";
            // for debug
            //System.Windows.Forms.MessageBox.Show("Selected Item: " + wItem);
            try
            {

                using (SqlConnection conn = new SqlConnection(connString))
                {

                    string query = @" SELECT TOP 10 * FROM " + tbl + " ;";


                    SqlCommand cmd = new SqlCommand(query, conn);
                    //cmd.Parameters.AddWithValue("@pSystem_WorkItemType", wItem);


                    SqlDataAdapter dAdapter = new SqlDataAdapter(cmd);

                    DataSet ds = new DataSet();

                    dAdapter.Fill(ds);

                    //ds.Tables[0];

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        retVal = ConvertDataTableToHTML(ds.Tables[0]);
                    }
                    else
                    {
                        retVal = "Table " + objectName + " is empty";
                    }


                    conn.Close();

                    return retVal;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;

            }

        }


        public string requestGetTableColumns(string tableName)
        {
            try
            {               
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the table exists
                    using (SqlCommand cmd = new SqlCommand($"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{tableName}'", connection))
                    {
                        int count = (int)cmd.ExecuteScalar();
                        if (count == 0)
                        {
                            return $"{tableName} does not exist";
                        }
                    }

                    StringBuilder query = new StringBuilder("SELECT TOP 1000 ");
                    using (SqlCommand command = new SqlCommand($"SELECT TOP 0 * FROM {tableName}", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            var table = reader.GetSchemaTable();
                            for (int i = 0; i < table.Rows.Count; i++)
                            {
                                query.Append(table.Rows[i]["ColumnName"].ToString());
                                if (i < table.Rows.Count - 1)
                                {
                                    query.AppendLine(", ");
                                }
                            }
                        }
                    }
                    query.AppendLine("");
                    query.Append($" FROM {tableName}");
                    return query.ToString();
                }

            }
            catch (Exception ex)
            {
                string retMsg = $"Error in requestGetTableColumns() using tablename: {tableName} Error: {ex.Message}";

                Console.WriteLine(ex.Message);
                return retMsg;

            }

        }

        public string requestObjectScript(string serverName, string databaseName, string objectName)
        {
            try
            {
                StringBuilder retVal = new StringBuilder();

                string objectType = FileUtils.GetObjectType(serverName, databaseName, objectName);

                string connectionStringLocal = $"Data Source={serverName};Initial Catalog={databaseName};Integrated Security=True";
                Console.WriteLine($"In requestObjectScript() objectType {objectType} using {connectionStringLocal}");
                using (SqlConnection connection = new SqlConnection(connectionStringLocal))
                {
                    Server server = new Server(new ServerConnection(connection));

                    Scripter scripter = new Scripter(server);
                    scripter.Options.ScriptDrops = false;
                    scripter.Options.NoCollation = true;

                    Database db = server.Databases[databaseName];

                   

                    // Get the object and generate the script
                    StringCollection result = null;
                    switch (objectType)
                    {
                        case "Table":
                            if (db.Tables.Contains(objectName))
                            {
                                result = db.Tables[objectName].Script(scripter.Options);
                            }
                            break;
                        case "View":
                            if (db.Views.Contains(objectName))
                            {
                                result = db.Views[objectName].Script();
                            }
                            break;
                        case "Synonym":
                            if (db.Synonyms.Contains(objectName))
                            {
                                result = db.Synonyms[objectName].Script();
                            }
                            break;
                        case "StoredProcedure":
                            if (db.StoredProcedures.Contains(objectName))
                            {
                                result = db.StoredProcedures[objectName].Script();
                            }
                            break;
                        case "ScalarFunction":
                            if (db.UserDefinedFunctions.Contains(objectName) && db.UserDefinedFunctions[objectName].FunctionType == UserDefinedFunctionType.Scalar)
                            {
                                result = db.UserDefinedFunctions[objectName].Script();
                            }
                            break;
                        case "TableValuedFunction":
                            if (db.UserDefinedFunctions.Contains(objectName) && db.UserDefinedFunctions[objectName].FunctionType == UserDefinedFunctionType.Table)
                            {
                                result = db.UserDefinedFunctions[objectName].Script();
                            }
                            break;
                        case "InlineTableValuedFunction":
                            if (db.UserDefinedFunctions.Contains(objectName) && db.UserDefinedFunctions[objectName].FunctionType == UserDefinedFunctionType.Inline)
                            {
                                result = db.UserDefinedFunctions[objectName].Script();
                            }
                            break;
                        default:
                            Console.WriteLine($"{objectType} is not known");
                            break;
                    }

                    // Output the script
                    if (result != null)
                    {
                        foreach (string str in result)
                        {
                            retVal.AppendLine(str);
                        }
                    }
                    else
                    {                        
                        return $"<p>{objectName}<span style='color:red'> is not found</span></p>";
                    }


                }

                string retValCleansed = retVal.ToString()
                        .Replace("SET ANSI_NULLS ON", "")
                        .Replace("SET QUOTED_IDENTIFIER ON","").Trim();

                if ("Table".Equals(objectType))
                {
                    retValCleansed = retValCleansed.Replace("[","").Replace("]", "");
                }

                TSql_Parser parser = new TSql_Parser();
                string retValHtml =  parser.Execute(retValCleansed);

                return retValHtml;

            }
            catch (Exception ex)
            {                
                string retMsg = $"<p><span style='color:red'>{ex.Message}</span></p>";

                Console.WriteLine(ex.Message);
                return retMsg;

            }

        }

        public string requestDependentInfo(string objectName, string databaseName, string pattern, bool useheader)
        {   
            DataTable table = new DataTable();

            try
            {

             
                //int cnt = 0;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    Server server = new Server(new ServerConnection(connection));
                    Database database = server.Databases[databaseName];

                    List<string> matches = new List<string>();



                    // Add three columns
                    table.Columns.Add("LineNumber", typeof(string));
                    table.Columns.Add("ObjectName", typeof(string));
                    table.Columns.Add("DependentObject", typeof(string));


                    StoredProcedure storedProcedure = database.StoredProcedures[objectName.Replace("dbo.","")];
                    if (storedProcedure != null)
                    {
                        string text = "";

                        if (useheader)
                        {
                            text = storedProcedure.TextHeader + "\r\n" + storedProcedure.TextBody;
                        }
                        else
                        {
                            text = storedProcedure.TextBody;
                        }


                        foreach (Match match in Regex.Matches(text, pattern))
                        {
                            int lineNumber = text.Substring(0, match.Index).Split('\n').Length;
                            string strLineNumber = lineNumber.ToString().PadLeft(6, ' ');
                            matches.Add($"{strLineNumber} - {storedProcedure.Name} - {match.Value}");
                            table.Rows.Add(strLineNumber, storedProcedure.Name, match.Value);
                        }


                    }
                   

                    //foreach (StoredProcedure storedProcedure in database.StoredProcedures)
                    //{

                    //    if (storedProcedure.IsSystemObject)
                    //    {
                    //        cnt++;
                    //        if (cnt % 100 == 0)
                    //        {
                    //            Console.WriteLine($"DependentInfo count: {cnt.ToString().PadLeft(6, ' ')}");
                    //        }

                    //        continue;
                    //    }

                    //    string text = "";

                    //    if (useheader)
                    //    {
                    //        text = storedProcedure.TextHeader + "\r\n" + storedProcedure.TextBody;
                    //    }
                    //    else
                    //    {
                    //        text = storedProcedure.TextBody;
                    //    }




                    //    foreach (Match match in Regex.Matches(text, pattern))
                    //    {
                    //        int lineNumber = text.Substring(0, match.Index).Split('\n').Length;
                    //        string strLineNumber = lineNumber.ToString().PadLeft(6, ' ');
                    //        matches.Add($"{strLineNumber} - {storedProcedure.Name} - {match.Value}");
                    //    }
                    //}

                }
               
                if(table.Rows.Count == 0)
                {
                    table.Rows.Add("0", "No dependency found", "");
                }

                return ConvertDataTableToHTML(table); 
            }
            catch (Exception ex)
            {
                string retMsg = $"<p><span style='color:red'>{ex.Message}</span></p>";
                Console.WriteLine(ex.Message);
                return retMsg;

            }

        }


        public string requestAdHocSQl(string sqlquery)
        {
            string connString = connectionString; //@"Server =SCSQLD3; Database = TFSDW; Trusted_Connection = True;";
            
            string retVal = "No data found";
            // for debug
            //System.Windows.Forms.MessageBox.Show("Selected Item: " + wItem);
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = sqlquery;

                    SqlCommand cmd = new SqlCommand(query, conn);
                    //cmd.Parameters.AddWithValue("@pSystem_WorkItemType", wItem);


                    SqlDataAdapter dAdapter = new SqlDataAdapter(cmd);

                    DataSet ds = new DataSet();

                    dAdapter.Fill(ds);

                    //ds.Tables[0];

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        retVal = ConvertDataTableToHTML(ds.Tables[0]);
                    }
                    else
                    {
                        retVal = "No results found.";
                    }


                    conn.Close();

                    return retVal;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;

            }

        }



        public List<string> requestSysObjectInfo(string searchString, string server, string database, string dbObjectType, string searchAllDbs, string searchAllServers)
        {
            string connString = connectionString; //@"Server =SCSQLD3; Database = TFSDW; Trusted_Connection = True;";           
            List<string> listofObjects = new List<string>();

            List<string> listofErrors = new List<string>();

            try
            {

                string query = "";

                if (dbObjectType.Equals("table"))
                {
                    query = ""
                             + "SELECT	distinct s.name	SchemaName, o.name	ObjectName "
                             + "FROM sys.objects o  "
                             + "LEFT JOIN sys.schemas s ON s.schema_id = o.schema_id "
                             + "WHERE o.type IN ('U','V') "
                             + "AND o.name like '%" + searchString + "%' ";
                }
                else if (dbObjectType.Equals("sp"))
                {
                    query = ""
                        + "select distinct schema_name(a.schema_id) SchemaName,  a.name ObjectName	"
                        + "from sys.objects a join sys.syscomments b on a.object_id = b.id where a.type = 'p' "
                        + "and b.text like '%" + searchString + "%' ";
                }
                else if (dbObjectType.Equals("column"))
                {
                    query = ""
                   + "SELECT distinct s.name	SchemaName, t.name + '.'+ c.name ObjectName "
                    + "FROM sys.columns c "
                    + "JOIN sys.tables t ON c.object_id = t.object_id "
                    + "LEFT JOIN sys.schemas s ON s.schema_id = t.schema_id "
                    + "WHERE c.name LIKE  '%" + searchString + "%' "
                    + "ORDER BY  1,2 ";
                }
                else if (dbObjectType.Equals("view"))
                {
                    query = ""
                        + "select distinct schema_name(a.schema_id) SchemaName,  a.name ObjectName	"
                        + "from sys.objects a join sys.syscomments b on a.object_id = b.id where a.type = 'v' "
                        + "and b.text like '%" + searchString + "%' ";
                }

                


                connString = string.Format(Resources.connectionStringTemplate, server, "master");

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    List<string> databaseList = new List<string>();

                    if (searchAllDbs.Equals("true"))
                    {
                        databaseList = requestDatabaseList(connString);
                        Console.WriteLine("Will search all databases");
                    }
                    else
                    {
                        databaseList.Add(database);
                        Console.WriteLine("Will search one database: " + database);
                    }
                       

                    foreach (string dbName in databaseList)
                    {
                        if (!dbName.ToLower().Equals("master"))
                        {

                            try
                            {

                                conn.ChangeDatabase(dbName);

                                SqlCommand cmd = new SqlCommand(query, conn);
                                cmd.CommandTimeout = 30; //in seconds
                                SqlDataAdapter dAdapter = new SqlDataAdapter(cmd);

                                DataSet ds = new DataSet();

                                dAdapter.Fill(ds);

                                //ds.Tables[0];

                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    DataTable dt = ds.Tables[0];

                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        //for (int j = 0; j < dt.Columns.Count; j++){}
                                        listofObjects.Add(dbObjectType + "." + server + "." + dbName + "." + dt.Rows[i][0].ToString() + "." + dt.Rows[i][1].ToString());
                                    }
                                }

                            }
                            catch (Exception exdb)
                            {
                                listofErrors.Add(dbObjectType + "." + server + "." + dbName + "." + "ERROR" + "." + exdb.Message);
                            }    

                        }


                    }//foreach dbName
                    conn.Close();

                }
                 

                //if (listofObjects.Count == 0)
                //{
                //    listofObjects.Add("No object found");
                //}
                if (listofErrors.Count > 0)
                {
                    listofObjects.AddRange(listofErrors);
                }

                  return listofObjects;
          
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                listofObjects.Add(ex.Message);
                return listofObjects;

            }

        }


        public List<string> requestSysObjectInfoConcurrent(string searchString, List<string> serverList, List<string> databaseList,  List<string> objTypeList)
        {
            List<string> retList = new List<string>();
            List<string> retListError = new List<string>();
            List<string> retListAll = new List<string>();


            string TrackDBInfo = ConfigurationManager.AppSettings["TrackDBInfo"];
            Console.WriteLine($"TrackDBInfo: {TrackDBInfo}");
            string DiffPath = ConfigurationManager.AppSettings["DiffPath"];
            string FileTrackDBInfo = $"{DiffPath}\\TrackDBInfo.txt";

            List<string> listTrackedDBInfo = new List<string>();
            List<string> listTrackedDBInfoOrig = new List<string>();

            if ("true".Equals(TrackDBInfo))
            {
                if (File.Exists(FileTrackDBInfo))
                {
                    listTrackedDBInfo = FileUtils.GetListOfFileContent(FileTrackDBInfo);
                    listTrackedDBInfoOrig = FileUtils.GetListOfFileContent(FileTrackDBInfo);
                }
            }
            
            foreach (string server in serverList)
            {                

                if (databaseList.Count == 0)
                {
                    string connString = string.Format(Resources.connectionStringTemplate, server, "master");
                    databaseList = requestDatabaseList(connString);
                }                

                List<object> allObjList = new List<object>();
                List<Thread> threads = new List<Thread>();

                foreach (string database in databaseList)
                {


                    List<string> listofObjects = new List<string>();
                    allObjList.Add(listofObjects);

                    string dbbinfo = $"{server.Trim().ToLower()}|{database.Trim().ToLower()}";
                    
                    if ("true".Equals(TrackDBInfo) && listTrackedDBInfo.Contains(dbbinfo))
                    {                        
                        Console.WriteLine($"{dbbinfo} is skipped");                        
                    }
                    else
                    {
                        Thread t = new Thread(() => SearchDB(searchString, server, database, objTypeList, ref listTrackedDBInfo, ref listofObjects));
                        threads.Add(t);
                        t.Name = server + "-" + database;
                        t.Start();
                    }


                }

                foreach (Thread item in threads)
                {
                    item.Join();
                }

                foreach (object ob in allObjList)
                {
                    List<string> tmp = (List<string>) ob;
                    foreach (string tmpItem in tmp)
                    {
                        if (tmpItem.Contains("dbObjectType ALL"))
                        {
                            retListError.Add(tmpItem);
                        }
                        else
                        {
                            retList.Add(tmpItem);
                        }
                    }                    
                }

                retListAll.AddRange(retList);
                retListAll.AddRange(retListError);




            }
            //sort before sending


            if ("true".Equals(TrackDBInfo))
            {
                foreach (string item in listTrackedDBInfo)
                {
                    if (!listTrackedDBInfoOrig.Contains(item))
                    {
                        FileUtils.AppendToFile(FileTrackDBInfo, item);
                    }
                }

            }


            return retListAll;
        }

        public void SearchDB(string searchString, string server, string database, List<string> objTypeList, ref List<string> listTrackedDBInfo, ref List<string> listofObjects)
        {
            string searchString2 = "";

            if (searchString.Contains("||"))
            {
                string[] searchStringArray = searchString.Trim().Split(new string[] { "||" }, StringSplitOptions.None);

                foreach (string item in searchStringArray)
                {
                    searchString2 += $" and b.text like '%{item}%' \r\n";
                }
            }
            else
            {
                searchString2 = $"and b.text like '%{searchString}%'";
            }

            Console.WriteLine("thread " + server + "." +  database + "started");

            string TrackDBInfo = ConfigurationManager.AppSettings["TrackDBInfo"];
    

            string connString = string.Format(Resources.connectionStringTemplate, server, database);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();


                    foreach (string dbObjectType in objTypeList)
                    {
                        string query = "";

                        if (dbObjectType.Equals("table"))
                        {
                            query = ""
                                     + "SELECT	distinct s.name	SchemaName, o.name	ObjectName "
                                     + "FROM sys.objects o  "
                                     + "LEFT JOIN sys.schemas s ON s.schema_id = o.schema_id "
                                     + "WHERE o.type IN ('U','V') "
                                     + "AND o.name like '%" + searchString + "%' ";
                        }
                        else if (dbObjectType.Equals("sp"))
                        {
                            query = ""
                                + "select distinct schema_name(a.schema_id) SchemaName,  a.name ObjectName	"
                                + "from sys.objects a join sys.syscomments b on a.object_id = b.id where a.type = 'p' "
                                + searchString2;  //+ "and b.text like '%" + searchString + "%' ";
                        }
                        else if (dbObjectType.Equals("column"))
                        {
                            query = ""
                           + "SELECT distinct s.name	SchemaName, t.name + '.'+ c.name ObjectName "
                            + "FROM sys.columns c "
                            + "JOIN sys.tables t ON c.object_id = t.object_id "
                            + "LEFT JOIN sys.schemas s ON s.schema_id = t.schema_id "
                            + "WHERE c.name LIKE  '%" + searchString + "%' "
                            + "ORDER BY  1,2 ";
                        }
                        else if (dbObjectType.Equals("view"))
                        {
                            query = ""
                                + "select distinct schema_name(a.schema_id) SchemaName,  a.name ObjectName	"
                                + "from sys.objects a join sys.syscomments b on a.object_id = b.id where a.type = 'v' "
                                + "and b.text like '%" + searchString + "%' ";
                        }
                        else if (dbObjectType.Equals("synonym"))
                        {
                            query = ""
                                 + "SELECT distinct schema_name(o.schema_id) SchemaName, s.name "
                                 + "FROM sys.synonyms s "
                                 + "JOIN sys.objects o on o.object_id = s.object_id "
                                 + "WHERE ( s.name LIKE '%" + searchString + "%' OR s.base_object_name LIKE '%" + searchString + "%') ";
                        }
                        else
                        {
                            continue;
                        }



                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.CommandTimeout = 30; //in seconds
                        SqlDataAdapter dAdapter = new SqlDataAdapter(cmd);

                        DataSet ds = new DataSet();
                        dAdapter.Fill(ds);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataTable dt = ds.Tables[0];
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                listofObjects.Add(dbObjectType + "." + server + "." + database + "." + dt.Rows[i][0].ToString() + "." + dt.Rows[i][1].ToString());
                            }
                        }
                    }//foreach (string dbObjectType in objTypeList)


                    conn.Close();

                }
                catch (Exception exdb)
                {
                    listofObjects.Add("dbObjectType ALL" + "." + server + "." + database + "." + "ERROR" + "." + exdb.Message);
                    if (conn != null) { conn.Close(); }


                    if ("true".Equals(TrackDBInfo))
                    {
                        string dbbinfo = $"{server.Trim().ToLower()}|{database.Trim().ToLower()}";
                        if (!listTrackedDBInfo.Contains(dbbinfo))
                        {                            
                            listTrackedDBInfo.Add(dbbinfo);
                            Console.WriteLine($"{dbbinfo} is writted to tracker");
                        }
                    }


                }
            }




        }

        public string requestOpenInNotepadd(string server, string database, string schemaName, string objectName)
        {
            try
            {
                string connectionString = string.Format(Resources.connectionStringTemplate, server, database);

                using (var sqlConn = openConnection(connectionString))
                {

                    DbObject dbObject = null;
                    try
                    {
                        dbObject = sqlConn.Query<DbObject>(Resources.queryObjectInfo_sql, new { objectName = objectName, schemaName = schemaName }).Single();
                    }
                    catch (InvalidOperationException)
                    {
                        return $"object '{schemaName}.{objectName}' not exists";
                    }



                    if (dbObject.type_desc == "USER_TABLE")
                    {
                        return $"object type user_table '{schemaName}.{objectName}' not supported.";
                    }

                    if (dbObject != null)
                    {
                        return dbObject.object_text;
                    }


                }
                return "unknown type of object";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                return $"Exception: {ex}";
            }


        }
        public string requestDiff(string objectType, string server, string database, string schemaName, string objectName)
        {
            try
            {
                string connectionString = string.Format(Resources.connectionStringTemplate, server, database);

                using (var sqlConn = openConnection(connectionString))
                {

                    DbObject dbObject = null;
                    try
                    {
                        dbObject = sqlConn.Query<DbObject>(Resources.queryObjectInfo_sql, new { objectName = objectName, schemaName = schemaName }).Single();
                    }
                    catch (InvalidOperationException)
                    {
                        return $"object '{schemaName}.{objectName}' not exists";
                    }



                    if (dbObject.type_desc == "USER_TABLE")
                    {
                        var xxx = FileUtils.ListTableIndexes(server, database, objectName);
                        return FileUtils.ScriptTable(server, database, objectName);
                    }

                    if (dbObject != null)
                    {
                        return dbObject.object_text;
                    }

                    
                }
                return "unknown type of object";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                return $"Exception: {ex}";
            }

           
        }




        public string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table class='table  table-striped'>";

            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                html += "<th>" + dt.Columns[i].ColumnName + "</th>";
            }
            html += "</tr>";


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                }

                html += "</tr>";
            }
            html += "</table>";
            return html;
        }


        IDbConnection openConnection(string connectionString)
        {
            var sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();
            return sqlConn;
        }
    }
}
