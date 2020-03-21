using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TrojanShell.Model
{
    [Serializable]
    public class Configuration
    {
        public List<Server> configs;

        public int index;
        public bool global;
        public bool enabled;
        public bool shareOverLan;
        public bool isDefault;
        public int localPort;
        public int corePort;
        public string pacUrl;
        public bool useOnlinePac;
        public bool autoCheckUpdate;
        public bool isVerboseLogging;
        public HotkeyConfig hotkey;
        public List<SubscribeConfig> subscribes;

        private static readonly string CONFIG_FILE = Global.ProcessName + ".json";

        public Server GetCurrentServer()
        {
            if (index >= 0 && index < configs.Count)
                return configs[index];
            else
                return GetDefaultServer();
        }

        public static void CheckServerObject(Server server)
        {
            CheckPort(server.server_port);
            CheckServer(server.server);
        }

        public static Configuration Load()
        {
            try
            {
                string configContent = File.ReadAllText(CONFIG_FILE, Encoding.UTF8);
                Configuration config = Utils.DeSerializeJsonObject<Configuration>(configContent);
                config.isDefault = false;
                if (config.localPort == 0)
                    config.localPort = Utils.GetRandomPort(1080);
                if (config.hotkey == null)
                    config.hotkey = new HotkeyConfig();
                if (config.subscribes == null)
                    config.subscribes = new List<SubscribeConfig>();
                return config;
            }
            catch (Exception e)
            {
                if (!(e is FileNotFoundException))
                    Logging.LogUsefulException(e);
                var randport = Utils.GetRandomPort(1080);
                return new Configuration
                {
                    isDefault = true,
                    localPort = randport,
                    corePort = Utils.GetRandomPort(randport+1),
                    autoCheckUpdate = true,
                    useOnlinePac = false,
                    configs = new List<Server>
                    {
                        GetDefaultServer()
                    },
                    hotkey = new HotkeyConfig(),
                    subscribes = new List<SubscribeConfig>()
                };
            }
        }

        public static async Task<Configuration> LoadAsync()
        {
            try
            {
                string configContent = File.ReadAllText(CONFIG_FILE, Encoding.UTF8);
                Configuration config = await Utils.DeSerializeJsonObjectAsync<Configuration>(configContent);
                config.isDefault = false;
                if (config.localPort == 0)
                    config.localPort = Utils.GetRandomPort(1080);
                return config;
            }
            catch (Exception e)
            {
                if (!(e is FileNotFoundException))
                    Logging.LogUsefulException(e);
                var randport = Utils.GetRandomPort(1080);
                return new Configuration
                {
                    isDefault = true,
                    localPort = randport,
                    corePort = Utils.GetRandomPort(randport+1),
                    useOnlinePac = false,
                    autoCheckUpdate = true,
                    configs = new List<Server>
                    {
                        GetDefaultServer()
                    }
                };
            }
        }

        public static Server GetDefaultServer()
        {
            return new Server {server = "example.com", server_port = 443, remarks = I18N.GetString("New server")};
        }

        private static object configWriteLock = new object();
        public static void Save(Configuration config)
        {
            config.isDefault = false;
            try
            {
                lock (configWriteLock)
                {
                    File.WriteAllText(CONFIG_FILE,config.SerializeToJson());
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
            }
        }

        // public static async Task SaveAsync(Configuration config)
        // {
        //     config.isDefault = false;
        //     try
        //     {
        //         using (StreamWriter sw = new StreamWriter(File.Open(CONFIG_FILE, FileMode.Create)))
        //         {
        //             await sw.WriteAsync(await config.SerializeToJsonAsync());
        //             await sw.FlushAsync();
        //         }
        //     }
        //     catch (IOException e)
        //     {
        //         Console.Error.WriteLine(e);
        //     }
        // }


        private static void Assert(bool condition)
        {
            if (!condition)
                throw new Exception(I18N.GetString("assertion failure"));
        }

        public static void CheckPort(int port)
        {
            if (port <= 0 || port > 65535)
                throw new ArgumentException(I18N.GetString("Port out of range"));
        }

        public static void CheckLocalPort(int port)
        {
            CheckPort(port);
            if (port == 8123)
                throw new ArgumentException(I18N.GetString("Port can't be 8123"));
        }


        public static void CheckServer(string server)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentException(I18N.GetString("Server can not be blank"));
        }
    }
}
