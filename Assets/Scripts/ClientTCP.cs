using System;
using System.Net;
using System.Net.Sockets;
using FPS_Bindings;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class ClientTCP
{
    private static TcpClient clientSocket;
    private static NetworkStream myStream;
    private static byte[] asyncBuffer;

    public static void InitClient(string address, int port)
    {
        clientSocket = new TcpClient();
        clientSocket.ReceiveBufferSize = Constants.MAX_BUFFERSIZE;
        clientSocket.SendBufferSize = Constants.MAX_BUFFERSIZE;
        
        asyncBuffer = new byte[Constants.MAX_BUFFERSIZE * 2];
        
        clientSocket.BeginConnect(address, port, new AsyncCallback(ClientConnectCallback), clientSocket);
    }

    private static void ClientConnectCallback(IAsyncResult ar)
    {
        clientSocket.EndConnect(ar);

        if (clientSocket.Connected == false)
        {
            return;
        }

        clientSocket.NoDelay = true;
        myStream = clientSocket.GetStream();
        myStream.BeginRead(asyncBuffer, 0, Constants.MAX_BUFFERSIZE * 2, ReceiveCallback, null);
        
        Debug.Log("Successfully connected to the server!");
        
//        ClientManager.instance.InstantiateNetworkPlayer();
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            int readBytes = myStream.EndRead(ar);

            if (readBytes <= 0)
            {
                return;
            }
        
            byte[] newBytes = new byte[readBytes];
            Buffer.BlockCopy(asyncBuffer, 0, newBytes, 0, readBytes);

            // Add Unity Thread Here
            UnityThread.executeInUpdate(() =>
            {
                // HandleData here
                ClientHandleData.HandleData(newBytes);
            });
            
            myStream.BeginRead(asyncBuffer, 0, Constants.MAX_BUFFERSIZE * 2, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static void DisconnectFromServer()
    {
        clientSocket.Close();
        clientSocket = null;
    }

    public static void SendData(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
        buffer.WriteBytes(data);
        myStream.Write(buffer.ToArray(), 0, buffer.ToArray().Length);
        buffer.Dispose();
    }

    public static void SendMovement(Vector3 position, Quaternion rotation)
    {
        var pos = new FPS_Bindings.Vector3(position.x, position.y, position.z);
        var rot = new FPS_Bindings.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInteger((int) Enumerations.ClientPackets.CMovement);
        buffer.WriteVector3(pos);
        buffer.WriteQuaternion(rot);
        SendData(buffer.ToArray());
    }
}
