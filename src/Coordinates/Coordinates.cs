using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rbx2Source.Coordinates
{
    interface IBaseCoordinates
    {
        string ToStudioMdlString(bool excludeZ = false);
    }

    abstract class BaseCoordinates : IBaseCoordinates
    {
        /// <summary>
        /// Converts the floats into strings with 6 decimal places.
        /// This is a utility function more than anything else.
        /// </summary>
        /// <returns></returns>
        /// 
        private static string format = "0.000000";

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
        
        protected abstract string ToStudioMdlString_Impl(bool excludeZ = false);

        public string ToStudioMdlString(bool excludeZ = false)
        {
            return ToStudioMdlString_Impl(excludeZ);
        }
    }
}
