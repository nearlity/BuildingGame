using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

public class LogWriter : MonoBehaviour
{
    private static string _logPath;
    public static string m_logFileName = "log_{0}.txt";
    private string m_logFilePath;
    private FileStream m_fs;
    private StreamWriter m_sw;
    private Action<String> m_logWriter;
    private List<string> _logList = new List<string>();
    private readonly object m_locker = new object();
    public static string LogPath
    {
        get 
        {
            if (string.IsNullOrEmpty(_logPath))
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                        _logPath = Application.persistentDataPath + "/../hotupdate_log/";
                        break;

                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                    default:
                        _logPath = Application.dataPath + "/../hotupdate_log/";
                        break;
                }
            }
            return _logPath; 
        }
    }

    private static GameObject _logGo;

    public static LogWriter GetWriter()
    {
        //Logger.Assert(_logGo == null, "logWriter must be Singleton");
        _logGo = new GameObject("logWriter");
        return _logGo.AddComponent<LogWriter>();
    }
    public void Awake()
    {
        if (!Directory.Exists(LogPath))
            Directory.CreateDirectory(LogPath);
        m_logFilePath = String.Concat(LogPath, String.Format(m_logFileName, DateTime.Today.ToString("yyyyMMdd")));
        try
        {
            m_fs = new FileStream(m_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            m_sw = new StreamWriter(m_fs);
            m_sw.WriteLine("=====================");
            m_sw.Flush();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("[LogWriter]" + ex.Message);
        }
    }

    public void Release()
    {
        lock (m_locker)
        {
            if (m_sw != null)
            {
                m_sw.Close();
                m_sw.Dispose();
                m_sw = null;
            }
            if (m_fs != null)
            {
                m_fs.Close();
                m_fs.Dispose();
                m_fs = null;
            }
        }
        _logList.Clear();
        _logList = null;
        GameObject.Destroy(_logGo);
    }

    void OnDestroy()
    {
        lock (m_locker)
        {
            try
            {
                if (m_sw != null)
                {
                    m_sw.Close();
                    m_sw.Dispose();
                    m_sw = null;
                }
                if (m_fs != null)
                {
                    m_fs.Close();
                    m_fs.Dispose();
                    m_fs = null;
                }
            }
            catch (IOException ex)
            {
                string msg = "写文件异常 " + ex.Message;
                if (ex.Message.Contains("Disk full"))
                    msg = "磁盘空间不足, 请清理出足够的空间以便游戏运行";
                PluginMsgAction.Instance.ShowMsgBox(msg, "确定", "退出", () =>
                {
                    PluginMsgAction.Instance.HideMsgBox();
                },
                () =>
                {
                    Application.Quit();
                });
            }
        }
    }

    public void WriteLog(string msg)
    {
        lock(m_locker)
        {
            if (_logList != null)
                _logList.Add(msg);
        }
    }

    void Update()
    {
        lock (m_locker)
        {
            if (m_sw == null)
                return;
            try
            {
                for (int i = 0; i < _logList.Count; i++)
                {
                    m_sw.WriteLine(_logList[i]);
                }
                _logList.Clear();
                m_sw.Flush();
            }
            catch(IOException ex)
            {
                string msg = "写文件异常 " + ex.Message;
                if (ex.Message.Contains("Disk full"))
                    msg = "磁盘空间不足, 请清理出足够的空间以便游戏运行";
                PluginMsgAction.Instance.ShowMsgBox(msg, "确定", "退出", () =>
                {
                    PluginMsgAction.Instance.HideMsgBox();
                },
                () =>
                {
                    Application.Quit();
                });
            }
        }
    }
}