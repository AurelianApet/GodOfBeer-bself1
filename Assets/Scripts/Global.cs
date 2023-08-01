using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Linq;
using SimpleJSON;
using UnityEngine.SceneManagement;

public struct ProductInfo
{
    public int no;//
    public int id;
    public int server_id;

    public int is_soldout;
    public int unit_price;
    public int cup_size;
    public int sell_type;//0-ml, 1-cup

    public int open_time;
    public int decarbo_time;
    public int decarbonation;
    public int total_amount;
    public int remain_amount;

    public int tagGW_no;
    public int tagGW_channel;
    public int board_no;
    public int board_channel;

    public WorkSceneType sceneType;
}

public enum WorkSceneType
{
    standby = 1,
    pour,
    soldout,
    remain
}

public class Global
{
    //image download path
    public static string imgPath = "";
    public static string prePath = "";
    public static string sdate = "";
    public static int newStatusBarValue;
    public static ProductInfo pInfo = new ProductInfo();

    //api
    public static string ip = "";
    public static string api_server_port = "3006";
    public static string api_url = "";
    public static string check_db_api = "check-db";
    public static string cancel_soldout_api = "cancel-soldout";
    public static string soldout_api = "soldout";
    public static string keg_init_confirm_api = "keg-init-confirm";
    public static string save_setinfo_api = "save-tap-info";
    public static string image_server_path = "http://" + ip + ":" + api_server_port + "/self/";
    public static string socket_server = "";

    public static string GetPriceFormat(float price)
    {
        return string.Format("{0:N0}", price);
    }

    public static void setStatusBarValue(int value)
    {
        newStatusBarValue = value;
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                try
                {
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(setStatusBarValueInThread));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }
        }
    }

    private static void setStatusBarValueInThread()
    {
#if UNITY_ANDROID
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (var window = activity.Call<AndroidJavaObject>("getWindow"))
                {
                    window.Call("setFlags", newStatusBarValue, -1);
                }
            }
        }
#endif
    }
}