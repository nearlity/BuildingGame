using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
#if PUBLISH_IPHONE
using System.Net.NetworkInformation;
#endif
using System.Net.Sockets;
using System.Text;

public class MobileInfo
{
    public static string GetMobileCurrentIPv4()
    {
#if PUBLISH_IPHONE
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface adapter in adapters)
        {
            if (adapter.Supports(NetworkInterfaceComponent.IPv4))
            {
                UnicastIPAddressInformationCollection uniCast = adapter.GetIPProperties().UnicastAddresses;
                if (uniCast.Count > 0)
                {
                    foreach (UnicastIPAddressInformation uni in uniCast)
                    {
                        if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return uni.Address.ToString();
                        }
                    }
                }
            }
        }
#else
        return Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
#endif
    }
}
