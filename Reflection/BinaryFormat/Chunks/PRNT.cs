namespace Rbx2Source.Reflection.BinaryFormat
{
    public class PRNT
    {
        public readonly byte Format;
        public readonly int NumRelations;

        public readonly int[] ChildrenIds;
        public readonly int[] ParentIds;

        public PRNT(RobloxBinaryChunk chunk)
        {
            using (RobloxBinaryReader reader = chunk.GetReader("PRNT"))
            {
                Format = reader.ReadByte();
                NumRelations = reader.ReadInt32();

                ChildrenIds = reader.ReadInstanceIds(NumRelations);
                ParentIds = reader.ReadInstanceIds(NumRelations);
            }
        }

        public void Assemble(RobloxBinaryFile file)
        {
            for (int i = 0; i < NumRelations; i++)
            {
                int childId = ChildrenIds[i];
                int parentId = ParentIds[i];

                FileInstance child = file.Instances[childId];
                FileInstance parent = null;

                if (parentId >= 0)
                    parent = file.Instances[parentId];
                else
                    parent = file.Contents;

                child.Parent = parent;
            }
        }
    }
}
