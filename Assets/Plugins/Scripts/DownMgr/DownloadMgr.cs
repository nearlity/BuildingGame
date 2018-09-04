using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;
using UnityEngine;
using System.Linq;

public class DownloadMgr
{
    public static int LOADER_MAXIMUM = 5;
    public static int MAX_COUNT = 5;	
    private static DownloadMgr _mInstance;
    public static DownloadMgr Instance
    {
        get { return _mInstance ?? (_mInstance = new DownloadMgr()); }
    }
    private readonly WebClient _textLoader = new WebClient();
    private List<DownloadTask> _taskList = new List<DownloadTask>();
    private WebLoader[] _loaders = new WebLoader[LOADER_MAXIMUM];
    //添加一个taskProgress的回调,第一个参数总任务数，第二个是当前任务索引,第三个是下载的文件名
    public Action<int, int> _totalProgress;
    //全部下载完成的回调函数
    public Action AllDownloadFinished;
    //任务完成时解压文件的回调函数
    public DownloadMgr()
    {

    }

    public void Init()
    {
        GameObject go = new GameObject();
        go.name = "DownloadMgr";
        for (int i = 0; i < LOADER_MAXIMUM; i++)
        {
            WebLoader loader = go.AddComponent<WebLoader>();
            loader.Init(this);
            _loaders[i] = loader;
        }
    }

    public void AddTask(string webPath, string itemPath, string md5, Action<string, WWW> loaded, Action<string, float> progress)
    {
        DownloadTask task = new DownloadTask();
        task.Url = webPath;
        task.itemPath = itemPath;
        task.Progress = progress;
        task.Finished = loaded;
        task.MD5 = md5;
        _taskList.Add(task);
        CheckDownLoadList();
    }

    public string DownLoadText(String url)
    {
        try
        {
            string address = GetRandomParasUrl(url);
            Logger.Log("[DownLoadText]" + address);
            _textLoader.Encoding = System.Text.Encoding.UTF8;
            string result = _textLoader.DownloadString(address);
            //Logger.Log("ret = " + result);
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message + "\n[TRACK]" + ex.StackTrace);
            return String.Empty;
        }
    }

    public void AsynDownLoadText(String url, Action<String> asynResult, Action OnError)
    {
        //var u = GetRandomParasUrl(url);    DownLoadText里面又GetRandomParasUrl,所以这里不用了
        var u = url;
        Action action = () =>
        {
            var result = DownLoadText(u);
            if (String.IsNullOrEmpty(result))
            {
                if (OnError != null)
                {
#if UNITY_IPHONE
					OnError.DynamicInvoke();
#else
                    OnError();
#endif
                }
            }
            else
            {
                if (asynResult != null)
                {
#if UNITY_IPHONE
					asynResult.DynamicInvoke(result);
#else
                    asynResult(result);
#endif
                    //UnityEngine.Debug.Log(result);
                }
            }
        };
        action.BeginInvoke(null, null);
    }

    public WebLoader GetIdleLoader()
    {
        for (int i = 0; i < _loaders.Length; i++)
        {
            if (_loaders[i].isLoading)
                continue;
            return _loaders[i];
        }
        return null;
    }
    
    //开始、继续、从新下载
    public void CheckDownLoadList()
    {
        if (_taskList.Count == 0) 
            return;
        WebLoader loader = GetIdleLoader();
        if (loader == null)
            return;
        var finishedCount = 0;//已经完成的数目
        for (int i = 0; i < _taskList.Count; i++)
        {
            DownloadTask task = _taskList[i];
            switch (task.state)
            {
                case DownloadTaskState.Failed:
                    Logger.Error(task.itemPath + " giveup");
                    _taskList.RemoveAt(i);
                    i--;
                    task.OnError();
                    break;
                case DownloadTaskState.Success:
                    //Logger.Log("完成任务：" + task.itemPath);
                    finishedCount++;
                    _taskList.RemoveAt(i);
                    task.OnFinished();
                    i--;
                    break;
                case DownloadTaskState.Wait:
                case DownloadTaskState.Retry:
                    if (task.retryCount >= MAX_COUNT)
                    {
                        Logger.Error("failed too much. give up" +  task.itemPath);
                        _taskList.RemoveAt(i);
                        i--;
                        task.OnError();
                        break;
                    }
                    
                    loader.StartLoad(task);
                    return;
            }
        }
        //如果全部完成,修改原来等于判断，不然最后一个或者只有一个下载任务时不会执行自己的finish
        if (_taskList.Count == 0)
        {
            if (AllDownloadFinished != null)
            {
                AllDownloadFinished();
                AllDownloadFinished = null;
            }
        }
    }

#if !UNITY_WEBPLAYER
    public bool DownloadFile(String webPath, String localPath, String bakPath)
    {
        if (File.Exists(bakPath))
            File.Delete(bakPath);
        if (File.Exists(localPath))
            File.Move(localPath, bakPath);
        var path = Path.GetDirectoryName(localPath);
        Logger.Log("path: " + localPath);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        try
        {
            _textLoader.DownloadFile(GetRandomParasUrl(webPath), localPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message + "\n[TRACK]" + ex.StackTrace);
            if (File.Exists(bakPath))
            {
                if (File.Exists(localPath))
                    File.Delete(localPath);
                File.Move(bakPath, localPath);
            }
            return false;
        }
        finally
        {
            if (File.Exists(bakPath))
                File.Delete(bakPath);
        }
    }
#endif

    public String GetRandomParasUrl(String url)
    {
        var r = Utils.CreateRandom();
        var u = String.Format("{0}?type={1}&ver={2}&sign={3}", url.Trim(), r.Next(100), r.Next(100), Guid.NewGuid().ToString().Substring(0, 8));
        UnityEngine.Debug.Log(u);
        return u;
    }

    public void Dispose()
    {
        _textLoader.CancelAsync();
        _textLoader.Dispose();
    }
}