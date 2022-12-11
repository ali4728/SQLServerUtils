using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SQLServerUtils
{
    public class TSql_Parser
    {
        public string ExecutePlain(string p_contents)
        {
            string contents = p_contents;
            IList<ParseError> errors = null;
            string formattedSQL = "";
            using (StringReader rdr = new StringReader(contents))
            {
                TSql140Parser parser = new TSql140Parser(true);
                TSqlFragment tree = parser.Parse(rdr, out errors);

                foreach (ParseError err in errors) { Console.WriteLine(err.Message); }

                Sql120ScriptGenerator scrGen = new Sql120ScriptGenerator();
                scrGen.GenerateScript(tree, out formattedSQL);

            }

            formattedSQL = "<pre class=\"prettyprint lang-sql linenums\">" + formattedSQL + "</pre>";

            return formattedSQL;
        }


        public string Execute(string p_contents)
        {
            string contents = p_contents;

            StringBuilder htmlRetVal = new StringBuilder();
            int linecount = 0;
            

            IList<ParseError> errors = null;
            List<string> kw = new List<string> { "alter", "quoted_identifier", "ansi_nulls", "bulk", "go", "use", "select", "from", "where", "order", "group", "by", "insert", "update", "delete", "merge", "into", "having", "exec", "with", "declare", "create", "function", "procedure", "return", "returns", "begin", "end", "as", "if", "while", "for", "set", "try", "catch", "drop", "table", "view", "union", "all", "case", "then", "when", "cross", "apply", "outer", "inner", "left", "join", "and", "or", "on", "else", "raiserror", "throw", "tran", "commit", "rollback", "pivot", "unpivot", "for", "cursor", "open", "is", "in", "not", "null", "over", "partition", "using", "values", "varchar", "tinyint", "integer", "int", "bigint", "sysname", "date", "datetime", "datetime2", "bit", "numeric", "char", "decimal", "varbinary", "execute", "fetch", "nvarchar", "revert", "nocount", "exists", "uniqueidentifier", "output", "index", "raiserror", "print", "on", "off", "nolock", "open", "close", "deallocate", "goto", "image", "ntext", "top", "truncate" };
            List<string> kw2 = new List<string> { "sum", "avg", "min", "max", "count", "row_number", "rank", "isnull", "coalesce", "nullif", "getdate", "getutcdate", "getdate", "day", "month", "year", "len", "trim", "stuff", "datepart", "eomonth", "suser_sname", "cast", "convert", "db_name", "dateadd", "datediff" };
            List<string> kw3 = new List<string> { "asciistringliteral" };
            
            using (StringReader rdr = new StringReader(contents))
            {
                TSql140Parser parser = new TSql140Parser(true);
            
                TSqlFragment tree = parser.Parse(rdr, out errors);

                foreach (ParseError err in errors)
                {
                    Console.WriteLine(err.Message);

                }            

                string htmltop = @"<div id=""sqlcode"">
<ol class=""olclass"">
    <li><code class=""line"">";

                string htmlbottom = @"</code></li>
</ol>
</div>";
                var tok = tree.ScriptTokenStream;
                string template = "<b style='color:blue'>{0}</b>";
                string magenta = "<span style='color:magenta'>{0}</span>";
                string comment = "<span style='color:green'>{0}</span>";
                string red = "<span style='color:red'>{0}</span>";
                string gray = "<span style='color:gray'>{0}</span>";
                string whitespace = "<span style='color:gray'>{0}</span>";

                
                htmlRetVal.AppendLine(htmltop);


                foreach (var item in tok)
                {
                   
                    string tt = item.TokenType.ToString().ToLower().Trim();
                    string txt = "QWERTYUIO8";
                    string origstr = "QWERTYUIO8";




                    if (item.Text != null)
                    {
                        txt = item.Text.Trim().ToLower();
                        origstr = item.Text.Trim();

                    }

                    if (kw.Contains(txt))
                    {
                        string fmt = string.Format(template, txt);                       
                        htmlRetVal.Append(fmt.ToUpper());
                    }
                    else if (kw2.Contains(txt))
                    {
                        string fmt = string.Format(magenta, txt);                       
                        htmlRetVal.Append(fmt.ToUpper());
                    }
                    else if (kw3.Contains(tt) && !txt.Equals("QWERTYUIO8"))
                    {
                        string fmt = string.Format(red, origstr);                      
                        htmlRetVal.Append(fmt);
                    }
                    else if (item.TokenType == TSqlTokenType.UnicodeStringLiteral && !txt.Equals("QWERTYUIO8"))
                    {
                        string fmt = string.Format(red, origstr);                      
                        htmlRetVal.Append(fmt);
                    }
                    else if (item.Text != null && item.Text.Equals("\r\n"))
                    {
                        linecount++;                      
                        htmlRetVal.AppendLine("</code></li><li><code class=\"line\">");
                    }
                    else if (item.Text != null && item.TokenType == TSqlTokenType.WhiteSpace && item.Text.Length == 1)
                    {
                        char[] ca = item.Text.ToCharArray();
                        bool newline = false;
                        for (int i = 0; i < ca.Length; i++)
                        {
                            int kk = (int)ca[i];
                            if (kk == 10 || kk == 13)
                            {
                                linecount++;
                                htmlRetVal.AppendLine("</code></li><li><code class=\"line\">");
                                newline = true;
                            }
                        }

                        if (!newline)
                        {
                            string fmt = string.Format(whitespace, ParseWhiteSpace(item.Text));
                            htmlRetVal.Append(fmt);
                        }
                    }
                    else if (item.Text != null && item.TokenType == TSqlTokenType.WhiteSpace)
                    {
                        string fmt = string.Format(whitespace, ParseWhiteSpace(item.Text));                      
                        htmlRetVal.Append(fmt);
                    }
                    else if (item.TokenType == TSqlTokenType.MultilineComment)
                    {

                        string mls = origstr.Replace("\t", "    ");
                        mls = mls.Replace(" ", "&nbsp;");

                        string[] mlsary = mls.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        foreach (string it in mlsary)
                        {
                            string fmt = string.Format(comment, it);                         
                            htmlRetVal.Append(fmt);
                            if (!it.Contains("*/")) // dont put new line after the end of multi line comment, it is taken care of by the new line 
                            {
                              
                                htmlRetVal.AppendLine("</code></li><li><code class=\"line\">");
                                linecount++;
                            }
                        }

                    }
                    else if (item.TokenType == TSqlTokenType.SingleLineComment)
                    {
                        string mls = origstr.Replace("\t", "    ");
                        mls = mls.Replace(" ", "&nbsp;");

                        string fmt = string.Format(comment, mls);
                       
                        htmlRetVal.Append(fmt);
                    

                    }                   
                    else
                    {                       
                        htmlRetVal.Append(item.Text);
                    }
                }

              
                htmlRetVal.AppendLine(htmlbottom);



            }

          

            return htmlRetVal.ToString();
        }



        public string ExecuteTable(string p_contents)
        {
            string contents = p_contents;

            StringBuilder htmlRetVal = new StringBuilder();
            int linecount = 0;


            IList<ParseError> errors = null;
            List<string> kw = new List<string> { "alter", "quoted_identifier", "ansi_nulls", "bulk", "go", "use", "select", "from", "where", "order", "group", "by", "insert", "update", "delete", "merge", "into", "having", "exec", "with", "declare", "create", "function", "procedure", "return", "returns", "begin", "end", "as", "if", "while", "for", "set", "try", "catch", "drop", "table", "view", "union", "all", "case", "then", "when", "cross", "apply", "outer", "inner", "left", "join", "and", "or", "on", "else", "raiserror", "throw", "tran", "commit", "rollback", "pivot", "unpivot", "for", "cursor", "open", "is", "in", "not", "null", "over", "partition", "using", "values", "varchar", "tinyint", "integer", "int", "bigint", "sysname", "date", "datetime", "datetime2", "bit", "numeric", "char", "decimal", "varbinary", "execute", "fetch", "nvarchar", "revert", "nocount", "exists", "uniqueidentifier", "output", "index", "raiserror", "print", "on", "off", "nolock", "open", "close", "deallocate", "goto", "image", "ntext", "top", "truncate" };
            List<string> kw2 = new List<string> { "sum", "avg", "min", "max", "count", "row_number", "rank", "isnull", "coalesce", "nullif", "getdate", "getutcdate", "getdate", "day", "month", "year", "len", "trim", "stuff", "datepart", "eomonth", "suser_sname", "cast", "convert", "db_name", "dateadd", "datediff" };
            List<string> kw3 = new List<string> { "asciistringliteral" };

            using (StringReader rdr = new StringReader(contents))
            {
                TSql140Parser parser = new TSql140Parser(true);

                TSqlFragment tree = parser.Parse(rdr, out errors);

                foreach (ParseError err in errors)
                {
                    Console.WriteLine(err.Message);

                }
                int lctr = 1;
                string htmltop = @"<div id=""sqlcode"">
<table class=""tblclass""><tbody>
    <tr><td class=""blob-num"" data-line-number=""" + lctr.ToString() + "\"></td><td class=\"blob-code blob-code-inner\">";

                string htmlbottom = @" </td></tr>
</tbody></table>
</div>";
                var tok = tree.ScriptTokenStream;
                string template = "<b style='color:blue'>{0}</b>";
                string magenta = "<span style='color:magenta'>{0}</span>";
                string comment = "<span style='color:green'>{0}</span>";
                string red = "<span style='color:red'>{0}</span>";
                string gray = "<span style='color:gray'>{0}</span>";
                string whitespace = "<span style='color:gray'>{0}</span>";


                htmlRetVal.AppendLine(htmltop);


                foreach (var item in tok)
                {
                    
                    string tt = item.TokenType.ToString().ToLower().Trim();
                    string txt = "QWERTYUIO8";
                    string origstr = "QWERTYUIO8";




                    if (item.Text != null)
                    {
                        txt = item.Text.Trim().ToLower();
                        origstr = item.Text.Trim();

                    }

                    if (kw.Contains(txt))
                    {
                        string fmt = string.Format(template, txt);
                        htmlRetVal.Append(fmt.ToUpper());
                    }
                    else if (kw2.Contains(txt))
                    {
                        string fmt = string.Format(magenta, txt);
                        htmlRetVal.Append(fmt.ToUpper());
                    }
                    else if (kw3.Contains(tt) && !txt.Equals("QWERTYUIO8"))
                    {
                        string fmt = string.Format(red, origstr);
                        htmlRetVal.Append(fmt);
                    }
                    else if (item.TokenType == TSqlTokenType.UnicodeStringLiteral && !txt.Equals("QWERTYUIO8"))
                    {
                        string fmt = string.Format(red, origstr);
                        htmlRetVal.Append(fmt);
                    }
                    else if (item.Text != null && item.Text.Equals("\r\n"))
                    {
                        linecount++;
                        lctr++;
                        htmlRetVal.AppendLine("</td></tr><tr><td class=\"blob-num\" data-line-number=\"" + lctr.ToString() + "\"></td><td class=\"blob-code blob-code-inner\">");

                    }
                    else if (item.Text != null && item.TokenType == TSqlTokenType.WhiteSpace && item.Text.Length == 1)
                    {
                        char[] ca = item.Text.ToCharArray();
                        bool newline = false;
                        for (int i = 0; i < ca.Length; i++)
                        {
                            int kk = (int)ca[i];
                            if (kk == 10 || kk == 13)
                            {
                                linecount++;
                                lctr++;
                                htmlRetVal.AppendLine("</td></tr><tr><td class=\"blob-num\" data-line-number=\"" + lctr.ToString() + "\"></td><td class=\"blob-code blob-code-inner\">");
                                newline = true;
                            }
                        }

                        if (!newline)
                        {
                            string fmt = string.Format(whitespace, ParseWhiteSpace(item.Text));
                            htmlRetVal.Append(fmt);
                        }
                    }
                    else if (item.Text != null && item.TokenType == TSqlTokenType.WhiteSpace)
                    {
                        string fmt = string.Format(whitespace, ParseWhiteSpace(item.Text));
                        htmlRetVal.Append(fmt);
                    }
                    else if (item.TokenType == TSqlTokenType.MultilineComment)
                    {

                        string mls = origstr.Replace("\t", "    ");
                        mls = mls.Replace(" ", "&nbsp;");

                        string[] mlsary = mls.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        foreach (string it in mlsary)
                        {
                            string fmt = string.Format(comment, it);
                            htmlRetVal.Append(fmt);
                            if (!it.Contains("*/")) // dont put new line after the end of multi line comment, it is taken care of by the new line 
                            {

                                lctr++;
                                htmlRetVal.AppendLine("</td></tr><tr><td class=\"blob-num\" data-line-number=\"" + lctr.ToString() + "\"></td><td class=\"blob-code blob-code-inner\">");
                                linecount++;
                            }
                        }

                    }
                    else if (item.TokenType == TSqlTokenType.SingleLineComment)
                    {
                        string mls = origstr.Replace("\t", "    ");
                        mls = mls.Replace(" ", "&nbsp;");

                        string fmt = string.Format(comment, mls);

                        htmlRetVal.Append(fmt);


                    }
                    else
                    {
                        htmlRetVal.Append(item.Text);
                    }
                }


                htmlRetVal.AppendLine(htmlbottom);



            }



            return htmlRetVal.ToString();
        }



        public string ParseWhiteSpace(string ws)
        {

            if (ws == null || "".Equals(ws) || "\r\n".Equals(ws))
            {
                return ws;
            }

            ws = ws.Replace("\t", "    ");
            StringBuilder retval = new StringBuilder();

            for (int i = 0; i < ws.Length; i++)
            {
                retval.Append("&nbsp;");
            }

            return retval.ToString();
        }
    }
}
