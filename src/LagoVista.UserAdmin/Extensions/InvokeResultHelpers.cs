using System.Collections.Generic;

namespace LagoVista.Core
{
    public static class InvokeResultHelpers
    {        

        public static KeyValuePair<string,string> ToKVP(this string value, string key)
        {
            return new KeyValuePair<string, string>(key, value);
        }
    }
}
