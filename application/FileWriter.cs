using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RobloxToSourceEngine
{
    // A version of the string writer which isn't so messy for my file writing stuff.
    class FileWriter : StringWriter
    {
        private string inQuotes(string line)
        {
            return "\"" + line + "\"";
        }
        public void WriteInBrackets(bool inCommand, params string[] lines)
        {
            string tab = "\t";
            if (inCommand)
            {
                this.Write("{");
            }
            else
            {
                this.WriteLine("{");
            }
            foreach (string line in lines)
            {
                this.AddLine(tab + line);
            }
            this.WriteLine("}");
        }

        public string FormatCommand(string command, params string[] args)
        {
            string line = "$" + command;
            foreach (string arg in args)
            {
                line = line + " " + inQuotes(arg);
            }
            return line;
        }

        public void WriteCommand(string command, params string[] args)
        {
            string line = FormatCommand(command, args);
            this.AddLine(line);
        }

        public void AddLine(string line)
        {
            line.Replace("/", "\\");
            line.Replace("'","\"");
            this.WriteLine(line);
        }

        public string Dump()
        {
            string contents = this.ToString();
            this.Close();
            return contents;
        }
    }
}
