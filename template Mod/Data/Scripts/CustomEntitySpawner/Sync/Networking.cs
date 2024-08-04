using PEPCO.LogError;
using PEPCO.Sync;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System;
using VRage.Game.ModAPI;

public class Networking
{
    public static PEPCO_LogError log = new PEPCO_LogError();

    public readonly ushort PacketId;

    public Networking(ushort packetId)
    {
        PacketId = packetId;
    }

    public void Register()
    {
        MyAPIGateway.Multiplayer.RegisterMessageHandler(PacketId, ReceivedPacket);
    }

    public void Unregister()
    {
        MyAPIGateway.Multiplayer.UnregisterMessageHandler(PacketId, ReceivedPacket);
    }

    private void ReceivedPacket(byte[] rawData)
    {
        try
        {
            var packet = MyAPIGateway.Utilities.SerializeFromBinary<PacketBase>(rawData);
            bool relay = false;
            packet.Received(ref relay);

            if (relay)
                RelayToClients(packet, rawData);
        }
        catch (Exception ex)
        {
            log.LogError($"Update error: {ex.Message}");
        }
    }

    public void SendToServer(PacketBase packet)
    {
        var bytes = MyAPIGateway.Utilities.SerializeToBinary(packet);
        MyAPIGateway.Multiplayer.SendMessageToServer(PacketId, bytes);
    }

    public void SendToPlayer(PacketBase packet, ulong steamId)
    {
        //if (!MyAPIGateway.Multiplayer.IsServer) return;

        var bytes = MyAPIGateway.Utilities.SerializeToBinary(packet);
        MyAPIGateway.Multiplayer.SendMessageTo(PacketId, bytes, steamId);
    }

    private List<IMyPlayer> tempPlayers;

    public void RelayToClients(PacketBase packet, byte[] rawData = null)
    {
        //if (!MyAPIGateway.Multiplayer.IsServer) return;

        if (tempPlayers == null)
            tempPlayers = new List<IMyPlayer>(MyAPIGateway.Session.SessionSettings.MaxPlayers);
        else
            tempPlayers.Clear();

        MyAPIGateway.Players.GetPlayers(tempPlayers);

        foreach (var p in tempPlayers)
        {
            if (p.SteamUserId == MyAPIGateway.Multiplayer.ServerId || p.SteamUserId == packet.SenderId)
                continue;

            if (rawData == null)
                rawData = MyAPIGateway.Utilities.SerializeToBinary(packet);

            MyAPIGateway.Multiplayer.SendMessageTo(PacketId, rawData, p.SteamUserId);
        }

        tempPlayers.Clear();
    }
}
