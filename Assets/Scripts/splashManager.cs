using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class splashManager : MonoBehaviour
{
    public float delay_time = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.fullScreen = true;
#if UNITY_ANDROID
        Global.setStatusBarValue(1024); // WindowManager.LayoutParams.FLAG_FORCE_NOT_FULLSCREEN
#endif
#if UNITY_IPHONE
		Global.imgPath = Application.persistentDataPath + "/bself1_img/";
#elif UNITY_ANDROID
        Global.imgPath = Application.persistentDataPath + "/bself1_img/";
#else
        if (Application.isEditor == true)
        {
            Global.imgPath = "/img/";
        }
#endif

#if UNITY_IPHONE
		Global.prePath = @"file://";
#elif UNITY_ANDROID
        Global.prePath = @"file:///";
#else
        Global.prePath = @"file://" + Application.dataPath.Replace("/Assets", "/");
#endif

        //delete all downloaded images
        try
        {
            if (Directory.Exists(Global.imgPath))
            {
                Directory.Delete(Global.imgPath, true);
            }
        }
        catch (Exception)
        {

        }
        LoadInfoFromPrefab();
    }

    void LoadInfoFromPrefab()
    {
        Global.ip = PlayerPrefs.GetString("ip");
        Global.api_url = "http://" + Global.ip + ":" + Global.api_server_port + "/m-api/self/";
        Global.socket_server = "ws://" + Global.ip + ":" + Global.api_server_port;
        Global.image_server_path = "http://" + Global.ip + ":" + Global.api_server_port + "/self/";
        Global.pInfo.no = PlayerPrefs.GetInt("no");
        StartCoroutine(GotoMainScene());
    }

    IEnumerator GotoMainScene()
    {
        yield return new WaitForSeconds(delay_time);
        SceneManager.LoadScene("main");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
