using CrowdedMod.Net;
using Reactor.Networking.MethodRpc;

namespace CrowdedMod.Extensions;

public static class PlayerControlExtensions
{
    [MethodRpc((uint) CustomRpcCalls.SetColor)]
    public static void CustomRpcSetColor(this PlayerControl playerControl, byte colorId)
    {
        playerControl.SetColor(colorId);
    }
}