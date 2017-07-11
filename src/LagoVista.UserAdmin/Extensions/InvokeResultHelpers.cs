using LagoVista.Core.Validation;
using System.Collections.Generic;

namespace LagoVista.Core
{
    public static class InvokeResultHelpers
    {        

        public static KeyValuePair<string,string> ToKVP(this string value, string key)
        {
            return new KeyValuePair<string, string>(key, value);
        }

        public static InvokeResult ToInvokeResult(this string error)
        {
            return InvokeResult.FromErrors(new ErrorMessage(error));
        }
    }
}
