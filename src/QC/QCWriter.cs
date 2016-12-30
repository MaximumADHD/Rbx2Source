using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rbx2Source.QC
{
    class QCWriter
    {
        private List<QCommand> commands = new List<QCommand>();

        public void AddCommand(QCommand command)
        {
            commands.Add(command);
        }

        public void WriteBasicCmd(string name, string param = "", bool paramInQuotes = false)
        {
            QCommand command = new QCommand(name, param, paramInQuotes);
            AddCommand(command);
        }

        public string BuildFile()
        {
            StringWriter buffer = new StringWriter();
            foreach (QCommand command in commands)
                command.Write(buffer);

            return buffer.ToString();
        }
    }
}
