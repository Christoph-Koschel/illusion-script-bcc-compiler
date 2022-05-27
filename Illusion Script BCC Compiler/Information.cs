using System;

namespace IllusionScript.Compiler.BCC
{
    public class Information
    {
        public static string getLibVersion()
        {
            return "0." + Math.Round((decimal)Enum.GetNames(typeof(KeywordCollection)).Length / 10);
        }
    }
}