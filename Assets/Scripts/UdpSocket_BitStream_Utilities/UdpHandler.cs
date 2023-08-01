using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPHandler {
    public int[] _unique_device_id { get; private set; }
    public int _udp_listen_port { get; private set; }
    public byte[] _mac { get; private set; }
    public byte[] _ip_address { get; private set; }
    public byte[] _netmask { get; private set; }
    public byte[] _gateway { get; private set; }
    public byte[] _server_ip { get; private set; }
    public short _server_port { get; private set; }

    UDPHandler() {
        _unique_device_id = new int[3];
        _udp_listen_port = -1;
        _mac = new byte[6];
        _ip_address = new byte[4];
        _netmask = new byte[4];
        _gateway = new byte[4];
        _server_ip = new byte[4];
        _server_port = -1;
    }

    public void SET_INFO(int[] unique_device_id, int udp_listen_port, byte[] mac, byte[] ip_address, byte[] netmask, byte[] gateway, byte[] server_ip, short server_port){
        this._unique_device_id = unique_device_id;
        this._udp_listen_port = udp_listen_port;
        this._mac = mac;
        this._ip_address = ip_address;
        this._netmask = netmask;
        this._gateway = gateway;
        this._server_ip = server_ip;
        this._server_port = server_port;
    }

    public byte[] Send_REQ_GET_DEVICE_INFO() {
        int length = Packet.HeaderSize;
        int opCode = (int)Opcode.REQ_GET_DEVICE_INFO;
        long rsved1 = new System.Random().Next();
        long rsved2 = new System.Random().Next();
        byte[] data = new byte[length];
        Array.Copy(NetUtils.GetBytes(length), 0, data, 0, 4);
        Array.Copy(NetUtils.GetBytes(opCode), 0, data, 4, 4);
        Array.Copy(NetUtils.GetBytes(rsved1), 0, data, 8, 8);
        Array.Copy(NetUtils.GetBytes(rsved2), 0, data, 16, 8);
        return data;
    }

    public bool Receive_RES_GET_DEVICE_INFO(int length, int opCode, long rsved1, long rsved2, int dataLength, byte[] data) {
        if(dataLength == 40) {
            int[] unique_device_id = new int[3];
            int udp_listen_port;
            byte[] mac = new byte[6];
            byte[] ip_address = new byte[4];
            byte[] netmask = new byte[4];
            byte[] gateway = new byte[4];
            byte[] server_ip = new byte[4];
            short server_port;

            for(int i = 0; i < 3; i ++) unique_device_id[i] = NetUtils.ToInt32(data, i * 4);
            udp_listen_port = NetUtils.ToInt32(data, 12);
            Array.Copy(data, 16, mac, 0, 6);
            Array.Copy(data, 22, ip_address, 0, 4);
            Array.Copy(data, 26, netmask, 0, 4);
            Array.Copy(data, 30, gateway, 0, 4);
            Array.Copy(data, 34, server_ip, 0, 4);
            server_port = NetUtils.ToInt16(data, 38);

            SET_INFO(unique_device_id, udp_listen_port, mac, ip_address, netmask, gateway, server_ip, server_port);

            Debug.Log("UDP Recive : RES_GET_DEVICE_INFO" + 
                " length : " + length + 
                " rsved1 : " + rsved1 + 
                " rsved2 : " + rsved2 + "\n");
            Debug.Log("Unique Device Id : ");
            for(int i = 0; i < 3; i ++) Debug.Log(unique_device_id[i] + " ");
            Debug.Log("\nUDP Listen Port : " + udp_listen_port);
            Debug.Log("\nMAC : ");
            for(int i = 0; i < 6; i ++) Debug.Log(mac[i] + " ");
            Debug.Log("\nIP Address : ");
            for(int i = 0; i < 4; i ++) Debug.Log(ip_address[i] + " : ");
            Debug.Log("\nNetmask : ");
            for(int i = 0; i < 4; i ++) Debug.Log(netmask[i] + " : ");
            Debug.Log("\nGateway : ");
            for(int i = 0; i < 4; i ++) Debug.Log(gateway[i] + " : ");
            Debug.Log("\nServer IP : ");
            for(int i = 0; i < 4; i ++) Debug.Log(server_ip[i] + " : ");
            Debug.Log("\nServer Port :" + server_port + "\n");
            return true;
        }
        else {
            Debug.Log("Receive UDP : RES_GET_DEVICE_INFO   Error occured!");
            return false;
        }
    }

    public byte[] Send_REQ_SET_DEVICE_INFO() {
        int length = Packet.HeaderSize + 40;
        int opCode = (int)Opcode.REQ_SET_DEVICE_INFO;
        long rsved1 = new System.Random().Next();
        long rsved2 = new System.Random().Next();

        byte[] data = new byte[length];
        Array.Copy(NetUtils.GetBytes(length), 0, data, 0, 4);
        Array.Copy(NetUtils.GetBytes(opCode), 0, data, 4, 4);
        Array.Copy(NetUtils.GetBytes(rsved1), 0, data, 8, 8);
        Array.Copy(NetUtils.GetBytes(rsved2), 0, data, 16, 8);

        for(int i = 0; i < 3; i++) Array.Copy(NetUtils.GetBytes(this._unique_device_id[i]), 0, data, 24 + (i * 4), 4);
        Array.Copy(NetUtils.GetBytes(this._udp_listen_port), 0, data, 36, 4);
        Array.Copy(this._mac, 0, data, 40, 6);
        Array.Copy(this._ip_address, 0, data, 46, 4);
        Array.Copy(this._netmask, 0, data, 50, 4);
        Array.Copy(this._gateway, 0, data, 54, 4);
        Array.Copy(this._server_ip, 0, data, 58, 4);
        Array.Copy(NetUtils.GetBytes(this._server_port), 0, data, 62, 2);

        return data;
    }

    public bool Receive_RES_SET_DEVICE_INFO(int length, int opCode, long rsved1, long rsved2, int dataLength, byte[] data) {
        if(dataLength == 40) {
            int udp_listen_port;
            int[] unique_device_id = new int[3];
            byte[] mac = new byte[6];
            byte[] ip_address = new byte[4];
            byte[] netmask = new byte[4];
            byte[] gateway = new byte[4];
            byte[] server_ip = new byte[4];
            short server_port;

            for(int i = 0; i < 3; i ++) unique_device_id[i] = NetUtils.ToInt32(data, i * 4);
            udp_listen_port = NetUtils.ToInt32(data, 12);
            Array.Copy(data, 16, mac, 0, 6);
            Array.Copy(data, 22, ip_address, 0, 4);
            Array.Copy(data, 26, netmask, 0, 4);
            Array.Copy(data, 30, gateway, 0, 4);
            Array.Copy(data, 34, server_ip, 0, 4);
            server_port = NetUtils.ToInt16(data, 38);

            SET_INFO(unique_device_id, udp_listen_port, mac, ip_address, netmask, gateway, server_ip, server_port);

            Debug.Log("UDP Recive : RES_SET_DEVICE_INFO" + 
                " length : " + length + 
                " rsved1 : " + rsved1 + 
                " rsved2 : " + rsved2 + "\n");
            Debug.Log("UDP Listen Port : " + udp_listen_port);
            Debug.Log("\nUnique Device Id : ");
            for(int i = 0; i < 3; i ++) Debug.Log(unique_device_id[i] + " ");
            Debug.Log("\nMAC : ");
            for(int i = 0; i < 6; i ++) Debug.Log(mac[i] + " ");
            Debug.Log("\nIP Address : ");
            for(int i = 0; i < 4; i ++) Debug.Log(ip_address[i] + " : ");
            Debug.Log("\nNetmask : ");
            for(int i = 0; i < 4; i ++) Debug.Log(netmask[i] + " : ");
            Debug.Log("\nGateway : ");
            for(int i = 0; i < 4; i ++) Debug.Log(gateway[i] + " : ");
            Debug.Log("\nServer IP : ");
            for(int i = 0; i < 4; i ++) Debug.Log(server_ip[i] + " : ");
            Debug.Log("\nServer Port :" + server_port + "\n");
            return true;
        }
        else {
            Debug.Log("Receive UDP : RES_SET_DEVICE_INFO   Error occured!");
            return false;
        }
    }

    public byte[] Send_REQ_SET_DEVICE_REBOOT() {
        int length = Packet.HeaderSize;
        int opCode = (int)Opcode.REQ_SET_DEVICE_REBOOT;
        long rsved1 = new System.Random().Next();
        long rsved2 = new System.Random().Next();
        byte[] data = new byte[length];
        Array.Copy(NetUtils.GetBytes(length), 0, data, 0, 4);
        Array.Copy(NetUtils.GetBytes(opCode), 0, data, 4, 4);
        Array.Copy(NetUtils.GetBytes(rsved1), 0, data, 8, 8);
        Array.Copy(NetUtils.GetBytes(rsved2), 0, data, 16, 8);
        return data;        
    }

    public bool Receive_RES_SET_DEVICE_REBOOT(int length, int opCode, long rsved1, long rsved2, int dataLength, byte[] data) {
        if(dataLength == 0) {
            Debug.Log("UDP Recive : RES_SET_DEVICE_REBOOT" + 
                " length : " + length + 
                " rsved1 : " + rsved1 + 
                " rsved2 : " + rsved2 + "\n");
            return true;
        }
        else {
            Debug.Log("Receive UDP : RES_SET_DEVICE_REBOOT Error occured!");
            return false;
        }
    }
}
