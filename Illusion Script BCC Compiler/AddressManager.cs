using System;
using System.Collections.Generic;
using System.Linq;

namespace IllusionScript.Compiler.BCC
{
    public class AddressManager
    {
        private Dictionary<string, int[]> register;
        private int count = (int)Enum.GetValues(typeof(KeywordCollection)).Cast<KeywordCollection>().Max() + 1;

        public AddressManager()
        {
            register = new Dictionary<string, int[]>();
        }

        public int[] get(string name)
        {
            if (register.ContainsKey(name))
            {
                return register[name];
            }

            count++;
            int[] res;
            if (count <= 255)
            {
                res = new[] { count };
            }
            else
            {
                int rest = count;
                List<int> data = new List<int>();
                while (rest >= 255)
                {
                    data.Add(255);
                    rest -= 255;
                }

                if (rest != 0)
                {
                    data.Add(rest);
                }

                if (data.Count > 8)
                {
                    throw new Exception("Memory out of 8bit bounds");
                }

                res = data.ToArray();
            }

            register.Add(name, res);
            return res;
        }
    }
}