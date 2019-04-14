using Rbx2Source.DataTypes;

namespace Rbx2Source.Reflection
{
    // These are all blank classes that inherit directly from other classes.
    // Its required for the reflection to work correctly.

    public class Accessory : Accoutrement { }
    public class BlockMesh : BevelMesh { }
    public class CharacterAppearance : Instance { }
    public class CylinderMesh : BevelMesh { }
    public class Folder : Instance { }
    public class Hat : Accoutrement { }
    public class Tool : Instance { }

    // Value classes
    public class NumberValue  : ValueBase<double>  { };
    public class StringValue  : ValueBase<string>  { };
    public class Vector3Value : ValueBase<Vector3> { };
}
