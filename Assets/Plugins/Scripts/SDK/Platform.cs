using System;
using System.ComponentModel;
using UnityEngine;

namespace Plugins
{
    public class Platform
    {
        public static EditorPlatform editorPlatform =
#if UNITY_WEBPLAYER
        EditorPlatform.WindowsWebPlayer;
#elif UNITY_ANDROID
        EditorPlatform.Android;
#elif UNITY_IPHONE
        EditorPlatform.IPhonePlayer;
#elif UNITY_STANDALONE
        EditorPlatform.WindowsPlayer;
#else
	    EditorPlatform.WindowsPlayer;
#endif

        static  public bool IsWindowsEditorPlatform
        {
            get { return editorPlatform == EditorPlatform.WindowsPlayer || editorPlatform == EditorPlatform.WindowsWebPlayer; }
        }

        public static bool IsMobileEditorPlatform
        {
            get { return editorPlatform == EditorPlatform.Android || editorPlatform == EditorPlatform.IPhonePlayer; }
        }
    }

    public enum EditorPlatform
    {
        [Obsolete("do NOT use this.")]
        OSXEditor = 0,
        OSXPlayer = 1,
        WindowsPlayer = 2,
        OSXWebPlayer = 3,
        OSXDashboardPlayer = 4,
        WindowsWebPlayer = 5,
        [Obsolete("do NOT use this.")]
        WindowsEditor = 7,
        IPhonePlayer = 8,
        PS3 = 9,
        XBOX360 = 10,
        Android = 11,
        [Obsolete("NaCl export is no longer supported in Unity 5.0+.")]
        NaCl = 12,
        LinuxPlayer = 13,
        [Obsolete("FlashPlayer export is no longer supported in Unity 5.0+.")]
        FlashPlayer = 15,
        WebGLPlayer = 17,
        [Obsolete("Use WSAPlayerX86 instead")]
        MetroPlayerX86 = 18,
        WSAPlayerX86 = 18,
        [Obsolete("Use WSAPlayerX64 instead")]
        MetroPlayerX64 = 19,
        WSAPlayerX64 = 19,
        [Obsolete("Use WSAPlayerARM instead")]
        MetroPlayerARM = 20,
        WSAPlayerARM = 20,
        WP8Player = 21,
        [Obsolete("BB10Player has been deprecated. Use BlackBerryPlayer instead (UnityUpgradable) -> BlackBerryPlayer", true), EditorBrowsable(EditorBrowsableState.Never)]
        BB10Player = 22,
        BlackBerryPlayer = 22,
        TizenPlayer = 23,
        PSP2 = 24,
        PS4 = 25,
        PSM = 26,
        XboxOne = 27,
        SamsungTVPlayer = 28,
        WiiU = 30,
    }
}

