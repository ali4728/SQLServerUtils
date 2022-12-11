using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace SQLServerUtils
{
    public class FileUtils
    {

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
