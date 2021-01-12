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






    }
}
