using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rbx2Source.Compiler
{
    public class ThirdPartyUtility
    {
        private string appPath;
        private HashSet<UtilParameter> parameters;

        public ThirdPartyUtility(string path)
        {
            appPath = path;
            parameters = new HashSet<UtilParameter>();
        }

        public void AddParameter(UtilParameter parameter)
        {
            parameters.Add(parameter);
        }
		
		public void AddParameter(string name = "", string value = "")
		{
			var param = new UtilParameter(name, value);
            AddParameter(param);
		}

        public void AddFile(string filePath)
        {
            var file = new UtilParameter("", filePath);
            AddParameter(file);
        }

        public Process Run()
        {
            var paramStrings = parameters
                .Select(param => param.ToString())
                .ToArray();

            ProcessStartInfo info = new ProcessStartInfo()
            {
                Arguments = string.Join(" ", paramStrings),
                FileName = appPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            return Process.Start(info);
        }

        public Task RunWithOutput()
        {
            Process process = Run();
            StreamReader output = process.StandardOutput;

            Task runTask = Task.Run(() =>
            {
                while (true)
                {
                    Task<string> nextLineAsync = output.ReadLineAsync();
                    nextLineAsync.Wait(1000);

                    string nextLine = nextLineAsync.Result;
                    if (nextLine == null)
                        break;

                    Rbx2Source.Print(nextLine);
                }
            });

            return runTask;
        }
    }
}
