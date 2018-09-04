using System;
using System.Collections.Generic;
using UnityEngine;

public class PluginMsgAction
{
    private static PluginMsgAction m_instance;
    public static PluginMsgAction Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new PluginMsgAction();
            }
            return m_instance;
        }
    }
    public void ShowMsgBox(string content, string btn1, string btn2, Action callBack1, Action callBack2)
    {
        if (MessageNoticeUI.instance == null)
            return;
        MessageNoticeUI.instance.Show(content, btn1, btn2, callBack1, callBack2);

    }
    public void HideMsgBox()
    {
        if (MessageNoticeUI.instance == null)
            return;
        MessageNoticeUI.instance.Hide();
    }
}