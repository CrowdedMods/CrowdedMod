using System.Linq;
using CrowdedMod.Extensions;
using Hazel;
using Reactor;
using Reactor.Networking;

namespace CrowdedMod.Net;

[RegisterCustomRpc((uint) CustomRpcCalls.VotingComplete)]
public class VotingCompleteRpc : CustomRpc<CrowdedModPlugin, MeetingHud, VotingCompleteRpc.Data>
{
    public VotingCompleteRpc(CrowdedModPlugin plugin, uint id) : base(plugin, id)
    {
    }

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
        
    public override void Write(MessageWriter writer, Data data)
    {
        // worst way can do
        writer.WriteBytesAndSize(data.states.Select(x => x.VoterId).ToArray());
        writer.WriteBytesAndSize(data.states.Select(x => x.VotedForId).ToArray());
            
        writer.Write(data.exiled.PlayerId);
        writer.Write(data.tie);
    }

    public override Data Read(MessageReader reader)
    {
        var voterStates = Enumerable.Zip(reader.ReadBytesAndSize(), reader.ReadBytesAndSize(), 
            (x, y) => new MeetingHud.VoterState {VoterId = x, VotedForId = y}).ToArray();
            
        return new Data(
            voterStates,
            GameData.Instance.GetPlayerById(reader.ReadByte()),
            reader.ReadBoolean()
        );
    }

    public override void Handle(MeetingHud hud, Data data)
    {
        hud.CustomVotingComplete(data.states, data.exiled, data.tie);
    }

    public readonly struct Data
    {
        public readonly MeetingHud.VoterState[] states;
        public readonly GameData.PlayerInfo exiled;
        public readonly bool tie;

        public Data(MeetingHud.VoterState[] states, GameData.PlayerInfo exiled, bool tie)
        {
            this.states = states;
            this.exiled = exiled;
            this.tie = tie;
        }
    }
}