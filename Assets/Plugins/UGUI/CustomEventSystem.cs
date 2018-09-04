using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class CustomEventSystem : EventSystem
{
    public static Action beforeUpdate = null;
    public static Action afterUpdate = null;

    protected override void Update()
    {
        if(beforeUpdate != null)
            beforeUpdate.Invoke();

        base.Update();

        if(afterUpdate != null)
            afterUpdate.Invoke();
    }
}

