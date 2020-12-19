using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.IO;

namespace CrowdedMod
{
    public static class ServersParser
    {
        private static ushort DefaultPort = 22023;
        public static List<CustomServerInfo> servers = new List<CustomServerInfo>();
        /// <summary>
        /// Parses a hardcoded `servers.txt` file to get server list, which will be stored in <see cref="servers"/>
        /// </summary>
        public static void Parse()
        {
            servers.Clear(); // clear if reparsing

            StreamReader r = File.OpenText("servers.txt"); 
            // no try catch here since i think its error message is clear enough
            string line;
            while((line = r.ReadLine()) != null)
            {
                if (ParseLine(line, out var s))
                {
                    servers.Add(s);
                }
            }
        }
        /// <summary>
        /// Parses a server line. Throws <see cref="System.Exception"/> if wrong data has been provided
        /// </summary>
        /// <param name="line">Line to parse</param>
        /// <param name="server">Parsed <see cref="CustomServerInfo"/>. <see langword="null"/> if line is a comment</param>
        /// <returns>False if line is a comment, otherwise true</returns>
        static private bool ParseLine(string line, out CustomServerInfo server)
        {
            server = null;
            line = line.Trim();
            if (line.StartsWith("#")) return false;
            try
            {
                string name;
                string[] ipdata;
                if(line.StartsWith("\""))
                {
                    int i = line.Substring(1).IndexOf('"');
                    name = line.Substring(1, i);
                    ipdata = line.Substring(i+3).Split(); // +2 due to indexes, +1 because we hope it has a space after
                } else
                {
                    string[] data = line.Split();
                    name = data[0];
                    ipdata = data.Skip(1).ToArray();
                }
                ushort port = DefaultPort;
                if(ipdata.Length == 1)
                {
                    ipdata = ipdata[0].Split(':');
                }
                if(ipdata.Length > 1) port = ushort.Parse(ipdata[1]);
                server = new CustomServerInfo(name, ipdata[0], port);
            } catch
            {
                throw new System.Exception("Invalid `servers.txt` data. See https://pastebin.com/GsusE0Wh as an example");
            }
            return true;
        }
    }
}
