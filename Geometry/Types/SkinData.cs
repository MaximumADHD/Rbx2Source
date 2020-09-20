using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rbx2Source.Geometry
{
    public class SkinData
    {
        public int FacesBegin;
        public int FacesLength;

        public int VertsBegin;
        public int VertsLength;

        public int NumBones;
        public short[] BoneIndexTree;
    }
}
