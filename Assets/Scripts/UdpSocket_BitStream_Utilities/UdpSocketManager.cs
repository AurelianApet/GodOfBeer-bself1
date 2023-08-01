using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/**
 * This class wraps a C# UdpClient, creates two threads for send & receive
 * and provides methods for sending, receiving data and closing threads.
 */

class UdpSession
{
    AsyncCallback receiveCallback;
    UdpClient udpSocket;

    public UdpSession(int port, AsyncCallback OnReceiveCallback)
    {
        udpSocket = new UdpClient(port);
        udpSocket.Client.Blocking = false;
        //udpSocket set ioctl/////////////
        const int SIO_UDP_CONNRESET = -1744830452;
        udpSocket.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
        //////////////////////////////////
        
        udpSocket.EnableBroadcast = true;
        receiveCallback = OnReceiveCallback;
        udpSocket.BeginReceive(receiveCallback, udpSocket);
    }

    public void Close()
    {
        if (udpSocket != null)
        {
            udpSocket.Close();
            udpSocket = null;
            receiveCallback = null;
        }
    }

    public void Broadcast(int port, byte[] data)
    {
        IPAddress[] broadcastAddresses = NetUtils.GetDirectedBroadcastAddresses();
        foreach (IPAddress broadcastAddress in broadcastAddresses)
        {
            Send(broadcastAddress.ToString(), port, data);
        }
    }

    public void Send(string ip, int port, byte[] data)
    {
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ip), port);
        Send(target, data);
    }

    public void Send(IPEndPoint target, byte[] data)
    {
        int length = data == null ? 0 : data.Length;

        int result = udpSocket.Send(data, data.Length, target);
    }
}