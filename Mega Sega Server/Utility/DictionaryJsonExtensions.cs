using System;
using System.Web.Script.Serialization;

namespace Mega_Sega_Server.Utility
{
    public static class DictionaryJsonExtensions
    {
        public static String ToJson(this object obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        public static String ToJson(this object obj, int recursionDepth)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RecursionLimit = recursionDepth;
            return serializer.Serialize(obj);
        }
    }
}