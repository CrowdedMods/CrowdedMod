using CrowdedMod.Extensions;
using CrowdedMod.Patches;
using Hazel;
using Reactor;
using Reactor.Networking;

namespace CrowdedMod.Net
{
    [RegisterCustomRpc((uint) CustomRpcCalls.VotingComplete)]
    public class VotingCompleteRpc : CustomRpc<CrowdedModPlugin, MeetingHud, VotingCompleteRpc.Data>
    {
        public readonly struct Data
        {
            public readonly byte[] states;
            public readonly byte[] votes;
            public readonly byte exiled;

            public Data(byte[] states, byte[] votes, byte exiled)
            {
                this.states = states;
                this.votes = votes;
                this.exiled = exiled;
            }
        }

        public VotingCompleteRpc(CrowdedModPlugin plugin, uint id) : base(plugin, id)
        {
        }

        public override RpcLocalHandling LocalHandling { get; } = RpcLocalHandling.After;
        public override void Write(MessageWriter writer, Data data)
        {
            writer.WriteBytesAndSize(data.states);
            writer.WriteBytesAndSize(data.votes);
            writer.Write(data.exiled);
        }

        public override Data Read(MessageReader reader)
            => new Data(
                    reader.ReadBytesAndSize(),
                    reader.ReadBytesAndSize(),
                    reader.ReadByte()
                );

        public override void Handle(MeetingHud hud, Data data)
        {
            hud.CustomVotingComplete(
                data.states, 
                data.votes, 
                GameData.Instance.GetPlayerById(data.exiled), 
                data.exiled == byte.MaxValue
            );
        }
    }
}