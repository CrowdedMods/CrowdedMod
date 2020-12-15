namespace CrowdedMod
{
    public class CustomServerInfo
    {
        public string name;
        public string ip;
        public int port;

        public CustomServerInfo(string name, string ip, int port)
        {
            this.name = name;
            this.ip = ip;
            this.port = port;
        }

        public override string ToString()
        {
            return $"{name} ({ip}:{port})";
        }
    }
}
