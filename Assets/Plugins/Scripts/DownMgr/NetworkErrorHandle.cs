using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;
using UnityEngine;
using System.Linq;

public class NetworkErrorHandle
{
    //添加一个网络异常处理的功能,mycontinue跳过当前这个下载下一个
    public static void Handle(string fileName, Exception e, Action giveup, Action again, Action finished = null)
    {
        Logger.Error(e.Message + "\n[Track]" + e.StackTrace);
        Action action = () =>
        {
            //Logger.Log("webclient error:" + e.Message);
            if (e.Message.Contains("ConnectFailure") //连接失败
                || e.Message.Contains("NameResolutionFailure") //域名解析失败
                || e.Message.Contains("No route to host")) //找不到主机
            {
                Logger.Error("-----------------Webclient ConnectFailure-------------");
                //异常时弹出网络设置对话框
                ShowMsgBoxForNetworkDisconnect(again, ":" + e.Message);
            }
            else
                //(404) Not Found
                if (e.Message.Contains("(404) Not Found") || e.Message.Contains("403"))
                {
                    Logger.Error("-----------------WebClient NotFount-------------");
                    //抛出一个error,并且继续下载下一个
                    //mycontinue();
                    //服务器维护中，请稍后再试
                    ShowMsgForServerMaintenance(again, ":" + e.Message);
                }
                else
                    //Disk full             
                    if (e.Message.Contains("Disk full"))
                    {
                        Logger.Error("-----------------WebClient Disk full-------------");
                        ShowMsgBoxForDiskFull(fileName, again, ":" + e.Message);
                    }
                    else
                        //timed out
                        if (e.Message.Contains("timed out") || e.Message.Contains("Error getting response stream"))
                        {
                            Logger.Error("-----------------WebClient timed out-------------");
                            //again();
                            ShowMsgForTimeout(again, ":" + e.Message);
                        }
                        else
                            //Sharing violation on path
                            if (e.Message.Contains("Sharing violation on path"))
                            {
                                Logger.Error("-----------------WebClient Sharing violation on path-------------");
                                again();
                            }
                            else
                            {
                                ShowMsgForUnknown(again, ":" + e.Message);
                            }
        };
        GameStart.Invoke(action);
    }
    //未知的网络错误
    private static void ShowMsgForUnknown(Action again, string msg = "")
    {
        Logger.Log("ShowMsgForUnknown");
        Action act = () =>
        {
            PluginMsgAction.Instance.ShowMsgBox("网络异常", "重试", "退出", () =>
            {
                PluginMsgAction.Instance.HideMsgBox();
                again();
            },
            () =>
            {
                Application.Quit();
            });
        };
        GameStart.Invoke(act);
    }
    //超时，采用倒计时对话框
    private static void ShowMsgForTimeout(Action again, string msg = "")
    {
        Logger.Log("ShowMsgForTimeout");
        Action act = () =>
        {
            PluginMsgAction.Instance.ShowMsgBox("网络异常", "重试", "退出", () =>
            {
                PluginMsgAction.Instance.HideMsgBox();
                again();
            },
            () =>
            {
                Application.Quit();
            });
        };
        GameStart.Invoke(act);
    }
    //服务器维护中，下载文件404/403时使用
    private static void ShowMsgForServerMaintenance(Action again, string msg = "")
    {
        Logger.Log("ShowMsgForServerMaintenance");
        Action act = () =>
        {
            PluginMsgAction.Instance.ShowMsgBox("服务器维护中,请稍后再试", "重试", "退出", () =>
            {
                PluginMsgAction.Instance.HideMsgBox();
                again();
            },
            () =>
            {
                Application.Quit();
            });
        };
        GameStart.Invoke(act);
    }
    private static void ShowMsgBoxForNetworkDisconnect(Action again, string msg = "")
    {
        Logger.Log("ShowMsgBoxForNetworkDisconnect");
        Action act = () =>
        {
            PluginMsgAction.Instance.ShowMsgBox("网络异常", "重试", "设置网络", () =>
            {
                PluginMsgAction.Instance.HideMsgBox();
                again();
            },
            () =>
            {
#if UNITY_ANDROID
                var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var mMainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                mMainActivity.Call("gotoNetworkSetting");
#endif
            });
        };
        GameStart.Invoke(act);
    }

    public static void ShowMsgBoxForWWWError(Action again, string msg = "")
    {
        Logger.Log("ShowMsgBoxForWWWError " + msg);
        Action act = () =>
        {
            PluginMsgAction.Instance.ShowMsgBox("下载失败:" + msg, "重试", "退出", () =>
            {
                PluginMsgAction.Instance.HideMsgBox();
                again();
            },
            () =>
            {
                Application.Quit();
            });
        };
        GameStart.Invoke(act);
    }

    private static void ShowMsgBoxForDiskFull(string fileName, Action again, string msg = "")
    {
        Logger.Log("ShowMsgBoxForDiskFull:" + fileName);
        if (File.Exists(fileName))
        {
            Logger.Log("Delete:" + fileName);
            File.Delete(fileName);
        }
        Action act = () =>
        {
            PluginMsgAction.Instance.ShowMsgBox("SD卡空间不足", "重试", "退出", () =>
            {
                PluginMsgAction.Instance.HideMsgBox();
                again();
            },
            () =>
            {
                Application.Quit();
            });
        };
        GameStart.Invoke(act);
    }
}