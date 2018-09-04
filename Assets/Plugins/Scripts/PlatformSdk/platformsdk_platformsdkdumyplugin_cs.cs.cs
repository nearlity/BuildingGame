using UnityEngine;
using System.Collections;

public class PlatformSDKDumyPlugin : PlatformSDKPlugin
{
    public override void CheckBinaryVersion(System.Action finishedCB)
    {
        if(finishedCB != null)
            finishedCB.Invoke();
    }
}

