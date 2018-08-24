namespace Rbx2Source.Reflection
{
    // These are all blank classes that inherit directly from other classes.
    // Its required for the reflection to work correctly.

    class Accessory : Accoutrement { }
    class BlockMesh : BevelMesh { }
    class CylinderMesh : BevelMesh { }
    class Folder : Instance { }
    class Hat : Accoutrement { }
    class Tool : Instance { }
    class CharacterAppearance : Instance { }
    class Clothing : CharacterAppearance { }
}
