using RobloxFiles.DataTypes;

namespace Rbx2Source.Assembler
{
    public class AvatarScaleRules
    {
        public Vector3 Head;
        public Vector3 UpperTorso;
        public Vector3 LowerTorso;

        public Vector3 LeftUpperArm;
        public Vector3 LeftLowerArm;
        public Vector3 LeftHand;

        public Vector3 LeftUpperLeg;
        public Vector3 LeftLowerLeg;
        public Vector3 LeftFoot;

        public Vector3 RightUpperArm;
        public Vector3 RightLowerArm;
        public Vector3 RightHand;

        public Vector3 RightUpperLeg;
        public Vector3 RightLowerLeg;
        public Vector3 RightFoot;

        public Vector3 this[string limbName]
        {
            get
            {
                try
                {
                    var type = GetType();
                    var fieldInfo = type.GetField(limbName);
                    return fieldInfo.GetValue(this) as Vector3;
                }
                catch
                {
                    return new Vector3(1, 1, 1);
                }
            }
        }
    }
}
