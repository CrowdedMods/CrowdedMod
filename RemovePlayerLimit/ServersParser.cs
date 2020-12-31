using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.IO;

namespace CrowdedMod
{
    public static class ServersParser
    {
        public enum ParseResult : byte
        {
            Success = 0,
            Comment = 1,
            FileNotFound = 2,
            InvalidData = 3
        }
        private static ushort DefaultPort = 22023;
        public static List<CustomServerInfo> servers = new List<CustomServerInfo>();
        /// <summary>
        /// Parses a hardcoded `servers.txt` file to get server list, which will be stored in <see cref="servers"/>
        /// </summary>
        public static void Parse()
        {
            servers.Clear(); // clear if reparsing
            StreamReader r;
            try
            {
                r = File.OpenText("servers.txt");
            } catch (Exception e)
            {
                RemovePlayerLimitPlugin.Logger.LogError(e);
                GenericPatches.parseStatus = ParseResult.FileNotFound;
                return;
            }
            string line;
            bool noServers = true;
            while((line = r.ReadLine()) != null)
            {
                var result = ParseLine(line, out var s);
                if (result == ParseResult.Success)
                {
                    noServers = false;
                    servers.Add(s);
                } else if (result != ParseResult.Comment)
                {
                    GenericPatches.parseStatus = result;
                    noServers = false;
                    break;
                }
            }
            if (noServers) GenericPatches.parseStatus = ParseResult.Comment;
        }
        /// <summary>
        /// Parses a server line. Throws <see cref="System.Exception"/> if wrong data has been provided
        /// </summary>
        /// <param name="line">Line to parse</param>
        /// <param name="server">Parsed <see cref="CustomServerInfo"/>. <see langword="null"/> if line is a comment</param>
        /// <returns><see cref="ParseResult"/></returns>
        static private ParseResult ParseLine(string line, out CustomServerInfo server)
        {
            server = null;
            line = line.Trim();
            if (line.StartsWith("#")) return ParseResult.Comment;
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
                RemovePlayerLimitPlugin.Logger.LogError("Invalid `servers.txt` data. See https://github.com/CrowdedMods/CrowdedMod/wiki/servers.txt");
                return ParseResult.InvalidData;
            }
            return ParseResult.Success;
        }
    }
}
