using UnityEngine;
using System.Collections;
using System;

public class PlatformSDKPlugin : MonoBehaviour
{
    protected static PlatformSDKPlugin _instance = null;
    public static PlatformSDKPlugin instance
    {
        get
        {
            if(_instance == null)
            {
                GameObject go = new GameObject("_PlatformSDK");
                GameObject.DontDestroyOnLoad(go);
#if !UNITY_EDITOR && UNITY_ANDROID
                _instance = go.AddComponent<PlatformSDKAndroidFNPlugin>();
#elif !UNITY_EDITOR && UNITY_IPHONE
                _instance = go.AddComponent<PlatformSDKIOSPlugin>();
#else
                _instance = go.AddComponent<PlatformSDKDumyPlugin>();
#endif
                _instance.Init();
            }

            return _instance;
        }
    }
   
    protected virtual void Init()
    {
        Logger.Log("PlatformSDKPlugin type={0}", this.GetType().ToString());
    }

    #region sdk function
    public virtual void CheckBinaryVersion(Action finishedCB)
    {
    }
    public virtual string GetCodeVersion()
    {
        return null;
    }
    public virtual string GetVersionName()
    {
        return null;
    }
    public virtual bool Restart()
    {
        return false;
    }

    public virtual bool OpenNetworkSetting()
    {
        return false;
    }
    #endregion
}

