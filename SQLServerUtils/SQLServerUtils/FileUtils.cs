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

            Table aTable = dbObjects.Tables[tableName.ToLower()];

            if (aTable != null)
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
            else
            {
                foreach (Table tblItem in dbObjects.Tables)
                {
                    if (tblItem.Name.ToLower().Equals(tableName.ToLower()))
                    {
                        Console.WriteLine("table found: " + tableName.ToLower() + " but was not able to selected");
                    }
                    else
                    {
                        Console.WriteLine(tableName + " not found!!");
                    }
                }
                sb.AppendLine(tableName + " not found");
            }


            //Scripter scripter = new Scripter(myServer);
            //Database red = myServer.Databases["red"];

            //Urn[] DatabaseURNs = new Urn[] { red.Urn };
            //StringCollection scriptCollection = scripter.Script(DatabaseURNs);



            //foreach (string script in scriptCollection)
            //{
            //    sb.Append(script);
            //}

            //Console.WriteLine(sb.ToString());

            return sb.ToString();
          

        }






    }
}
