using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR || UNITY_ANDROID
public class PlatformSDKAndroidPlugin : PlatformSDKPlugin
{
    public const string CODE_PACKAGE = "com.xsfh.union";
    protected AndroidJavaObject _actObj = null;
    protected AndroidJavaClass _utilsCls = null;
    public Action<string> logDelegate = null;

    protected override void Init()
    {
        base.Init();
        
        try
        {
            var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _actObj = jc.GetStatic<AndroidJavaObject>("currentActivity");
            _utilsCls = new AndroidJavaClass(CODE_PACKAGE + ".util.PlatformUtils");
            //Logger.Assert(_actObj != null, "PlatformSDKAndroidPlguin.Init _actObj is null");
        }
        catch(System.Exception e)
        {
            Logger.Error("PlatformSDKAnroidPlugin Init error: {0}", e.ToString());
        }
    }

    public override bool Restart()
    {
        _utilsCls.CallStatic("restartApplication");
        return true;
    }
    
    public override bool OpenNetworkSetting()
    {
        _utilsCls.CallStatic("openNetworkSetting");
        return true;
    }
    public override string GetCodeVersion()
    {
        try
        {
            Dictionary<string, object> statDict;

            var sdkObj = _actObj.Call<AndroidJavaObject>("getSdkModel");
            var utilsCls = new AndroidJavaClass(CODE_PACKAGE + ".util.PlatformUtils");
            //init stat
            var statJsonStr = utilsCls.CallStatic<string>("getStatistics");
            statDict = MiniJSON.Json.Deserialize(statJsonStr) as Dictionary<string, object>;
            sdkObj.Dispose();
            utilsCls.Dispose();
            return statDict["version_code"] as string;
        }
        catch (System.Exception e)
        {
            Logger.Error("GameStart.GetVersionCode Init error: {0}", e.ToString());
        }
        return null;
    }
    public override string GetVersionName()
    {
        try
        {
            Dictionary<string, object> statDict;

            var sdkObj = _actObj.Call<AndroidJavaObject>("getSdkModel");
            var utilsCls = new AndroidJavaClass(CODE_PACKAGE + ".util.PlatformUtils");

            //init stat
            var statJsonStr = utilsCls.CallStatic<string>("getStatistics");
            statDict = MiniJSON.Json.Deserialize(statJsonStr) as Dictionary<string, object>;
            sdkObj.Dispose();
            utilsCls.Dispose();
            return statDict["version_name"] as string;
        }
        catch (System.Exception e)
        {
            Logger.Error("GameStart.GetVersionName Init error: {0}", e.ToString());
        }
        return null;
    }

    #region platform callback
    protected void Log(string data)
    {
        if(logDelegate != null)
            logDelegate.Invoke(data);
        else
            Logger.Log("[AndroidPluginLog]:{0}", data);
    }
    #endregion
}
#endif

