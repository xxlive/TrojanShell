using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TrojanShell
{
    public static class Trojan
    {
        #region Props

        public const string TROJAN_CORE = "trojan.exe";
        public const string TROJAN_CORE_WITHOUTEXT = "trojan";


        private static readonly Regex _regexVersion = new Regex(@"trojan\s+(\d+).(\d+).(\d+)", RegexOptions.IgnoreCase);

        public static bool CoreExsis => File.Exists(TROJAN_CORE);

        public static Version Version
        {
            get
            {
                if (!CoreExsis) return null;
                var run = Utils.StartProcess(TROJAN_CORE, "-v");
                if (string.IsNullOrEmpty(run)) return null;
                var match = _regexVersion.Match(run);
                return match.Success ? new Version(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value)) : null;
            }
        }
        #endregion

        #region Template

        public static string GenerateConf(Server svc, int listenPort, string listenAddr = "127.0.0.1")
        {

            var conf = new TrojanConfig
            {
                run_type = "client",
                local_addr = listenAddr,
                local_port = listenPort,
                remote_addr = svc.server,
                remote_port = svc.server_port,
                password = new []{svc.password},
                log_level = 1,
                ssl = new Ssl
                {
                    verify = true,
                    verify_hostname = true,
                    cert="",
                    cipher = "ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-AES256-SHA:ECDHE-ECDSA-AES128-SHA:ECDHE-RSA-AES128-SHA:ECDHE-RSA-AES256-SHA:DHE-RSA-AES128-SHA:DHE-RSA-AES256-SHA:AES128-SHA:AES256-SHA:DES-CBC3-SHA",
                    cipher_tls13 = "TLS_AES_128_GCM_SHA256:TLS_CHACHA20_POLY1305_SHA256:TLS_AES_256_GCM_SHA384",
                    sni = "",
                    alpn = new []{"h2","http/1.1"},
                    reuse_session = true,
                    session_ticket = false,
                    curves = ""
                },
                tcp = new Tcp
                {
                    no_delay = true,
                    keep_alive = true,
                    reuse_port = false,
                    fast_open = false,
                    fast_open_qlen = 20
                }
            };
            return conf.SerializeToJson();
        }

        #endregion
    }

    #region Config

    public class Server
    {
        public Server()
        {
        }

        public string ToURL()
        {
            //[offcial](https://github.com/trojan-gfw/trojan-url): trojan://password@remote_host:remote_port
            //west: trojan://密码@节点IP:端口?allowInsecure=1&peer=节点域名#节点备注
            return $"trojan://{password}@{server}:{server_port}?allowInsecure=0#{Uri.EscapeDataString(remarks)}";//默认用域名，验证证书
        }

        public static List<Server> GetServers(string urls)
        {
            var serverUrls = urls.Contains("trojan://") ? urls.Split('\r', '\n') : Encoding.UTF8.GetString(Convert.FromBase64String(urls)).Split('\r', '\n');

            var servers = new List<Server>();
            foreach (var serverUrl in serverUrls)
            {
                if (!serverUrl.StartsWith("trojan://", StringComparison.OrdinalIgnoreCase)) continue;
                if (TryParse(serverUrl, out var saba))
                    servers.Add(saba);
            }

            return servers;
        }

        public static bool TryParse(string input, out Server svc)
        {
            if (!input.StartsWith("trojan://", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid");
            try
            {
                var uri = new Uri(input);
                svc = new Server {server = uri.Host, server_port = uri.Port, password = uri.UserInfo};
                if (!string.IsNullOrEmpty(uri.Fragment)) svc.remarks = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'));
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    if (queryParameters["peer"] != null)
                        svc.server = queryParameters["peer"];
                }

                return true;
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                svc = null;
                return false;
            }
        }

        public string server { get; set; }
        public int server_port { get; set; }
        public string password { get; set; }
        public string remarks { get; set; }
        //public bool fast_open { get; set; }

        /// <summary>
        /// 分组，默认空
        /// </summary>
        public string group { get; set; } = "";

        public string FriendlyName()
        {
            if (string.IsNullOrEmpty(server))
            {
                return I18N.GetString("New server");
            }

            string serverStr;
            // CheckHostName() won't do a real DNS lookup
            var hostType = Uri.CheckHostName(server);

            switch (hostType)
            {
                case UriHostNameType.IPv6:
                    serverStr = $"[{server}]:{server_port}";
                    break;
                default:
                    // IPv4 and domain name
                    serverStr = $"{server}:{server_port}";
                    break;
            }

            return string.IsNullOrEmpty(remarks)
                ? serverStr
                : $"{remarks} ({serverStr})";
        }

        public override int GetHashCode()
        {
            return server.GetHashCode() ^ server_port;
        }

        public override bool Equals(object obj)
        {
            Server o2 = (Server) obj;
            return server == o2.server && server_port == o2.server_port;
        }
    }


    public class TrojanConfig
    {
        public string run_type { get; set; }
        public string local_addr { get; set; }
        public int local_port { get; set; }
        public string remote_addr { get; set; }
        public int remote_port { get; set; }
        public string[] password { get; set; }
        public int log_level { get; set; }
        public Ssl ssl { get; set; }
        public Tcp tcp { get; set; }
    }

    public class Ssl
    {
        public bool verify { get; set; }
        public bool verify_hostname { get; set; }
        public string cert { get; set; }
        public string cipher { get; set; }
        public string cipher_tls13 { get; set; }
        public string sni { get; set; }
        public string[] alpn { get; set; }
        public bool reuse_session { get; set; }
        public bool session_ticket { get; set; }
        public string curves { get; set; }
    }

    public class Tcp
    {
        public bool no_delay { get; set; }
        public bool keep_alive { get; set; }
        public bool reuse_port { get; set; }
        public bool fast_open { get; set; }
        public int fast_open_qlen { get; set; }
    }

    #endregion
}
