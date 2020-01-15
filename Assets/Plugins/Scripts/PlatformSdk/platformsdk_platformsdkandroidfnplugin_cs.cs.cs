using UnityEngine;
using System.Collections;
using System;

#if UNITY_EDITOR || UNITY_ANDROID || ANDROID_MOBILE
public class PlatformSDKAndroidFNPlugin : PlatformSDKAndroidPlugin
{
    protected Action _checkBinaryFinishedCB = null;
    
    protected override void Init()
    {
        base.Init();
    }
    
    public override void CheckBinaryVersion(System.Action finishedCB)
    {
        //check binary is called in java code, waiting finished callback from java code
        _checkBinaryFinishedCB = finishedCB;
    }
    
    #region platform callback
    protected virtual void CheckBinaryVersionFinished()
    {
        if(_checkBinaryFinishedCB != null)
            _checkBinaryFinishedCB.Invoke();
    }
    #endregion
}
#endif
