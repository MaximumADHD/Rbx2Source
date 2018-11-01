using Rbx2Source.Coordinates;

namespace Rbx2Source.Reflection
{
    // These are all blank classes that inherit directly from other classes.
    // Its required for the reflection to work correctly.

    class Accessory : Accoutrement { }
    class BlockMesh : BevelMesh { }
    class CharacterAppearance : Instance { }
    class CylinderMesh : BevelMesh { }
    class Folder : Instance { }
    class Hat : Accoutrement { }
    class Tool : Instance { }

    // Value classes
    class NumberValue  : ValueBase<double>  { };
    class StringValue  : ValueBase<string>  { };
    class Vector3Value : ValueBase<Vector3> { };
}
