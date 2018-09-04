using System.Net;

public class NetInfo
{
    /// <summary>
    /// 通过传入的URL解析IP地址
    /// </summary>
    /// <param name="url">like "www.baidu.com"</param>
    /// <returns></returns>
    public static string GetIPAddress(string url)
    {
        IPHostEntry hostInfo = Dns.GetHostEntry(url);
        return hostInfo.AddressList[0].ToString();
    }
}