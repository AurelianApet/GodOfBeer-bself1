using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleJSON;
using SocketIO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class MainManager : MonoBehaviour
{
    public GameObject shop_open_popup;
    public GameObject decarbonate_popup;
    public float delay_time = 0.5f;

    //work
    public RawImage backImg;
    public GameObject workObj;
    public GameObject priceObj;
    public Text contentObj;

    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;

    //setting
    public GameObject settingObj;
    public GameObject washPopup;
    public GameObject kegPopup;
    public GameObject devicecheckingPopup;
    public GameObject kegInitPopup;
    public GameObject err_popup;
    public Text err_content;
    public GameObject splash_err_popup;
    public Text splash_err_content;

    public float response_delay_time = 5f;

    public int flag = 0;
    long prev_flowmeter_value = 0;

    //setting ui
    public InputField no;
    public InputField ipField;
    public AudioSource[] soundObjs; //0-sound, 1-alarm, 2-touch, 3-start_app

    // Start is called before the first frame update
    void Start()
    {
        soundObjs[3].Play();
        if (Global.ip == "" || Global.pInfo.no == 0)
        {
            showSettingScene();
        }
        else
        {
            checkIP();
        }
    }

    void checkIP()
    {
        WWWForm form = new WWWForm();
        form.AddField("serial_number", Global.pInfo.no);
        form.AddField("type", 1);
        WWW www = new WWW(Global.api_url + Global.check_db_api, form);
        StartCoroutine(ipCheck(www));
    }

    IEnumerator ipCheck(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            string result = jsonNode["suc"].ToString()/*.Replace("\"", "")*/;
            if (result == "1")
            {
                if (socket != null)
                {
                    socket.Close();
                    socket.OnDestroy();
                    socket.OnApplicationQuit();
                }
                if (socketObj != null)
                {
                    DestroyImmediate(socketObj);
                }
                Global.pInfo.id = jsonNode["id"].AsInt;
                Global.pInfo.server_id = jsonNode["server_id"].AsInt;
                Global.pInfo.is_soldout = jsonNode["soldout"].AsInt;
                Global.pInfo.sell_type = jsonNode["sell_type"].AsInt;
                Global.pInfo.unit_price = jsonNode["unit_price"].AsInt;
                Global.pInfo.cup_size = jsonNode["cup_size"].AsInt;

                Global.pInfo.open_time = jsonNode["opentime"].AsInt;
                Global.pInfo.decarbo_time = jsonNode["decarbo_time"].AsInt;
                Global.pInfo.total_amount = jsonNode["total_amount"].AsInt;
                Global.pInfo.remain_amount = jsonNode["remain_amount"].AsInt;
                Global.pInfo.decarbonation = jsonNode["decarbonation"].AsInt;

                Global.pInfo.tagGW_no = jsonNode["gw_no"].AsInt;
                Global.pInfo.tagGW_channel = jsonNode["gw_channel"].AsInt;
                Global.pInfo.board_no = jsonNode["board_no"].AsInt;
                Global.pInfo.board_channel = jsonNode["board_channel"].AsInt;
                if (jsonNode["sold_out"].AsInt == 1)
                {
                    Global.pInfo.sceneType = WorkSceneType.soldout;
                }
                else
                {
                    Global.pInfo.sceneType = WorkSceneType.standby;
                }
                string url = Global.image_server_path + "Standby" + jsonNode["server_id"].AsInt + ".jpg";
                StartCoroutine(downloadFile(url, Global.imgPath + Path.GetFileName(url)));
                string downloadImgUrl = Global.image_server_path + "Pour.jpg";
                StartCoroutine(downloadFile(downloadImgUrl, Global.imgPath + Path.GetFileName(downloadImgUrl)));
                downloadImgUrl = Global.image_server_path + "Remain.jpg";
                StartCoroutine(downloadFile(downloadImgUrl, Global.imgPath + Path.GetFileName(downloadImgUrl)));
                downloadImgUrl = Global.image_server_path + "Soldout.jpg";
                StartCoroutine(downloadFile(downloadImgUrl, Global.imgPath + Path.GetFileName(downloadImgUrl)));
                downloadImgUrl = Global.image_server_path + "tap.jpg";
                StartCoroutine(downloadFile(downloadImgUrl, Global.imgPath + Path.GetFileName(downloadImgUrl)));

                InitSocketFunctions();
                settingObj.SetActive(false);
                workObj.SetActive(true);
                showWorkScene();
            }
            else
            {
                showSettingScene();
            }
        }
        else
        {
            showSettingScene();
        }
    }

    void showSettingScene(bool is_set = true)
    {
        settingObj.SetActive(is_set);
        workObj.SetActive(!is_set);
        ipField.text = Global.ip;
        no.text = Global.pInfo.no.ToString();
    }

    public void onBack()
    {
        if(Global.ip == "" || Global.pInfo.no == 0)
        {
            err_content.text = "설정값들을 정확히 입력하세요.";
            err_popup.SetActive(true);
        }
        else
        {
            showSettingScene(false);
            showWorkScene();
            tagControl(1);
        }
    }

    public void SaveSetInfo()
    {
        if(ipField.text == "")
        {
            err_content.text = "ip를 입력하세요.";
            err_popup.SetActive(true);
        }
        else if (no.text == "" || no.text == "0")
        {
            err_content.text = "기기번호를 입력하세요.";
            err_popup.SetActive(true);
        }
        else
        {
            try
            {
                string tmp_url = "http://" + ipField.text.Trim() + ":" + Global.api_server_port + "/m-api/self/";
                WWWForm form = new WWWForm();
                form.AddField("serial_number", int.Parse(no.text));
                form.AddField("type", 1);
                WWW www = new WWW(tmp_url + Global.save_setinfo_api, form);
                StartCoroutine(saveSetProcess(www));
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }

    IEnumerator saveSetProcess(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            int result = jsonNode["suc"].AsInt;
            if (result == 1)
            {
                try
                {
                    if (Global.ip != ipField.text)
                    {
                        Global.ip = ipField.text;
                        PlayerPrefs.SetString("ip", Global.ip);
                        Global.api_url = "http://" + Global.ip + ":" + Global.api_server_port + "/m-api/self/";
                        Global.socket_server = "ws://" + Global.ip + ":" + Global.api_server_port;
                        Global.image_server_path = "http://" + Global.ip + ":" + Global.api_server_port + "/self/";
                        string _pourImgUrl = Global.image_server_path + "Pour.jpg";
                        string _remainImgUrl = Global.image_server_path + "Remain.jpg";
                        string _soldoutImgUrl = Global.image_server_path + "Soldout.jpg";
                        string _tapImgUrl = Global.image_server_path + "tap.jpg";
                        if (File.Exists(_pourImgUrl))
                        {
                            File.Delete(_pourImgUrl);
                        }
                        if (File.Exists(_remainImgUrl))
                        {
                            File.Delete(_remainImgUrl);
                        }
                        if (File.Exists(_soldoutImgUrl))
                        {
                            File.Delete(_soldoutImgUrl);
                        }
                        if (File.Exists(_tapImgUrl))
                        {
                            File.Delete(_tapImgUrl);
                        }
                    }
                    Global.pInfo.no = int.Parse(no.text);
                    PlayerPrefs.SetInt("no", Global.pInfo.no);
                    Global.pInfo.id = jsonNode["id"].AsInt;
                    Global.pInfo.server_id = jsonNode["server_id"].AsInt;
                    Global.pInfo.is_soldout = jsonNode["is_soldout"].AsInt;
                    Global.pInfo.cup_size = jsonNode["cup_size"].AsInt;
                    Global.pInfo.unit_price = jsonNode["unit_price"].AsInt;
                    Global.pInfo.open_time = jsonNode["opentime"].AsInt;
                    Global.pInfo.sell_type = jsonNode["sell_type"].AsInt;
                    Global.pInfo.decarbo_time = jsonNode["decarbo_time"].AsInt;
                    Global.pInfo.total_amount = jsonNode["total_amount"].AsInt;
                    Global.pInfo.remain_amount = jsonNode["remain_amount"].AsInt;
                    Global.pInfo.decarbonation = jsonNode["decarbonation"].AsInt;
                    Global.pInfo.board_no = jsonNode["board_no"].AsInt;
                    Global.pInfo.board_channel = jsonNode["board_channel"].AsInt;
                    Global.pInfo.tagGW_no = jsonNode["gw_no"].AsInt;
                    Global.pInfo.tagGW_channel = jsonNode["gw_channel"].AsInt;
                    if (Global.pInfo.is_soldout == 1)
                    {
                        Global.pInfo.sceneType = WorkSceneType.soldout;
                    }
                    else
                    {
                        Global.pInfo.sceneType = WorkSceneType.standby;
                    }
                    string url = Global.image_server_path + "Standby" + jsonNode["server_id"].AsInt + ".jpg";
                    //StartCoroutine(downloadFile(url, Global.imgPath + Path.GetFileName(url)));
                    string failImgUrl = Global.image_server_path + "tap.jpg";
                    string filepath = Global.imgPath + Path.GetFileName(url);
                    StartCoroutine(downloadAndLoadImage(url, filepath, backImg));
                    StartCoroutine(checkDownImage(filepath, failImgUrl));

                    err_popup.SetActive(true);
                    err_content.text = "저장되었습니다.";
                }
                catch(Exception ex)
                {
                    err_content.text = "정보를 정확히 입력하세요.";
                    err_popup.SetActive(true);
                }
            }
            else
            {
                err_content.text = "정보를 정확히 입력하세요.";
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_content.text = "저장에 실패하였습니다.";
            err_popup.SetActive(true);
        }
    }

    public void onConfirmErrPopup()
    {
        err_popup.SetActive(false);
    }

    public void Wash()
    {
        washPopup.SetActive(true);
        valveControl(0, 1);
    }

    public void onConfirmWashPopup()
    {
        Debug.Log("Finish washing.");
        washPopup.SetActive(false);
        //Global.pInfo.sceneType = WorkSceneType.standby;
        valveControl(0, 0);
    }

    public void KegChange()
    {
        kegPopup.SetActive(true);
        //tagControl(0);
        valveControl(0, 1);
    }

    public void onConfirmKegPopup()
    {
        kegPopup.SetActive(false);
        kegInitPopup.SetActive(true);
    }

    IEnumerator ProcessKegInitConfirmApi()
    {
        WWWForm form = new WWWForm();
        form.AddField("serial_number", Global.pInfo.no);
        WWW www = new WWW(Global.api_url + Global.keg_init_confirm_api, form);
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            string result = jsonNode["suc"].ToString()/*.Replace("\"", "")*/;
            if (result == "1")
            {
                kegInitPopup.SetActive(false);
                err_popup.SetActive(false);
                Global.pInfo.sceneType = WorkSceneType.standby;
                tagControl(1);
                valveControl(0, 0);
            }
        }
        else
        {
            kegInitPopup.SetActive(false);
            err_content.text = "서버와 연결 중입니다. 잠시만 기다려주세요.";
            err_popup.SetActive(true);
        }
    }

    public void onConfirmkegInitPopup()
    {
        valveControl(0, 0);
        tagControl(1);
        //StartCoroutine(ProcessKegInitConfirmApi());
        kegInitPopup.SetActive(false);
        err_popup.SetActive(false);
        showSettingScene(false);
        Global.pInfo.sceneType = WorkSceneType.standby;
        showWorkScene();
    }

    public void onCancelKegInitPopup()
    {
        valveControl(0, 0);
        kegInitPopup.SetActive(false);
    }

    public void Soldout()
    {
        WWWForm form = new WWWForm();
        form.AddField("serial_number", Global.pInfo.no);
        WWW www = new WWW(Global.api_url + Global.soldout_api, form);
        StartCoroutine(Soldout(www));
    }

    void InitSocketFunctions()
    {
        socketObj = Instantiate(socketPrefab);
        socket = socketObj.GetComponent<SocketIOComponent>();
        socket.On("open", socketOpen);
        socket.On("soldout", soldoutEventHandler);
        socket.On("infochanged", InfoChangedEventHandler);
        socket.On("flowmeterStart", flowmeterStartEventHandler);
        socket.On("flowmeterValue", flowmeterValueEventHandler);
        socket.On("flowmeterFinish", flowmeterFinishEventHandler);
        socket.On("errorReceived", errorReceivedEventHandler);
        socket.On("adminReceived", adminReceivedEventHandler);

        socket.On("shopOpen", openShopEventHandler);
        socket.On("shopClose", closeShopEventHandler);
        socket.On("RepairingDevice", RepairingDevice);

        socket.On("error", socketError);
        socket.On("close", socketClose);
    }

    void showWorkScene(int capacity = 0)
    {
        try
        {
            switch (Global.pInfo.sceneType)
            {
                case WorkSceneType.pour:
                    {
                        string downloadImgUrl = Global.image_server_path + "Pour.jpg";
                        string filepath = Global.imgPath + Path.GetFileName(downloadImgUrl);
                        StartCoroutine(downloadAndLoadImage(downloadImgUrl, filepath, backImg));
                        priceObj.SetActive(true);
                        contentObj.text = Global.GetPriceFormat(capacity) + " ml";
                        break;
                    }
                case WorkSceneType.remain:
                    {
                        string downloadImgUrl = Global.image_server_path + "Remain.jpg";
                        string filepath = Global.imgPath + Path.GetFileName(downloadImgUrl);
                        StartCoroutine(downloadAndLoadImage(downloadImgUrl, filepath, backImg));
                        priceObj.SetActive(true);
                        contentObj.text = Global.GetPriceFormat(capacity) + " 원";
                        break;
                    }
                case WorkSceneType.soldout:
                    {
                        soundObjs[1].Play();
                        string downloadImgUrl = Global.image_server_path + "Soldout.jpg";
                        string filepath = Global.imgPath + Path.GetFileName(downloadImgUrl);
                        StartCoroutine(downloadAndLoadImage(downloadImgUrl, filepath, backImg));
                        priceObj.SetActive(false);
                        break;
                    }
                case WorkSceneType.standby:
                    {
                        soundObjs[0].Play();
                        priceObj.SetActive(true);
                        if (Global.pInfo.sell_type == 0)
                        {
                            //cup
                            contentObj.text = Global.GetPriceFormat(Global.pInfo.unit_price * Global.pInfo.cup_size) + " 원/" + Global.GetPriceFormat(Global.pInfo.cup_size) + "ml";
                        }
                        else
                        {
                            //ml
                            contentObj.text = Global.GetPriceFormat(Global.pInfo.unit_price) + " 원/ml";
                        }
                        string downloadImgUrl = Global.image_server_path + "Standby" + Global.pInfo.server_id + ".jpg";
                        string failImgUrl = Global.image_server_path + "tap.jpg";
                        string filepath = Global.imgPath + Path.GetFileName(downloadImgUrl);
                        StartCoroutine(downloadAndLoadImage(downloadImgUrl, filepath, backImg));
                        StartCoroutine(checkDownImage(filepath, failImgUrl));
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    IEnumerator checkDownImage(string imgPath, string failPath)
    {
        yield return new WaitForSeconds(3f);
        if (!File.Exists(imgPath))
        {
            string filepath = Global.imgPath + Path.GetFileName(failPath);
            Debug.Log(filepath);
            StartCoroutine(downloadAndLoadImage(failPath, filepath, backImg));
        }
    }

    public void onDecarbonate()
    {
        decarbonate_popup.SetActive(true);
        StartCoroutine(Decarbonate());
    }

    IEnumerator Decarbonate()
    {
        valveControl(1, 1);
        yield return new WaitForSeconds(Global.pInfo.decarbo_time);
        tagControl(1);
        valveControl(1, 0);
        decarbonate_popup.SetActive(false);
        showSettingScene(false);
        Global.pInfo.sceneType = WorkSceneType.standby;
        showWorkScene();
    }

    IEnumerator shopDecarbonate()
    {
        Debug.Log("stop decarbonate from shop close event.");
        yield return new WaitForSeconds(Global.pInfo.decarbo_time);
        decarbonate_popup.SetActive(false);
        showSettingScene(false);
        Global.pInfo.sceneType = WorkSceneType.standby;
        showWorkScene();
    }

    public void socketOpen(SocketIOEvent e)
    {
        if(is_socket_open)
        {
            return;
        }
        if(Global.pInfo.no != 0 && socket != null)
        {
            is_socket_open = true;
            string sId = "{\"no\":\"" + Global.pInfo.no + "\"}";
            socket.Emit("self1SetInfo", JSONObject.Create(sId));
            Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
        }
    }

    public void InfoChangedEventHandler(SocketIOEvent e)
    {
        try
        {
            Debug.Log("[SocketIO] InfoChangedEvent received: " + e.name + " " + e.data);
            JSONNode jsonNode = SimpleJSON.JSON.Parse(e.data.ToString());
            int id = jsonNode["id"].AsInt;
            if (Global.pInfo.id != id)
            {
                return;
            }
            Global.pInfo.server_id = jsonNode["server_id"].AsInt;
            Global.pInfo.id = jsonNode["id"].AsInt;
            Global.pInfo.cup_size = jsonNode["cup_size"].AsInt;
            Global.pInfo.unit_price = jsonNode["unit_price"].AsInt;
            Global.pInfo.sell_type = jsonNode["sell_type"].AsInt;
            Global.pInfo.tagGW_no = jsonNode["gw_no"].AsInt;
            Global.pInfo.tagGW_channel = jsonNode["gw_channel"].AsInt;
            Global.pInfo.board_no = jsonNode["board_no"].AsInt;
            Global.pInfo.board_channel = jsonNode["board_channel"].AsInt;
            Global.pInfo.decarbo_time = jsonNode["decarbo_time"].AsInt;
            Global.pInfo.total_amount = jsonNode["total_amount"].AsInt;
            Global.pInfo.remain_amount = jsonNode["remain_amount"].AsInt;
            Global.pInfo.decarbonation = jsonNode["decarbonation"].AsInt;
            Global.pInfo.open_time = jsonNode["opentime"].AsInt;
            Global.pInfo.is_soldout = jsonNode["soldout"].AsInt;
            if (jsonNode["soldout"].AsInt == 1)
            {
                Global.pInfo.sceneType = WorkSceneType.soldout;
            }
            else
            {
                Global.pInfo.sceneType = WorkSceneType.standby;
            }
            string downloadImgUrl = Global.image_server_path + "Standby" + jsonNode["server_id"].AsInt + ".jpg";
            StartCoroutine(downloadFile(downloadImgUrl, Global.imgPath + Path.GetFileName(downloadImgUrl)));
            showWorkScene();
        }
        catch (Exception err)
        {
            Debug.Log(err);
        }
    }

    public void flowmeterStartEventHandler(SocketIOEvent e)
    {
        try
        {
            Debug.Log("[SocketIO] FlowmeterStartEvent received: " + e.name + " " + e.data);
            JSONNode jsonNode = SimpleJSON.JSON.Parse(e.data.ToString());
            int id = jsonNode["id"].AsInt;
            if (id != Global.pInfo.no)
                return;
            Global.pInfo.sceneType = WorkSceneType.pour;
            Global.pInfo.is_soldout = 0;
            showWorkScene();
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public void flowmeterValueEventHandler(SocketIOEvent e)
    {
        try
        {
            Debug.Log("[SocketIO] FlowmeterValueEvent received: " + e.name + " " + e.data);
            JSONNode jsonNode = SimpleJSON.JSON.Parse(e.data.ToString());
            int id = jsonNode["id"].AsInt;
            if (id != Global.pInfo.no)
                return;
            int value = jsonNode["value"].AsInt;
            contentObj.text = Global.GetPriceFormat(value) + " ml";
        }
        catch (Exception err)
        {
            Debug.Log(err);
        }
    }

    public void onConfirmDeviceCheckingPopup()
    {
        devicecheckingPopup.SetActive(false);
        tagControl(1);
        showSettingScene(false);
        Global.pInfo.sceneType = WorkSceneType.standby;
        showWorkScene();
    }

    public void flowmeterFinishEventHandler(SocketIOEvent e)
    {
        try
        {
            Debug.Log("[SocketIO] FinishEvent received: " + e.name + " " + e.data);
            JSONNode jsonNode = SimpleJSON.JSON.Parse(e.data.ToString());
            int id = jsonNode["id"].AsInt;
            if (id != Global.pInfo.no)
                return;
            int type = jsonNode["type"].AsInt;//0-pour, 1-soldout, 2-remain
            int value = jsonNode["value"].AsInt;
            if (type == 1)
            {
                //soldout 완료
                contentObj.text = Global.GetPriceFormat(value) + " ml";
                StartCoroutine(GotoSoldout());
            }
            else if (type == 2)
            {
                //remain 완료
                Global.pInfo.is_soldout = 0;
                Global.pInfo.sceneType = WorkSceneType.pour;
                showWorkScene(value);
                StartCoroutine(ReturntoRemain(jsonNode["remain_value"].AsInt));
            }
            else
            {
                //정상완료
                Global.pInfo.is_soldout = 0;
                int is_pay_after = jsonNode["is_pay_after"].AsInt;
                Global.pInfo.sceneType = WorkSceneType.pour;
                showWorkScene(value);
                if (is_pay_after == 1)
                {
                    //후불
                    StartCoroutine(ReturntoStandby());
                }
                else
                {
                    //선불
                    StartCoroutine(ReturntoRemain(jsonNode["remain_value"].AsInt));
                }
            }
        }
        catch (Exception err)
        {
            Debug.Log(err);
        }
    }

    IEnumerator GotoSoldout()
    {
        yield return new WaitForSeconds(1f);
        Global.pInfo.is_soldout = 1;
        Global.pInfo.sceneType = WorkSceneType.soldout;
        showWorkScene();
    }

    IEnumerator ReturntoRemain(int remain_value)
    {
        yield return new WaitForSeconds(1f);
        Global.pInfo.sceneType = WorkSceneType.remain;
        showWorkScene(remain_value);
        yield return new WaitForSeconds(3f);
        Global.pInfo.sceneType = WorkSceneType.standby;
        showWorkScene();
    }

    IEnumerator ReturntoStandby()
    {
        yield return new WaitForSeconds(1f);
        Global.pInfo.sceneType = WorkSceneType.standby;
        showWorkScene();
    }

    IEnumerator closeServerErrPopup()
    {
        yield return new WaitForSeconds(3f);
        splash_err_popup.SetActive(false);
        showSettingScene(false);
        Global.pInfo.sceneType = WorkSceneType.standby;
        showWorkScene();
    }

    public void errorReceivedEventHandler(SocketIOEvent e)
    {
        Debug.Log("errorReceivedEvent:" + e.data);
        //type
        try
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(e.data.ToString());
            int id = jsonNode["id"].AsInt;
            Debug.Log("id:" + id);
            Debug.Log("no:" + Global.pInfo.no);
            if (Global.pInfo.no != id)
            {
                return;
            }
            int type = jsonNode["type"].AsInt;
            Debug.Log("type:" + type);
            if (type == 1)
            {
                splash_err_content.text = jsonNode["content"];
                splash_err_popup.SetActive(true);
                StartCoroutine(closeServerErrPopup());
            }
            else
            {
                int is_close = jsonNode["is_close"].AsInt;
                Debug.Log("is_close:" + is_close);
                if (is_close == 1)
                {
                    splash_err_popup.SetActive(false);
                }
                else
                {
                    splash_err_content.text = jsonNode["content"];
                    splash_err_popup.SetActive(true);
                }
            }
        }
        catch (Exception err)
        {
            Debug.Log(err);
        }
    }

    public void adminReceivedEventHandler(SocketIOEvent e)
    {
        try
        {
            Debug.Log("[SocketIO] AdminReceivedEvent received: " + e.name + " " + e.data);
            JSONNode jsonNode = SimpleJSON.JSON.Parse(e.data.ToString());
            int id = jsonNode["id"].AsInt;
            if (Global.pInfo.no != id)
            {
                return;
            }
            showSettingScene();
        }
        catch (Exception err)
        {
            Debug.Log(err);
        }
    }

    public void openShopEventHandler(SocketIOEvent e)
    {
        Debug.Log("shopOpenEvent");
        shop_open_popup.SetActive(true);
        StartCoroutine(waitShopOpen());
    }

    IEnumerator waitShopOpen()
    {
        yield return new WaitForSeconds(Global.pInfo.open_time);
        onConfirmShopOpenPopup(false);
    }

    public void closeShopEventHandler(SocketIOEvent e)
    {
        try
        {
            int percentage = Global.pInfo.remain_amount * 100 / Global.pInfo.total_amount;
            if (percentage > Global.pInfo.decarbonation)
                return;
            decarbonate_popup.SetActive(true);
            StartCoroutine(shopDecarbonate());
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public void RepairingDevice(SocketIOEvent e)
    {
        try
        {
            Debug.Log("[SocketIO] Reparing Device Event received: " + e.name + " " + e.data);
            JSONNode jsonNode = SimpleJSON.JSON.Parse(e.data.ToString());
            int id = jsonNode["id"].AsInt;
            if (id == Global.pInfo.no)
            {
                devicecheckingPopup.SetActive(true);
            }
        }
        catch (Exception err)
        {
            Debug.Log(err);
        }
    }

    public void soldoutEventHandler(SocketIOEvent e)
    {
        try
        {
            Debug.Log("[SocketIO] Soldout received: " + e.name + " " + e.data);
            JSONNode jsonNode = SimpleJSON.JSON.Parse(e.data.ToString());
            int id = jsonNode["id"].AsInt;
            if(id == Global.pInfo.no)
            {
                Global.pInfo.is_soldout = 1;
                Global.pInfo.sceneType = WorkSceneType.soldout;
                showWorkScene();
            }
        }
        catch (Exception err)
        {
            Debug.Log(err);
        }
    }

    public void socketError(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
    }

    public void socketClose(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
        is_socket_open = false;
    }

    public void onConfirmShopOpenPopup(bool is_manual = true)
    {
        if (is_manual)
        {
            tagControl(1);
            valveControl(0, 0);
        }
        shop_open_popup.SetActive(false);
        showSettingScene(false);
        Global.pInfo.sceneType = WorkSceneType.standby;
        showWorkScene();
    }

    public void onConfirmDecarbonatePopup()
    {
        Debug.Log("confirm decarbonation");
        decarbonate_popup.SetActive(false);
        showSettingScene(false);
        Global.pInfo.sceneType = WorkSceneType.standby;
        showWorkScene();
        tagControl(1);
        valveControl(1, 0);
    }

    //download image
    IEnumerator downloadFile(string url, string pathToSaveImage)
    {
        yield return new WaitForEndOfFrame();
        if (!File.Exists(pathToSaveImage))
        {
            WWW www = new WWW(url);
            StartCoroutine(_downloadFile(www, pathToSaveImage));
        }
    }

    IEnumerator downloadAndLoadImage(string url, string pathToSaveImage, RawImage img)
    {
        try
        {
            if (img != null)
            {
                if (File.Exists(pathToSaveImage))
                {
                    StartCoroutine(LoadPictureToTexture(pathToSaveImage, img));
                }
                else
                {
                    WWW www = new WWW(url);
                    StartCoroutine(_downloadAndLoadImage(www, pathToSaveImage, img));
                }
            }
        }
        catch (Exception ex)
        {

        }
        yield return null;
    }

    private IEnumerator _downloadAndLoadImage(WWW www, string savePath, RawImage img)
    {
        yield return www;
        if (img != null)
        {
            //Check if we failed to send
            if (string.IsNullOrEmpty(www.error))
            {
                saveAndLoadImage(savePath, www.bytes, img);
            }
            else
            {
                UnityEngine.Debug.Log("Error: " + www.error);
            }
        }
    }

    void saveAndLoadImage(string path, byte[] imageBytes, RawImage img)
    {
        try
        {
            //Create Directory if it does not exist
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllBytes(path, imageBytes);
            //Debug.Log("Download Image: " + path.Replace("/", "\\"));
            StartCoroutine(LoadPictureToTexture(path, img));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
    }

    IEnumerator LoadPictureToTexture(string name, RawImage img)
    {
        //Debug.Log("load image = " + Global.prePath + name);
        WWW pictureWWW = new WWW(Global.prePath + name);
        yield return pictureWWW;
        try
        {
            if (img != null)
            {
                //img.sprite = Sprite.Create(pictureWWW.texture, new Rect(0, 0, pictureWWW.texture.width, pictureWWW.texture.height), new Vector2(0, 0), 8f, 0, SpriteMeshType.FullRect);
                img.texture = pictureWWW.texture;
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    private IEnumerator _downloadFile(WWW www, string path)
    {
        yield return www;
        //Check if we failed to send
        if (string.IsNullOrEmpty(www.error))
        {
            try
            {
                //Create Directory if it does not exist
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                File.WriteAllBytes(path, www.bytes);
                //Debug.Log("Download Image: " + path.Replace("/", "\\"));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
                Debug.LogWarning("Error: " + e.Message);
            }
        }
    }

    int order = 0;
    public void onClickOrder3()
    {

        if (order == 0)
        {
            order = 1;
        }
        else if (order == 1)
        {
            order = 2;
        }
        else if (order == 2)
        {
            order = 3;
        }
        else if (order == 3)
        {
            tagControl(0);
            showSettingScene();
            order = 0;
        }
        else
        {
            order = 0;
        }
    }

    public void onClickOrder4()
    {
        if (Global.pInfo.is_soldout == 1)
        {
            WWWForm form = new WWWForm();
            form.AddField("serial_number", Global.pInfo.no);
            WWW www = new WWW(Global.api_url + Global.cancel_soldout_api, form);
            StartCoroutine(CancelSoldout(www));
        }
    }

    IEnumerator CancelSoldout(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            int result = jsonNode["suc"].AsInt;
            if (result == 1)
            {
                showSettingScene(false);
                Global.pInfo.sceneType = WorkSceneType.standby;
                showWorkScene();
                tagControl(1);
                err_popup.SetActive(false);
            }
            else
            {
                err_content.text = "서버와의 조작시 알지 못할 오류가 발생하였습니다.";
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_content.text = "서버와의 조작시 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }

    }

    IEnumerator Soldout(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            int result = jsonNode["suc"].AsInt;
            if (result == 1)
            {
                showSettingScene(false);
                Global.pInfo.is_soldout = 1;
                Global.pInfo.sceneType = WorkSceneType.soldout;
                showWorkScene();
            }
            else
            {
                err_content.text = "서버와의 조작시 알지 못할 오류가 발생하였습니다.";
                err_popup.SetActive(true);
            }
        }
        else
        {
            err_content.text = "서버와의 조작시 알지 못할 오류가 발생하였습니다.";
            err_popup.SetActive(true);
        }
    }

    void tagControl(int status)
    {
        if(socket != null)
        {
            string tagGWData = "{\"tagGW_no\":\"" + Global.pInfo.tagGW_no + "\"," +
                "\"ch_value\":\"" + Global.pInfo.tagGW_channel + "\"," +
            "\"status\":\"" + status + "\"}";
            socket.Emit("deviceTagLock", JSONObject.Create(tagGWData));
        }
    }

    void valveControl(int valve, int type)
    {
        if(socket != null)
        {
            string data = "{\"board_no\":\"" + Global.pInfo.board_no + "\"," +
                "\"ch_value\":\"" + Global.pInfo.board_channel + "\"," +
            "\"valve\":\"" + valve + "\"," +
            "\"status\":\"" + type + "\"}";
            socket.Emit("boardValveCtrl", JSONObject.Create(data));
        }
    }

    float time = 0f;
    private bool is_socket_open = false;

    void FixedUpdate()
    {
        if (!Input.anyKey)
        {
            time += Time.deltaTime;
        }
        else
        {
            if (time != 0f)
            {
                soundObjs[2].Play();
                time = 0f;
            }
        }
    }
}
