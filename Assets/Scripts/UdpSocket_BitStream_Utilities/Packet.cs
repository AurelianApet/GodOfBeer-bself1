using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Errorcode : int
{
    ERR_VERSION = 0x00000000,

    ERR_UNKNOWN,
    ERR_INVALID_OPCODE,
    ERR_NOT_AUTH,
    ERR_SERVER_INTERNAL,
    ERR_INVALID_SERIAL,
    ERR_ALREADY_AUTH,

    ERR_VERSION_END
}
public enum Opcode : int
{
    OP_VERSION = 0x10000000,

    REQ_GET_DEVICE_INFO,
    RES_GET_DEVICE_INFO,
    REQ_SET_DEVICE_INFO,
    RES_SET_DEVICE_INFO,
    REQ_SET_DEVICE_REBOOT,
    RES_SET_DEVICE_REBOOT,
    REQ_PING,
    RES_PING,
    REQ_GET_TAG_INFO,
    RES_GET_TAG_INFO,
    REQ_SET_TAG_LOCK,
    RES_SET_TAG_LOCK,
    REQ_SET_VALVE_CTRL,
    RES_SET_VALVE_CTRL,
    REQ_GET_DEVICE_STATUS,
    RES_GET_DEVICE_STATUS,
    REQ_SET_DEVICE_STATUS,
    RES_SET_DEVICE_STATUS,
    REQ_SET_FLOWMETER_START,
    RES_SET_FLOWMETER_START,
    REQ_SET_FLOWMETER_VALUE,
    RES_SET_FLOWMETER_VALUE,
    REQ_SET_FLOWMETER_FINISH,
    RES_SET_FLOWMETER_FINISH,
    ERROR_MESSAGE,

    OP_VERSION_END
}

public class Packet
{
    public Int32 length { get; private set; }
    public Int32 opcode { get; private set; }
    public Int64 rsved1 { get; private set; }
    public Int64 rsved2 { get; private set; }

    public Packet() { }

    public Packet(int length, int opcode, long rsved1, long rsved2, byte[] body)
    {
        this.length = length;
        this.opcode = opcode;
        this.rsved1 = rsved1;
        this.rsved2 = rsved2;
    }

    public static int HeaderSize
    {
        get
        {
            return 24;
        }
    }
}
