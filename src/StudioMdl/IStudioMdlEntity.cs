using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rbx2Source.StudioMdl
{
    interface IStudioMdlEntity
    {
        void Write(StringWriter fileBuffer, IList rawArray, object rawEntity);
        string GroupName { get; }
    }
}
