using UnityEngine;
using System;
using System.Runtime.InteropServices;
#if UNITY_EDITOR || UNITY_IPHONE
public class PlatformSDKIOSPlugin : PlatformSDKPlugin
{
    protected Action _checkBinaryFinishedCB = null;
    protected override void Init()
    {
        base.Init();
    }
    public override bool Restart()
    {
        //退出自动重启游戏
        Application.Quit();
        return true;
    }
    public override string GetCodeVersion()
    {
        return _C_GetVersionCode();
    }
    public override string GetVersionName()
    {
        return _C_GetVersionName();
    }
    public override bool OpenNetworkSetting()
    {
        //打开设置网络
        return _C_OpenNetworkSetting();
    }
    public override void CheckBinaryVersion(Action finishedCB)
    { 
        _checkBinaryFinishedCB = finishedCB;
    }
    #region platform callback
    protected virtual void CheckBinaryVersionFinished()
    {
        if (_checkBinaryFinishedCB != null)
            _checkBinaryFinishedCB.Invoke();
    }
    #endregion

    #region Plugin callback
    [DllImport("__Internal")]
    private static extern bool _C_OpenNetworkSetting();
    [DllImport("__Internal")]
    private static extern string _C_GetVersionCode();
    [DllImport("__Internal")]
    private static extern string _C_GetVersionName();
    #endregion
}

#endif
