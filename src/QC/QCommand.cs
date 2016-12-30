using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rbx2Source.QC
{
    struct QCParam
    {
        public string Name;
        public List<string> Values;
    }

    class QCommand
    {
        public string Name;
        protected List<string> Options = new List<string>();
        public List<QCParam> Params = new List<QCParam>();
        public List<QCommand> SubCommands = new List<QCommand>();

        private static string inQuotes(string s)
        {
            return '"' + s + '"';
        }

        public void AddBasicOption(string value, bool putInQuotes = true)
        {
            if (putInQuotes) value = inQuotes(value);
            Options.Add(value);
        }

        public void AddParameter(string name, params object[] values)
        {
            QCParam param = new QCParam();
            param.Name = name;
            param.Values = new List<string>();
            foreach (object value in values)
                param.Values.Add(value.ToString());

            Params.Add(param);
        }

        public void Write(StringWriter buffer, int stack = 0)
        {
            string tab = "";
            for (int i = 0; i < stack; i++)
                tab += "\t";

            List<string> lineBuff = new List<string>();
            lineBuff.Add(tab + "$" + Name.ToLower());
            foreach (string option in Options)
                lineBuff.Add(option);

            Rbx2Source.Print("Writing Command {0}", string.Join(" ", lineBuff.ToArray()));

            bool hasParameters = (Params.Count > 0);

            if (hasParameters)
                lineBuff.Add(tab + "{");

            string firstLine = string.Join(" ",lineBuff.ToArray());
            buffer.WriteLine(tab + firstLine);

            if (hasParameters)
            {
                foreach (QCParam param in Params)
                {
                    List<string> paramBuff = new List<string>();
                    paramBuff.Add(param.Name);
                    foreach (string value in param.Values)
                        paramBuff.Add(value);

                    string result = string.Join(" ", paramBuff);
                    buffer.WriteLine(tab + "\t" + result);
                }
                buffer.WriteLine("}\n");
            }
            else
            {
                foreach (QCommand subCommand in SubCommands)
                    subCommand.Write(buffer, stack + 1);
            }
        }

        public QCommand(string name)
        {
            Name = name;
        }

        public QCommand(string name, string param, bool paramInQuotes = true)
        {
            Name = name;
            AddBasicOption(param, paramInQuotes);
        }

        public QCommand(string name, params object[] options)
        {
            Name = name;
            foreach (object option in options)
                AddBasicOption(option.ToString());
        }
    }
}