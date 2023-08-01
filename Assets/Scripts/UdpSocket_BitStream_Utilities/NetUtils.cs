using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public class NetUtils
{
    public static byte[] ConvertStringToByteArray(string source, int length)
    {
        byte[] result = new byte[length];
        char[] array = source.ToCharArray();
        for (int i = 0; i < array.Length; i += 2)
        {
            int first = 0, second = 0;
            int temp = 0;
            if (array[i] >= '0' && '9' >= array[i]) first = (array[i] - '0');
            else if (array[i] >= 'A' && 'F' >= array[i]) first = (array[i] - 'A') + 10;
            else if (array[i] >= 'a' && 'f' >= array[i]) first = (array[i] - 'a') + 10;

            if ((i + 1) != array.Length)
            {
                if (array[i + 1] >= '0' && '9' >= array[i + 1]) second = (array[i + 1] - '0');
                else if (array[i + 1] >= 'A' && 'F' >= array[i + 1]) second = (array[i + 1] - 'A') + 10;
                else if (array[i + 1] >= 'a' && 'f' >= array[i + 1]) second = (array[i + 1] - 'a') + 10;

                temp = first * 16 + second;
            }
            else temp = first;

            result[(i + 2) / 2 - 1] = (byte)temp;
        }
        return result;
    }

    public static string ConvertIntArrayToString(int[] source)
    {
        string result = "";
        for (int i = 0; i < source.Length; i++)
        {
            result += source[i].ToString();
        }
        return result;
    }

    /// 문자열->안시 바이트 배열
    public static Byte[] ConvertStringToByteArrayASCII(String s)
    {
        return Encoding.ASCII.GetBytes(s);
    }
    public static byte[] StrToBytes(string byteData)
    {

        System.Text.ASCIIEncoding asencoding = new System.Text.ASCIIEncoding();

        return Encoding.Default.GetBytes(byteData);
    }
    /// 안시 바이트 배열->문자열
    public static string ConvertByteArrayToStringASCII(byte[] b)
    {
        List<byte> _b = new List<byte>();
        foreach (var data in b)
        {
            if (data == 0x00)
                break;

            _b.Add(data);
        }
        return Encoding.ASCII.GetString(_b.ToArray(), 0, _b.Count);
    }

    ///// 문자열->유니코드 바이트 배열
    //public static Byte[] ConvertStringToByteArray(String s)
    //{
    //    return (new UnicodeEncoding()).GetBytes(s);
    //}

    ///// 유니코드 바이트 배열->문자열
    //public static string ConvertByteArrayToString(byte[] b)
    //{
    //    return (new UnicodeEncoding()).GetString(b, 0, b.Length);
    //}

    /// UTF8 string to bytes
    public static byte[] ConvertStringToByteArrayUTF8(string s)
    {
        Encoding src = Encoding.Default;
        Encoding dst = Encoding.UTF8;

        return Encoding.Convert(src, dst, src.GetBytes(s));
    }

    /// bytes to UTF8 string
    public static string ConvertByteArrayToStringUTF8(byte[] b)
    {
        return Encoding.UTF8.GetString(b);
    }

    public static int ConvertDateTimeToNetDate(DateTime dt)
    {
        return (dt.Year * 10000) + (dt.Month * 100) + dt.Day;
    }

    public static int ConvertDateTimeToNetTime(DateTime dt)
    {
        return (dt.Hour * 10000000) + (dt.Minute * 100000) + (dt.Second * 1000) + dt.Millisecond;
    }

    public static DateTime ConvertNetDatetimeToDateTime(int date, int time)
    {
        int year = date / 10000;
        int month = (date % 10000) / 100;
        int day = date % 100;

        int hour = time / 10000000;
        int min = (time % 10000000) / 100000;
        int sec = (time % 100000) / 1000;
        int msec = time % 1000;

        DateTime dt = new DateTime(year, month, day, hour, min, sec, msec);
        return dt;
    }
    static public ushort Reverse(ushort value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        return BitConverter.ToUInt16(temp, 0);
    }
    static public short Reverse(short value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        return BitConverter.ToInt16(temp, 0);
    }
    static public uint Reverse(uint value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        return BitConverter.ToUInt32(temp, 0);
    }
    static public int Reverse(int value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        return BitConverter.ToInt32(temp, 0);
    }
    static public ulong Reverse(ulong value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        return BitConverter.ToUInt64(temp, 0);
    }
    static public long Reverse(long value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        return BitConverter.ToInt64(temp, 0);
    }
    static public float Reverse(float value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        return BitConverter.ToSingle(temp, 0);
    }
    static public double Reverse(double value)
    {
        byte[] temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        return BitConverter.ToDouble(temp, 0);
    }

    static public byte[] GetBytes(ushort value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.GetBytes(Reverse(value));
        }
        else
        {
            return BitConverter.GetBytes(value);
        }
    }
    static public byte[] GetBytes(short value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.GetBytes(Reverse(value));
        }
        else
        {
            return BitConverter.GetBytes(value);
        }
    }
    static public byte[] GetBytes(uint value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.GetBytes(Reverse(value));
        }
        else
        {
            return BitConverter.GetBytes(value);
        }
    }
    static public byte[] GetBytes(int value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.GetBytes(Reverse(value));
        }
        else
        {
            return BitConverter.GetBytes(value);
        }
    }
    static public byte[] GetBytes(ulong value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.GetBytes(Reverse(value));
        }
        else
        {
            return BitConverter.GetBytes(value);
        }
    }
    static public byte[] GetBytes(long value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.GetBytes(Reverse(value));
        }
        else
        {
            return BitConverter.GetBytes(value);
        }
    }
    static public byte[] GetBytes(float value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.GetBytes(Reverse(value));
        }
        else
        {
            return BitConverter.GetBytes(value);
        }
    }
    static public byte[] GetBytes(double value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BitConverter.GetBytes(Reverse(value));
        }
        else
        {
            return BitConverter.GetBytes(value);
        }
    }

    static public ushort ToUInt16(byte[] value, int startindex)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Reverse(BitConverter.ToUInt16(value, startindex));
        }
        else
        {
            return BitConverter.ToUInt16(value, startindex);
        }
    }
    static public short ToInt16(byte[] value, int startindex)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Reverse(BitConverter.ToInt16(value, startindex));
        }
        else
        {
            return BitConverter.ToInt16(value, startindex);
        }
    }
    static public uint ToUInt32(byte[] value, int startindex)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Reverse(BitConverter.ToUInt32(value, startindex));
        }
        else
        {
            return BitConverter.ToUInt32(value, startindex);
        }
    }
    static public int ToInt32(byte[] value, int startindex)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Reverse(BitConverter.ToInt32(value, startindex));
        }
        else
        {
            return BitConverter.ToInt32(value, startindex);
        }
    }
    static public ulong ToUInt64(byte[] value, int startindex)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Reverse(BitConverter.ToUInt64(value, startindex));
        }
        else
        {
            return BitConverter.ToUInt64(value, startindex);
        }
    }
    static public long ToInt64(byte[] value, int startindex)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Reverse(BitConverter.ToInt64(value, startindex));
        }
        else
        {
            return BitConverter.ToInt64(value, startindex);
        }
    }
    static public float ToSingle(byte[] value, int startindex)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Reverse(BitConverter.ToSingle(value, startindex));
        }
        else
        {
            return BitConverter.ToSingle(value, startindex);
        }
    }
    static public double ToDouble(byte[] value, int startindex)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Reverse(BitConverter.ToDouble(value, startindex));
        }
        else
        {
            return BitConverter.ToDouble(value, startindex);
        }
    }

    static public int CheckSign(int data, int signBitPos)
    {
        if (((data >> signBitPos) & 0x00000001) == 0)
        {
            return data;
        }

        return data | (int)(0xFFFFFFFF << signBitPos);
    }
    static public IPAddress[] GetDirectedBroadcastAddresses()
    {
        List<IPAddress> list = new List<IPAddress>();
        var networks = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface net in networks)
        {
            if (net.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;
            if (net.OperationalStatus != OperationalStatus.Up)
                continue;

            UnicastIPAddressInformationCollection unicats = net.GetIPProperties().UnicastAddresses;
            foreach (UnicastIPAddressInformation unicast in unicats)
            {
                IPAddress ipAddress = unicast.Address;

                if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                byte[] addressBytes = ipAddress.GetAddressBytes();
                byte[] subnetBytes = unicast.IPv4Mask.GetAddressBytes();

                if (addressBytes.Length != subnetBytes.Length)
                    continue;

                byte[] broadcastAddressBytes = new byte[addressBytes.Length];
                for (int i = 0; i < broadcastAddressBytes.Length; i++)
                {
                    broadcastAddressBytes[i] = (byte)(addressBytes[i] | (subnetBytes[i] ^ 255));
                }

                list.Add(new IPAddress(broadcastAddressBytes));
            }
        }
        return list.ToArray();
    }

    #region Mac Address Providor
    const int PING_TIMEOUT = 1000;

    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

    // *********************************************************************
    /// <summary>
    /// Gets the MAC address from ARP table in colon (:) separated format.
    /// </summary>
    /// <param name="hostNameOrAddress">Host name or IP address of the
    /// remote host for which MAC address is desired.</param>
    /// <returns>A string containing MAC address; null if MAC address could
    /// not be found.</returns>
    public static string GetMACAddressFromARP(IPAddress ipaddress)
    {
        //Console.WriteLine("Get MAC Begin");
        if (!IsHostAccessible(ipaddress.ToString()))
        {
            //Console.WriteLine("IsHostAccessible == false");
            return null;
        }
        else
        {
            //Console.WriteLine("IsHostAccessible == true");
        }

        byte[] macAddr = new byte[6];

        uint macAddrLen = (uint)macAddr.Length;

#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
        if (SendARP((int)ipaddress.Address, 0, macAddr, ref macAddrLen) != 0)
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
        {
            //Console.WriteLine("SendARP((int)hostEntry.AddressList[0].Address, 0, macAddr, ref macAddrLen) != 0");
            return null;
        }
        else
        {
            //Console.WriteLine("SendARP Success");
        }


        StringBuilder macAddressString = new StringBuilder();
        for (int i = 0; i < macAddr.Length; i++)
        {
            if (macAddressString.Length > 0)
                macAddressString.Append(":");
            macAddressString.AppendFormat("{0:x2}", macAddr[i]);

        }
        Console.WriteLine("MAC : " + macAddressString.ToString());
        return macAddressString.ToString();
    }

    // *********************************************************************
    /// <summary>
    /// Checks to see if the host specified by
    /// <paramref name="hostNameOrAddress"/> is currently accessible.
    /// </summary>
    /// <param name="hostNameOrAddress">Host name or IP address of the
    /// remote host for which MAC address is desired.</param>
    /// <returns><see langword="true" /> if the host is currently accessible;
    /// <see langword="false"/> otherwise.</returns>
    private static bool IsHostAccessible(string hostNameOrAddress)
    {
        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
        PingReply reply = ping.Send(hostNameOrAddress, PING_TIMEOUT);
        return reply.Status == IPStatus.Success;
    }
    #endregion
}
