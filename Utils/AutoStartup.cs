using System;
using System.Linq;

namespace TrojanShell
{
    class AutoStartup
    {
        private const string KEY_NAME = "TrojanShell";
        public static bool Set(bool enabled)
        {
            try
            {
                var kNameWithHash = $"{KEY_NAME}_{Global.PathHash}";
                var runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (runKey == null) return false;
                var runList = runKey.GetValueNames();
                if (enabled)
                {
                    if(runList.Any(c=>c == KEY_NAME) && !runKey.GetValue(KEY_NAME).ToString().Equals(Global.ProcessPath))
                        runKey.SetValue(kNameWithHash, Global.ProcessPath);
                    else
                        runKey.SetValue(KEY_NAME, Global.ProcessPath);
                }
                else
                {
                    if (runList.Any(c => c == kNameWithHash))
                        runKey.DeleteValue(kNameWithHash);
                    else
                        runKey.DeleteValue(KEY_NAME);
                }
                runKey.Close();
                return true;
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return false;
            }
        }

        public static bool Check()
        {
            try
            {
                var kNameWithHash = $"{KEY_NAME}_{Global.PathHash}";
                var runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                if (runKey == null) return false;
                //string[] runList = runKey.GetValueNames();
                string fName = runKey.GetValue(KEY_NAME)?.ToString();
                string pName = runKey.GetValue(kNameWithHash)?.ToString();
                runKey.Close();
                if (pName != null && pName.Equals(Global.ProcessPath,StringComparison.OrdinalIgnoreCase))
                    return true;
                if (fName != null && fName.Equals(Global.ProcessPath, StringComparison.OrdinalIgnoreCase))
                    return true;
                //if (runList == null || runList.Length == 0) return false;
                //foreach (string item in runList)
                //{
                //    if (item.Equals(KEY_NAME))
                //        return true;
                //}
                return false;
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return false;
            }
        }
    }
}
