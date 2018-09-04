using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;
using UnityEngine;
using System.Linq;

public class WebLoader : MonoBehaviour
{
    private DownloadMgr _mgr;
    private DownloadTask _task;
    public bool isLoading = false;
    public void Init(DownloadMgr mgr)
    {
        _mgr = mgr;
    }

    public void StartLoad(DownloadTask task)
    {
        //Logger.Assert(task != null, " task can not be null");
        //Logger.Assert(!isLoading, " webLoad must be idle " + (_task == null ? "" : _task.Url));
        _task = task;
        _task.retryCount++;
        _task.state = DownloadTaskState.Loading;
        isLoading = true;
        //Logger.Log("StartWebLoad： " + _task.Url);
        StartCoroutine(Download(_task.Url));
    }

    private IEnumerator Download(string path)
    {
        WWW www = new WWW(path);
        while (!www.isDone)
        {
            if (string.IsNullOrEmpty(www.error))
            {
                Progress(www.progress);
                yield return null;			// 每帧查询
            }
            else
            {
                Logger.Log(www.error);
                Finished(null, www.error);
                yield break;
            }
        }
        if (!string.IsNullOrEmpty(www.error))
        {
            Logger.Log(www.error);
            Finished(null, www.error);
            yield break;
        }
        
        Finished(www, null);
        www = null;
    }

    void Progress(float p)
    {
        DownloadTask currentTask = _task;
        GameStart.Invoke(()=>
            {
                currentTask.OnProgress(p);
            });
    }

    void Finished(WWW www, string error)
    {
        DownloadTask currentTask = _task;
        if (www == null)
        {
            isLoading = false;
            NetworkErrorHandle.ShowMsgBoxForWWWError(() =>
                {
                    //跳过当前这个下载下一个
                    currentTask.state = DownloadTaskState.Retry;
                    _mgr.CheckDownLoadList();
                }, error);
            return;
        }
        Exception ex = CheckMd5(www, currentTask);
        if (ex != null)
        {
            isLoading = false;
            NetworkErrorHandle.Handle(currentTask.itemPath, ex,
                            () =>
                            {
                                //跳过当前这个下载下一个
                                currentTask.state = DownloadTaskState.Failed;
                                _mgr.CheckDownLoadList();
                            },
                            () =>
                            {
                                //从新下载这个
                                currentTask.state = DownloadTaskState.Retry;
                                _mgr.CheckDownLoadList();
                            });
            www.Dispose();
            return;
        }
        //Logger.Assert(currentTask.state == DownloadTaskState.Success, " task state must be Success " + currentTask.state);
        isLoading = false;
        currentTask.www = www;
        GameStart.Invoke(()=>
            {
                _mgr.CheckDownLoadList();
            });
    }

#if !UNITY_WEBPLAYER
    private Exception SaveLocalFile(byte[] content, string fileName)
    {
        try
        {
            string tempFile = fileName + "_temp";
            File.WriteAllBytes(tempFile, content);
            if (File.Exists(fileName))
                File.Delete(fileName);
            File.Move(tempFile, fileName);
        }
        catch (Exception ex)
        {
            return ex;
        }
        return null;
    }
#endif

    public static Exception CheckMd5(WWW www, DownloadTask task)
    {
        if (string.IsNullOrEmpty(task.MD5))
        {
            task.state = DownloadTaskState.Success;
            return null;
        }
            
        try
        {
#if UNITY_IPHONE
        //ios下如果封装该方法在一个函数中，调用该函数来产生文件的MD5的时候，就会抛JIT异常。
        //如果直接把这个产生MD5的方法体放在直接执行，就可以正常执行，这个原因还不清楚。
            string md5Compute = null;

			System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
			byte[] fileMD5Bytes = md5.ComputeHash(www.bytes);
            md5Compute = System.BitConverter.ToString(fileMD5Bytes).Replace("-", "").ToLower();
		    
#else
            var md5Compute = Utils.BuildFileMd5(www.bytes);
#endif
            //md5验证失败
            if (md5Compute == null || md5Compute.Trim() != task.MD5.Trim())
            {
                Logger.Error("MD5对比:" + task.itemPath + "--" + md5Compute + " vs " + task.MD5);
                task.state = DownloadTaskState.Retry;
                return new Exception("MD5验证失败，重新下载");
            }
            //所有通过验证就认为是下载完成
            task.state = DownloadTaskState.Success;
        }
        catch (Exception ex)
        {
            return ex;
        }
        return null;
    }
}