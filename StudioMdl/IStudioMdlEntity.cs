using System.Collections.Generic;
using System.IO;

namespace Rbx2Source.StudioMdl
{
    public interface IStudioMdlEntity<T>
    {
        string GroupName { get; }
        void WriteStudioMdl(StringWriter buffer, List<T> group);
    }
}
