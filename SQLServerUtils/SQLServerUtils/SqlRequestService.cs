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

        public SqlRequestService(string server, string database)
        {           
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
                        return htmlDest.ToString();
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
                        return tparser.Execute(dbObject.object_text);
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

                    Thread t = new Thread(() => SearchDB(searchString, server, database, objTypeList, ref listofObjects));
                    threads.Add(t);
                    t.Name = server + "-" + database;
                    t.Start();
                }

                foreach (Thread item in threads)
                {
                    item.Join();
                }

                foreach (object ob in allObjList)
                {
                    List<string> tmp = (List<string>) ob;
                    retList.AddRange(tmp);
                }


            }

            return retList;
        }

        public void SearchDB(string searchString, string server, string database, List<string> objTypeList, ref List<string> listofObjects)
        {
            Console.WriteLine("thread " + server + "." +  database + "started");

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
                }
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
