using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rbx2Source.Assembler
{
    public interface IAssembler
    {
        AssemblerData Assemble(object metadata);
    }
}
