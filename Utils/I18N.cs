using System.Collections.Generic;
using TrojanShell.Properties;

namespace TrojanShell
{
    public class I18N
    {
        protected static Dictionary<string, string> Strings;

        public static readonly bool SC;
        public static readonly bool TC;
        public static readonly bool JP;

        public static readonly string CurrentCulture;

        static I18N()
        {
            Strings = new Dictionary<string, string>();

#if DEBUG
            CurrentCulture = "zh-cn";
#else
            CurrentCulture = System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag.ToLowerInvariant();
#endif

            if (CurrentCulture.StartsWith("zh-hans") || CurrentCulture == "zh-cn" || CurrentCulture == "zh-sg")
                SC = true;
            else if (CurrentCulture.StartsWith("zh-hant") || CurrentCulture == "zh-tw" || CurrentCulture == "zh-hk")
                TC = true;
            else if (CurrentCulture.StartsWith("ja"))
                JP = true;

            if (SC)
            {
                var lines = Resources.cn.Split('\r','\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("#")) continue;

                    var kv = line.Split('=');
                    if (kv.Length == 2) Strings[kv[0]] = kv[1];
                }
            }
            else if (TC || JP)
            {
                var lines = Resources.tc.Split('\r', '\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("#")) continue;

                    var kv = line.Split('=');
                    if (kv.Length == 2) Strings[kv[0]] = kv[1];
                }
            }
        }

        public static string GetString(string key)
        {
            return Strings.ContainsKey(key) ? Strings[key] : key;
        }

        public static string GetString(string format,params object[] args)
        {
            return Strings.ContainsKey(format) ? string.Format(Strings[format], args) : string.Format(format, args);
        }
    }
}
