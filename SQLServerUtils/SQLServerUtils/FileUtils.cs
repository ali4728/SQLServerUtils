using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using SimpleBase;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace SQLServerUtils
{
    public class FileUtils
    {
        public static string DecodeFromBase58(string input)
        {
            byte[] bytes = Base58.Bitcoin.Decode(input).ToArray();
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        public static string EncodeToBase58(string input)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Base58.Bitcoin.Encode(bytes);
        }

        public static void AppendToFile(string filePath, string content )
        {
            File.AppendAllText(filePath, $"{content}{Environment.NewLine}");
        }

        public static List<string> GetListOfFileContent(string filePath)
        {
            List<string> lines = new List<string>();

            foreach (var line in File.ReadLines(filePath))
            {
                lines.Add(line.Trim().ToLower());
            }

            return lines;
        }
        public static void WriteTextToFile(string fileFullPath, string content)
        {

            using (StreamWriter writer = new StreamWriter(fileFullPath))
            {
                writer.Write(content);
            }
        }


        public static string ReadFileContent(string fileFullPath)
        {
            string retVal = "";

            retVal = File.ReadAllText(fileFullPath);

            return retVal;
        }

        public static void DeleteOldSqlFile(string directoryPath, string ext)
        {
            foreach (string filePath in Directory.GetFiles(directoryPath, ext))
            {
                FileInfo fileInfo = new FileInfo(filePath);                
                
                if (fileInfo.LastWriteTime < DateTime.Now.AddDays(-1))
                {
                    fileInfo.Delete();
                    Console.WriteLine(  $"Deleted old file: {fileInfo.Name}");
                }
            }
        }
        public static string ScriptTable(string servername, string databasename, string tableName)
        {
            
            Server aServer = new Server(servername);           


            Scripter scripter = new Scripter(aServer);
            Database dbObjects = aServer.Databases[databasename];
            /* With ScriptingOptions you can specify different scripting
             * options, for example to include IF NOT EXISTS, DROP
             * statements, output location etc*/
            ScriptingOptions scriptOptions = new ScriptingOptions();
            scriptOptions.ScriptDrops = true;
            scriptOptions.IncludeIfNotExists = true;

            StringBuilder sb = new StringBuilder();

            //Table aTable = dbObjects.Tables[tableName.ToLower()];

            foreach (Table aTable in dbObjects.Tables)
            {

                if (aTable.Name.ToLower().Equals(tableName.ToLower()))
                {
                    /* Generating IF EXISTS and DROP command for tables */
                    StringCollection tableScripts = aTable.Script(scriptOptions);
                    foreach (string script in tableScripts)
                        Console.WriteLine(script);

                    /* Generating CREATE TABLE command */
                    tableScripts = aTable.Script();
                    foreach (string script in tableScripts)
                    {
                        Console.WriteLine(script);
                        sb.AppendLine(script);
                    }


                    IndexCollection indexCol = aTable.Indexes;
                    foreach (Index myIndex in aTable.Indexes)
                    {
                        /* Generating IF EXISTS and DROP command for table indexes */
                        StringCollection indexScripts = myIndex.Script(scriptOptions);
                        foreach (string script in indexScripts)
                        {
                            Console.WriteLine(script);
                            sb.AppendLine(script);
                        }

                        /* Generating CREATE INDEX command for table indexes */
                        indexScripts = myIndex.Script();
                        foreach (string script in indexScripts)
                        {
                            Console.WriteLine(script);
                            sb.AppendLine(script);
                        }
                    }
                }                

            }


            if (sb.ToString().Length < 1)
            {
                sb.AppendLine(tableName + " not found");
            }



            return sb.ToString();
          

        }

        public static string GetObjectType(string servername, string databasename, string objName)
        {
            Console.WriteLine($"in GetObjectType() servername {servername} databasename {databasename} objName {objName}");
            string sqlselect = @"
SELECT    
    CASE 
        WHEN type = 'U' THEN 'Table'
        WHEN type = 'V' THEN 'View'
        WHEN type = 'P' THEN 'StoredProcedure'
        WHEN type = 'FN' THEN 'ScalarFunction'
        WHEN type = 'TF' THEN 'TableValuedFunction'        
        WHEN type = 'IF' THEN 'InlineTableValuedFunction'        
        WHEN type = 'TR' THEN 'Trigger'
        WHEN type = 'SN' THEN 'Synonym'
    END AS ObjectType
FROM 
    sys.objects
WHERE 
    name = '{0}'";

            sqlselect = String.Format(sqlselect, objName);

            string connectionString = $"Data Source={servername};Initial Catalog={databasename};Integrated Security=True";

            string result = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlselect, connection);
                connection.Open();

                result = (string)command.ExecuteScalar();
               
            }

            return result;

        }

        public static string ScriptSynonym(string servername, string databasename, string synonymName)
        {
            Server aServer = new Server(servername);
            Database dbObjects = aServer.Databases[databasename];
            
            ScriptingOptions scriptOptions = new ScriptingOptions();
            scriptOptions.ScriptDrops = true;
            scriptOptions.IncludeIfNotExists = true;

            StringBuilder sb = new StringBuilder();

            foreach (Synonym aSyn in dbObjects.Synonyms)
            {
                if (aSyn.Name.ToLower().Equals(synonymName.ToLower()))
                {                                        
                    StringCollection objScripts = aSyn.Script(scriptOptions);
                    objScripts = aSyn.Script();

                    foreach (string script in objScripts)
                    {
                        //Console.WriteLine(script);
                        sb.AppendLine(script);
                    }
                }
            }

            if (sb.ToString().Length < 1)
            {
                sb.AppendLine(synonymName + " not found");
            }

            return sb.ToString();

        }

        public static string ListTableIndexes(string servername, string databasename, string tableName)
        {

            Server aServer = new Server(servername);
            

            Scripter scripter = new Scripter(aServer);
            
            Database dbObjects = aServer.Databases[databasename];
                        /* With ScriptingOptions you can specify different scripting
             * options, for example to include IF NOT EXISTS, DROP
             * statements, output location etc*/
            ScriptingOptions scriptOptions = new ScriptingOptions();
            scriptOptions.ScriptDrops = true;
            scriptOptions.IncludeIfNotExists = true;
            
            StringBuilder sb = new StringBuilder();

            //Table aTable = dbObjects.Tables[tableName.ToLower()];

            foreach (Table aTable in dbObjects.Tables)
            {

                if (aTable.Name.ToLower().Equals(tableName.ToLower()))
                {                                       
                    foreach (Index myIndex in aTable.Indexes)
                    {
                        string idxtype = "OT";
                        if (myIndex.IndexType.ToString().Equals("NonClusteredIndex"))
                        {
                            idxtype = "NC";
                        }
                        else if (myIndex.IndexType.ToString().Equals("ClusteredIndex"))
                        {
                            idxtype = "CL";
                        }
                        sb.Append($"Index Name ({idxtype}): <strong>{myIndex.Name}</strong> (");
                        //Console.Write($"Index Name: {myIndex.Name} {myIndex.IndexType.ToString()} ");
                        
                        foreach (IndexedColumn item in myIndex.IndexedColumns)
                        {
                            
                            string dir = item.Descending ? "desc" : "" ;
                            //Console.Write($" {item.ToString()} {dir} " );
                            sb.Append($"{item.Name.ToString()} {dir}, ");
                        }
                        sb.AppendLine(")");
                        //StringCollection indexScripts = myIndex.Script(); //scriptOptions
                        //foreach (string script in indexScripts)
                        //{
                        //    Console.WriteLine(script.ToString());
                        //    sb.AppendLine(script.ToString());
                        //}



                    }
                }

            }


            if (sb.ToString().Length < 1)
            {
                sb.AppendLine($"Table: <strong>{tableName}</strong>" + " has no index");
            }



            return sb.ToString().Replace(", )",")");


        }


    }
}
