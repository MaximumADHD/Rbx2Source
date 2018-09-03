using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rbx2Source.Compiler
{
    class UtilParameter
    {
        public string Name;
        public string Value;
        private static string inQuotes(string str)
        {
            return "\"" + str + "\"";
        }

        public override string ToString()
        {
            string result = "";
            if (Name != null)
                result += "-" + Name;

            if (Value != null)
                result += " \"" + Value + '"';

            return result;
        }

        public UtilParameter() { }

        public UtilParameter(string name)
        {
            Name = name;
        }

        public UtilParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static UtilParameter FilePush(string value)
        {
            UtilParameter parameter = new UtilParameter();
            parameter.Value = value;
            return parameter;
        }
    }

    class ThirdPartyUtility
    {

        private string appPath;
        private List<UtilParameter> parameters;
        public ThirdPartyUtility(string path)
        {
            appPath = path;
            parameters = new List<UtilParameter>();
        }

        public void AddParameter(UtilParameter parameter)
        {
            if (!parameters.Contains(parameter))
                parameters.Add(parameter);
        }
		
		public void AddParameter(string name)
		{
			UtilParameter parameter = new UtilParameter(name);
			parameters.Add(parameter);
		}
		
		public void AddParameter(string name, string val)
		{
			UtilParameter parameter = new UtilParameter(name,val);
            parameters.Add(parameter);
		}

        public Process RunSimple()
        {
            List<string> paramStrings = new List<string>();
            foreach (UtilParameter parameter in parameters)
                paramStrings.Add(parameter.ToString());

            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = string.Join(" ", paramStrings.ToArray());
            info.FileName = appPath;
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            return Process.Start(info);
        }

        public Task Run()
        {
            List<string> paramStrings = new List<string>();
            foreach (UtilParameter parameter in parameters)
                paramStrings.Add(parameter.ToString());

            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = string.Join(" ", paramStrings.ToArray());
            info.FileName = appPath;
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            Process process = Process.Start(info);
            StreamReader output = process.StandardOutput;
            Task runTask = Task.Run(() =>
            {
                while (true)
                {
                    Task<string> nextLineAsync = output.ReadLineAsync();
                    nextLineAsync.Wait(1000);
                    string nextLine = nextLineAsync.Result;
                    if (nextLine == null) break;
                    Rbx2Source.Print(nextLine);
                }
            });
            return runTask;
        }
    }
}
