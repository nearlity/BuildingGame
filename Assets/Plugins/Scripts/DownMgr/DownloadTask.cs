using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;
using UnityEngine;
using System.Linq;

public enum DownloadTaskState
{
    Wait,
    Loading,
    Success,
    Failed,
    Retry,
}

public class DownloadTask
{
    public string Url;
    public string itemPath;
    public Action<string, float> Progress;
    public Action<long> TotalBytesToReceive;
    public Action<long> BytesReceived;
    public String MD5;
    public WWW www;
    public Action<string, WWW> Finished;
    public Action<Exception> Error;
    public DownloadTaskState state = DownloadTaskState.Wait;
    public int retryCount = 0;

    public void OnTotalBytesToReceive(long size)
    {
        if (TotalBytesToReceive != null)
            TotalBytesToReceive(size);
    }

    public void OnBytesReceived(long size)
    {
        if (BytesReceived != null)
            BytesReceived(size);
    }

    public void OnProgress(float p)
    {
        if (Progress != null)
            Progress(itemPath, p);
    }

    public void OnFinished()
    {
        if (Finished != null)
            Finished(itemPath, www);
        www = null;
    }

    public void OnError()
    {
        OnFinished();
    }

}