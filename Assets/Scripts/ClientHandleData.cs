using System;
using System.Collections.Generic;
using UnityEngine;
using FPS_Bindings;

public class ClientHandleData
{
    private static ByteBuffer playerBuffer;

    public delegate void Packet_(byte[] data);

    public static Dictionary<int, Packet_> packets = new Dictionary<int, Packet_>();
    private static int pLength;

    public static void InitPackets()
    {
        packets.Add((int) Enumerations.ServerPackets.SIngame, HandleIngame);
        packets.Add((int) Enumerations.ServerPackets.SPlayerData, HandlePlayerData);
        packets.Add((int) Enumerations.ServerPackets.SPlayerMove, HandlePlayerMove);
    }

    public static void HandleData(byte[] data)
    {
        byte[] buffer = (byte[]) data.Clone();

        if (playerBuffer == null)
        {
            playerBuffer = new ByteBuffer();
        }

        playerBuffer.WriteBytes(buffer);

        if (playerBuffer.Count() == 0)
        {
            playerBuffer.Clear();
            return;
        }

        if (playerBuffer.Length() >= 4)
        {
            pLength = playerBuffer.ReadInteger(false);

            if (pLength <= 0)
            {
                playerBuffer.Clear();
                return;
            }
        }

        while (pLength > 0 & pLength <= playerBuffer.Length() - 4)
        {
            if (pLength <= playerBuffer.Length() - 4)
            {
                playerBuffer.ReadInteger();
                data = playerBuffer.ReadBytes(pLength);
                HandleDataPackets(data);
            }

            pLength = 0;

            if (playerBuffer.Length() >= 4)
            {
                pLength = playerBuffer.ReadInteger(false);

                if (pLength < 0)
                {
                    playerBuffer.Clear();
                    return;
                }
            }

            if (pLength < 1)
            {
                playerBuffer.Clear();
            }
        }
    }

    private static void HandleDataPackets(byte[] data)
    {
        int packetId;
        ByteBuffer buffer;
        Packet_ packet;

        buffer = new ByteBuffer();
        buffer.WriteBytes(data);

        packetId = buffer.ReadInteger();
        buffer.Dispose();

        if (packets.TryGetValue(packetId, out packet))
        {
            Debug.Log("<Packet> " + Enum.GetName(typeof(Enumerations.ServerPackets), packetId));
            packet.Invoke(data);
        }
    }

    private static void HandleIngame(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInteger();

        ClientManager.instance.myConnectionId = buffer.ReadInteger();
        
        buffer.Dispose();
    }
    
    public static void HandlePlayerData(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInteger();

        int connectionId = buffer.ReadInteger();
        Types.players[connectionId].connectionId = connectionId;
        
        buffer.Dispose();
        
        ClientManager.instance.InstantiateNetworkPlayer(connectionId);
    }
    
    public static void HandlePlayerMove(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInteger();

        int connectionId = buffer.ReadInteger();

        var pos = buffer.ReadVector3();
        var rot = buffer.ReadQuaternion();

        Types.players[connectionId].playerPref.transform.position = new UnityEngine.Vector3(pos.x, pos.y, pos.z);
        Types.players[connectionId].playerPref.transform.rotation = new UnityEngine.Quaternion(rot.x, rot.y, rot.z, rot.w);
        
        buffer.Dispose();
    }
}
