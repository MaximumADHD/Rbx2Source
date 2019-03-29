namespace Rbx2Source.Coordinates
{
    public abstract class BaseCoordinates : IBaseCoordinates
    {
        private static string format = "0.000000";
        protected abstract string ToStudioMdlString_Impl(bool excludeZ = false);

        protected string[] truncate(params float[] vals)
        {
            string[] result = new string[vals.Length];

            for (int i = 0; i < vals.Length; i++)
            {
                string value = vals[i].ToString(format, Rbx2Source.NormalParse);
                if (value.ToLower() == "nan") // oh.
                    value = format;

                result[i] = value;
            }
            
            return result;
        }
        
        public string ToStudioMdlString(bool excludeZ = false)
        {
            return ToStudioMdlString_Impl(excludeZ);
        }
    }
}
