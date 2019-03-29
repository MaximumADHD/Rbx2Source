namespace Rbx2Source.Compiler
{
    public class UtilParameter
    {
        public readonly string Name;
        public readonly string Value;

        public UtilParameter(string name = "", string value = "")
        {
            Name = name;
            Value = value;
        }

        private static string inQuotes(string str)
        {
            return '"' + str + '"';
        }

        public override string ToString()
        {
            string result = "";

            if (Name.Length > 0)
                result += "-" + Name;

            if (Value.Length > 0)
                result += ' ' + inQuotes(Value);

            return result;
        }
    }
}
