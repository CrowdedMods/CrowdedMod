using Hazel;
using Reactor;
using Reactor.Networking;

namespace CrowdedMod.Net
{
    [RegisterCustomRpc((uint) CustomRpcCalls.SetColor)]
    public class SetColorRpc : PlayerCustomRpc<CrowdedModPlugin, byte>
    {
        public SetColorRpc(CrowdedModPlugin plugin, uint id) : base(plugin, id)
        {
        }

        public override RpcLocalHandling LocalHandling { get; } = RpcLocalHandling.After;

        public override void Write(MessageWriter writer, byte data)
            => writer.Write(data);

        public override byte Read(MessageReader reader)
            => reader.ReadByte();

        public override void Handle(PlayerControl sender, byte data)
            => sender.SetColor(data);
    }
}