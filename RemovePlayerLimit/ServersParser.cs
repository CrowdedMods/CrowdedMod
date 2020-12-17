using System.Collections.Generic;
using Il2CppSystem.IO;

namespace CrowdedMod
{
    public static class ServersParser
    {
        public static List<CustomServerInfo> servers = new List<CustomServerInfo>();
        public static void Parse()
        {
            servers.Clear(); // it shouldn't be called twice, but safety ya know

            StreamReader r = File.OpenText("servers.txt"); // couldn't use runtime Newtonsoft.Json
            string line;
            while((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("#")) continue;
                string[] data = line.Split();
                servers.Add(new CustomServerInfo(data[0], data[1], int.Parse(data[2])));
            }
        }
    }
}
