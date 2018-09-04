using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Net;
using System.Xml;
using System.Text;
using System.IO;
using System.Threading;
using Plugins;

public class URLs
{
	public static string FILE_PROTOCOL = "file://";
	public static string HTTP_PROTOCOL = "http://";

	public static string serverCfgURL;

	public static string versionRootPath;
	private static string _persistentDataPath;
	static public string persistentDataPath
	{
		get
		{
			if (!string.IsNullOrEmpty(_persistentDataPath))
				return _persistentDataPath;



			switch (Application.platform)
			{
			case RuntimePlatform.OSXEditor:
				_persistentDataPath = string.Format("{0}/../Build/ios/SDCard/", Application.dataPath);
				break;
			case RuntimePlatform.WindowsEditor:
				_persistentDataPath = string.Format("{0}/../Build/windows/SDCard/", Application.dataPath);
				break;
			case RuntimePlatform.WindowsPlayer:
				_persistentDataPath = string.Format("{0}/", Application.dataPath);
				break;
			case RuntimePlatform.Android:
				string tempPersistentDataPath = Application.persistentDataPath;
				if (string.IsNullOrEmpty(tempPersistentDataPath))
				{
					tempPersistentDataPath = GetSDCardPath();
				}
				_persistentDataPath = string.Format("{0}/", tempPersistentDataPath);
				break;
			case RuntimePlatform.IPhonePlayer:
				_persistentDataPath = string.Format("{0}/", Application.persistentDataPath);
				break;
			case RuntimePlatform.WindowsWebPlayer:
				_persistentDataPath = string.Format("{0}/", Application.persistentDataPath);
				break;
			}
			return _persistentDataPath;
		}
	}

	public static string GetSDCardPath()
	{
		#if UNITY_ANDROID
		try
		{
		Dictionary<string, object> statDict;
		using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		{
		var utilsCls = new AndroidJavaClass(GameStart.CODE_PACKAGE + ".util.PlatformUtils");

		//init stat
		var statJsonStr = utilsCls.CallStatic<string>("getStatistics");
		statDict = MiniJSON.Json.Deserialize(statJsonStr) as Dictionary<string, object>;
		utilsCls.Dispose();
		}
		return statDict["sdcard_write_path"] as string;
		}
		catch (System.Exception e)
		{
		Logger.Error("GameStart.GetSDCardPath Init error: {0}", e.ToString());
		}
		#endif
		return null;
	}

	private static string _streamingAssetsPath;
	static public string streamingAssetsPath
	{
		get
		{
			if (!string.IsNullOrEmpty(_streamingAssetsPath))
				return _streamingAssetsPath;
			switch (Application.platform)
			{
			case RuntimePlatform.OSXEditor:
				_streamingAssetsPath = string.Format("file://{0}/../Build/ios/resources/", Application.dataPath);
				break;
			case RuntimePlatform.WindowsEditor:
				#if WINDOW_PACK
				_streamingAssetsPath = string.Format("{0}/", Application.streamingAssetsPath);
				#else
				_streamingAssetsPath = string.Format("{0}/../Build/windows/resources/", Application.dataPath);
				#endif
				break;
			case RuntimePlatform.WindowsPlayer:
				_streamingAssetsPath = string.Format("{0}/", Application.streamingAssetsPath);
				break;
			case RuntimePlatform.Android:
				_streamingAssetsPath = string.Format("{0}/", Application.streamingAssetsPath);
				break;
			case RuntimePlatform.IPhonePlayer:
				_streamingAssetsPath = string.Format("file://{0}/", Application.streamingAssetsPath);
				break;
			case RuntimePlatform.WindowsWebPlayer:
				_persistentDataPath = string.Format("{0}/", Application.streamingAssetsPath);
				break;
			}
			return _streamingAssetsPath;
		}
	}

	public static string GetShortPath(string localPathInSDCard)
	{
		int index = localPathInSDCard.IndexOf(URLs.persistentDataPath);
		string shortPath = localPathInSDCard.Substring(index + URLs.persistentDataPath.Length);
		return shortPath;
	}

	public static string GetVersionPath(int version, string file)
	{
		return string.Format("{0}{1}/{2}", versionRootPath, version, file);
	}
}

public class GameStart : MonoBehaviour
{
	public const string GAME_NAME = "侠影天下";
	private static string DLLFileName = "code/assembly-csharp.u";
	public static string LOCAL_MD5_ZIP = "apkmd5/md5.zip";  //必须跟apk包中相对路径一致  不然无法直接使用ExportFile接口
	public static string NEWEST_MD5_ZIP = "newestmd5/md5.zip";
	public static string LOCAL_MD5_FILE = "apkmd5/md5.txt";
	public static string NEWEST_MD5_FILE = "newestmd5/md5.txt";

	private static string DLLLoaclPath;
	public static string LocalMD5ZipFilePath;
	public static string ServerMD5ZipFilePath;
	public static string ServerMD5FilePath;
	public static string LocalRootCfgPath;

	private static String APKRootCfgPath;

	public static bool allowDownloadBy3G = false;

	private bool dllExportResult = false;
	private bool md5ExportResult = false;
	private float dllProgress = 0;
	private float md5Progress = 0;

	private static List<Action> _actions = new List<Action>();

	public static XmlDocument _localRootXML = new XmlDocument();
	public static XmlDocument _apkRootXML = new XmlDocument();
	public static XmlDocument _serverRootXML = new XmlDocument();
	public static GameStart instance;
	public int _localDLLVersion = 0;
	private int _serverDLLVersion = 0;
	public int _localMD5Version = 0; 
	private int _serverMD5Version = 0;
	private LogWriter _logWriter;
	private WebClient _webClient;

	public static string ApkVersionName;

	public string md5ZipBytes;

	void Awake()
	{
		instance = this;

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		GameObject uiGo = GameObject.Find("UI").gameObject;
		GameObject.DontDestroyOnLoad(uiGo);
		GameObject setupUIGo = uiGo.transform.Find("Canvas/SetupUI").gameObject;
		setupUIGo.AddComponent<SetupUI>();
		setupUIGo.SetActive(true);

		GameObject msgUIGo = uiGo.transform.Find("Canvas/MessageNoticeUI").gameObject;
		msgUIGo.SetActive(true);
		msgUIGo.AddComponent<MessageNoticeUI>();

		#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBPLAYER
		StandaloneInputModule inputModule = GameObject.Find("EventSystem").GetComponent<StandaloneInputModule>();
		inputModule.forceModuleActive = true;
		#endif
		Application.targetFrameRate = 30;
		InitEnvironment();
	}

	void InitEnvironment()
	{
		if (string.IsNullOrEmpty(Application.persistentDataPath))
		{
			ShowErrorInfo("SD卡检测失败,请退出游戏后重启手机", InitEnvironment);
			return;
		}
		if (Plugins.Platform.editorPlatform == Plugins.EditorPlatform.WindowsWebPlayer)
		{
			InitWebPlayer();
		}
		else
		{
			//BuglyUtils.InitBugly();

			DLLLoaclPath = URLs.persistentDataPath + DLLFileName;
			LocalMD5ZipFilePath = URLs.persistentDataPath + LOCAL_MD5_ZIP;
			ServerMD5ZipFilePath = URLs.persistentDataPath + NEWEST_MD5_ZIP;
			ServerMD5FilePath = URLs.persistentDataPath + NEWEST_MD5_FILE;
			LocalRootCfgPath = URLs.persistentDataPath + "apk_cfg.xml";
			APKRootCfgPath = URLs.streamingAssetsPath + "apk_cfg.xml";
		}

		Logger.isEditor = Application.platform == RuntimePlatform.WindowsEditor;
		Logger.logLevel = LogLevel.LL_Info;

		if (Plugins.Platform.editorPlatform != Plugins.EditorPlatform.WindowsWebPlayer)
		{
			Application.logMessageReceived += HandleMessageReceived;
			_logWriter = LogWriter.GetWriter();
			Logger.LogHandler += LogHandler;
		}

		DownloadMgr.Instance.Init();
		PlatformSDKPlugin.instance.ToString();
		SetupUI.instance.SetHint("平台初始化");
		PlatformSDKPlugin.instance.CheckBinaryVersion(
			()=>{
				SetupUI.instance.SetHint("网络检查中");
				CheckNetwork(TryHotUpdate);
			}
		);
	}

	private void InitWebPlayer()
	{
		string xml = Resources.Load<TextAsset>("web_cfg").text;
		_localRootXML.LoadXml(xml);
		_localDLLVersion = GetDLLVersion(_localRootXML);
		_localMD5Version = GetMD5Version(_localRootXML);
		Logger.Log("localVersion " + _localDLLVersion + "-" + _localMD5Version);
	}

	protected void HandleMessageReceived(string condiction, string stackTrack, LogType type)
	{
		var msg = string.Concat("[", type.ToString(), "]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff "), condiction);
		if (type == LogType.Error || type == LogType.Warning || type == LogType.Exception)
			msg += "\n[TRACK]" + stackTrack;
		_logWriter.WriteLog(msg);
	}

	private void LogHandler(int level, string message)
	{
		LogLevel lv = (LogLevel)level;
		var msg = string.Concat("[", lv.ToString(), "]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message);
		_logWriter.WriteLog(msg);
	}

	void CheckNetwork(Action callback)
	{
		GameStart.Invoke(callback);
		//
		//        if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) //wifi
		//        {
		//            GameStart.Invoke(callback);
		//            return;
		//        }
		//
		//        if (Application.internetReachability == NetworkReachability.NotReachable) //无网络
		//        {
		//            PluginMsgAction.Instance.ShowMsgBox("网络异常,未找到可用网络", "设置网络", "重试", () =>
		//            {
		//                PlatformSDKPlugin.instance.OpenNetworkSetting();
		//            }, () =>
		//            {
		//                PluginMsgAction.Instance.HideMsgBox();
		//                CheckNetwork(callback);
		//            });
		//            return;
		//        }
		//
		//        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork) //移动数据
		//        {
		//            if (allowDownloadBy3G)
		//            {
		//                GameStart.Invoke(callback);
		//            }
		//            else
		//            {
		//                PluginMsgAction.Instance.ShowMsgBox("检测到当前在使用移动数据，可能会产生一些流量消耗，是否继续使用？", "设置网络", "继续", () =>
		//                {
		//                    PlatformSDKPlugin.instance.OpenNetworkSetting();
		//                }, () =>
		//                {
		//                    allowDownloadBy3G = true;
		//                    PluginMsgAction.Instance.HideMsgBox();
		//                    GameStart.Invoke(callback);
		//                });
		//            }
		//            return;
		//        }
	}

	void TryHotUpdate()
	{
		SetupUI.instance.SetHint("版本检查中TryHotUpdate");

		switch (Plugins.Platform.editorPlatform)
		{
		case Plugins.EditorPlatform.WindowsWebPlayer:
			WebPlayerCheckServerVersion();
			return;
		}

		switch (Application.platform)
		{
		case RuntimePlatform.Android:
			Logger.Log("GetSDCardPath " + URLs.GetSDCardPath());
			StartHotUpdate();
			break;
		case RuntimePlatform.WindowsPlayer:
			//windows返回路径自带file://
			URLs.FILE_PROTOCOL = "";
			LoadStreamingDllAndEnterGame();
			break;
		case RuntimePlatform.IPhonePlayer:
			StartHotUpdate();
			break;
			#if WIN_HOTUPDATE
			case RuntimePlatform.WindowsEditor:
			StartHotUpdate();
			break;
			#endif
		default:
			#if WINDOW_PACK
			LoadStreamingDllAndEnterGame();
			#else
			EnterGame();
			#endif
			break;
		}
	}

	public static string GetServerCfgURL()
	{
		return _localRootXML.SelectSingleNode("/root/RootCfg").Attributes["url"].Value;
	}

	private void WebPlayerCheckServerVersion()
	{
		SetupUI.instance.SetHint("检查服务器版本");
		SetupUI.instance.SetProgress(0.3f);

		URLs.serverCfgURL = GetServerCfgURL();
		Logger.Log("CheckServerVersion Download " + URLs.serverCfgURL);

		StartCoroutine(LoadFileForm(DownloadMgr.Instance.GetRandomParasUrl(URLs.serverCfgURL), null, OnDownloadServerRootFinished));
	}

	private void OnDownloadServerRootFinished(WWW www, string errorInfo)
	{
		string text = www.text;
		_serverRootXML.LoadXml(text);
		URLs.versionRootPath = _serverRootXML.SelectSingleNode("/root/VersionPath").Attributes["url"].Value;
		Logger.Log("server root path is " + URLs.versionRootPath);

		Action<WWW, string> OnDownloadMd5Finished = (www1, s) =>
		{
			md5ZipBytes = www1.text;

			if (Application.platform == RuntimePlatform.WindowsEditor)
				EnterGame();
			else
				DownloadDllAndEnterGame();
		};

		string md5Path = _serverRootXML.SelectSingleNode("/root/Version").Attributes["md5url"].Value;
		StartCoroutine(LoadFileForm(DownloadMgr.Instance.GetRandomParasUrl(URLs.versionRootPath + md5Path), null, OnDownloadMd5Finished));
	}

	private void OnDownloadServerRootFailed()
	{
		Logger.Error("CheckServerVersion Download serverCfg Failed");
		Action errorCallback = () =>
		{
			ShowErrorInfo("检查服务器版本出错", WebPlayerCheckServerVersion);
		};
		GameStart.Invoke(errorCallback);
	}

	private void DownloadDllAndEnterGame()
	{
		_serverDLLVersion = GetDLLVersion(_serverRootXML);
		Action<WWW, string> downloadResult = (www, error) =>
		{
			if (!string.IsNullOrEmpty(error))
			{
				Logger.Error("Download Dll Error " + error);
				return;
			}
			//Logger.Assert(www != null, "download file bytes empty error:" + error);

			byte[] dllData = www.bytes;
			Action enter = () =>
			{
				Type t = GetTypeFromAssembly(dllData);
				EnterGame(t);
			};
			GameStart.Invoke(enter);
		};

		Action downloadDll = () =>
		{
			string fileName = _serverRootXML.SelectSingleNode("/root/Version").Attributes["url"].Value;
			StartCoroutine(LoadFileForm(DownloadMgr.Instance.GetRandomParasUrl(fileName), null, downloadResult));
		};

		GameStart.Invoke(downloadDll);
	}

	void Update()
	{
		for (int i = 0; i < _actions.Count; i++)
		{
			Action a = _actions[i];
			_actions.RemoveAt(i);
			i--;
			a.Invoke();
		}
	}

	private void EnterGame()
	{
		Type t = Type.GetType("GameMain, Assembly-CSharp");
		EnterGame(t);
	}

	void EnterGame(Type t)
	{
		gameObject.AddComponent(t);
		if (Plugins.Platform.editorPlatform != Plugins.EditorPlatform.WindowsWebPlayer)
		{
			//   Logger.LogHandler -= LogHandler;
			//   Application.logMessageReceived -= HandleMessageReceived;
			//   _logWriter.Release();
		}
	}

	void StartHotUpdate()
	{
		GetApkRootCfg(() =>
			{
				GameStart.Invoke(FirstExport);
			});
	}



	public static void SetDLLVersion(XmlDocument xml, int version)
	{
		xml.SelectSingleNode("/root/Version").Attributes["program"].Value = version.ToString();
	}

	public static int GetDLLVersion(XmlDocument xml)
	{
		string version = xml.SelectSingleNode("/root/Version").Attributes["program"].Value;
		return int.Parse(version);
	}

	public static void SetMD5Version(XmlDocument xml, int version)
	{
		xml.SelectSingleNode("/root/Version").Attributes["md5"].Value = version.ToString();
	}

	public static int GetMD5Version(XmlDocument xml)
	{
		string version = xml.SelectSingleNode("/root/Version").Attributes["md5"].Value;
		return int.Parse(version);
	}

	public static void SetAPKVersionCode(XmlDocument xml, int version)
	{
		XmlNode versionNode = xml.SelectSingleNode("/root/Version");
		var collection = versionNode.Attributes;
		var xmlNode = collection.GetNamedItem("apk");
		XmlAttribute attr = null;
		if (xmlNode == null)
		{
			attr = xml.CreateAttribute("apk");
			collection.Append(attr);
		}
		else
			attr = (XmlAttribute)xmlNode;
		attr.Value = version.ToString();
	}

	public static int GetAPKVersionCode(XmlDocument xml)
	{
		var xmlNode = xml.SelectSingleNode("/root/Version").Attributes.GetNamedItem("apk");
		if (xmlNode == null)
			return 0;
		XmlAttribute attribute = (XmlAttribute)xmlNode;
		string version = attribute.Value;
		return int.Parse(version);
	}

	public void LoadLocalRootCfg()
	{
		Logger.Log("Load LocalVersion " + LocalRootCfgPath);
		if (!File.Exists(LocalRootCfgPath))
		{
			Logger.Log("LoadLocalVersion file not find");
			return;
		}
		var ver = Utils.LoadFile(LocalRootCfgPath);
		_localRootXML.LoadXml(ver);
		_localDLLVersion = GetDLLVersion(_localRootXML);
		_localMD5Version = GetMD5Version(_localRootXML);
		Logger.Log("localVersion " + _localDLLVersion + "-" + _localMD5Version);
	}

	private void GetApkRootCfg(Action callback)
	{
		Logger.Log("GetApkRootCfg: " + APKRootCfgPath);

		StartCoroutine(LoadFileForm(APKRootCfgPath,
			(p) =>
			{     },
			(www, errorInfo) =>
			{
				if (www == null || !string.IsNullOrEmpty(errorInfo))
				{
					Logger.Error("get ApkRootCfg file empty error:" + errorInfo);
					PluginMsgAction.Instance.ShowMsgBox("ApkRootCfg :" + errorInfo, "重试", "退出", () =>
						{
							PluginMsgAction.Instance.HideMsgBox();
							Logger.Log("get ApkRootCfg file retry");
							GameStart.Invoke(StartHotUpdate);
						},
						() =>
						{
							Application.Quit();
						});
					return;
				}
				string content = Encoding.Default.GetString(www.bytes);
				_apkRootXML.LoadXml(content);
				GameStart.Invoke(callback);
			}));
	}

	IEnumerator LoadFileForm(string path, Action<float> loading, Action<WWW, string> loaded)
	{
		Logger.Log ("LoadFileForm:path = "+ path);
		WWW www = new WWW(path);
		string errorInfo = null;
		while (!www.isDone)
		{
			if (string.IsNullOrEmpty(www.error))
			{
				if (loading != null)
					loading(www.progress);
				yield return null;			// 每帧查询
			}
		}
		if (!string.IsNullOrEmpty(www.error))
		{
			errorInfo = www.error;
			Logger.Error(errorInfo);
		}
		Logger.Log ("LoadFileForm:loaded = "+ path+","+(loaded != null));
		if (loaded != null)
			loaded(www, errorInfo);
		www.Dispose();
		www = null;
	}
	private void ExportMD5(int version, Action<bool, string> exportEnd, Action<float> progressCallback)
	{
		Logger.Log("FirstExport.ExportMD5 ... " + LOCAL_MD5_ZIP);
		ExportFile(LOCAL_MD5_ZIP,
			(p) =>
			{
				progressCallback(p);
			},
			(bytes, errorInfo) =>
			{
				if (!string.IsNullOrEmpty(errorInfo))
				{
					exportEnd(false, "export file bytes empty error:" + errorInfo);
					return;
				}
				SetMD5Version(_localRootXML, version);
				SaveLocalRootXML();
				_localDLLVersion = version;
				exportEnd(true, null);
			});
	}

	private void ExportDLL(int version, Action<bool, string> exportEnd, Action<float> progressCallback)
	{
		Logger.Log("FirstExport.DLLFileName ... " + DLLFileName);
		#if UNITY_IPHONE
		SetDLLVersion(_localRootXML, version);
		SaveLocalRootXML();
		_localDLLVersion = version;
		exportEnd(true, null);
		#else
		ExportFile(DLLFileName,
			(p) =>
			{
				progressCallback(p);
			},
			(bytes, errorInfo) =>
			{
				if (!string.IsNullOrEmpty(errorInfo))
				{
					exportEnd(false, "export file bytes empty error:" + errorInfo);
					return;
				}
				SetDLLVersion(_localRootXML, version);
				SaveLocalRootXML();
				_localDLLVersion = version;
				exportEnd(true, null);
			});
		#endif
	}

	public static void ExportFile(string path, Action<float> loading, Action<byte[], string> loaded)
	{
		var lowerPath = path.ToLower();
		string apkPath = URLs.streamingAssetsPath + lowerPath;
		string savePath = URLs.persistentDataPath + lowerPath;

		Logger.Log("[ExportFile]" + apkPath + " " + savePath);
		instance.StartCoroutine(instance.LoadFileForm(apkPath, loading,
			(www, errorInfo) =>
			{
				if (!string.IsNullOrEmpty(errorInfo))
				{
					GameStart.Invoke(() =>
						{
							loaded(null, errorInfo);
						});
					return;
				}

				byte[] bytes = www.bytes;
				GameStart.Invoke(() =>
					{
						if (bytes != null && bytes.Length != 0)
						{
							errorInfo = SaveFile(bytes, savePath, false);
						}
						loaded(bytes, errorInfo);
					});
			}));
	}

	public static string SaveFile(byte[] bytes, string path, bool importantFile)
	{

		#if !UNITY_WEBPLAYER
		string dir = Path.GetDirectoryName(path);
		if (!Directory.Exists(dir))
			Directory.CreateDirectory(dir);
		string errorInfo = "";
		if (bytes == null || bytes.Length == 0)
			return "empty bytes";
		bool needRevert = false;
		byte[] orgBytes = null;
		try
		{
			if (importantFile && File.Exists(path))
			{
				orgBytes = File.ReadAllBytes(path);
			}
			File.WriteAllBytes(path, bytes);
			bytes = null;
		}
		catch (Exception e)
		{
			errorInfo = "SaveFile Failed " + path + " \n" + e.Message;
			Logger.Error(errorInfo);
			if (importantFile && orgBytes != null)
				needRevert = true;
		}

		try
		{
			if (needRevert)
				File.WriteAllBytes(path, orgBytes);
		}
		catch (Exception e)
		{
			string error = "RevertFile Failed " + path + " \n" + e.Message;
			errorInfo += "\n" + error;
			Logger.Error(error);
		}

		orgBytes = null;
		return errorInfo;
		#else
		return "";
		#endif
	}


	private void ExportRootCfg(XmlDocument apkRootCfgXML)
	{
		_localRootXML = apkRootCfgXML;
		string dir = Path.GetDirectoryName(LocalRootCfgPath);
		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}
		_localRootXML.Save(LocalRootCfgPath);
		_localDLLVersion = GetDLLVersion(_localRootXML);
		_localMD5Version = GetMD5Version(_localRootXML);
	}

	void ShowErrorInfo(string errorInfo, Action retryAction)
	{
		PluginMsgAction.Instance.ShowMsgBox(errorInfo, "重试", "退出", () =>
			{
				PluginMsgAction.Instance.HideMsgBox();
				if (retryAction != null)
					retryAction.Invoke();
			},
			() =>
			{
				Application.Quit();
			});
	}

	void CheckExportEnd()
	{
		if (!dllExportResult)
			return;
		if (!md5ExportResult)
			return;
		CheckServerVersion();
	}

	public const string CODE_PACKAGE = "com.xsfh.union";
	string GetVersionName()
	{
		return PlatformSDKPlugin.instance.GetVersionName();
	}

	string GetVersionCode()
	{
		return PlatformSDKPlugin.instance.GetCodeVersion();
	}




	void FirstExport()
	{
		int dllVersion = GetDLLVersion(_apkRootXML);
		int md5Version = GetMD5Version(_apkRootXML);
		Logger.Log("[FirstExport] version " + dllVersion + "-" +  md5Version);
		SetupUI.instance.SetHint("导出资源...");
		Action<bool, string> dllCallback = (exportResult, error) =>
		{
			Logger.Log("[FirstExport] dllCallback success " + exportResult + "-" + error);
			dllExportResult = exportResult;
			if (!dllExportResult)
			{
				ShowErrorInfo("导出DLL错误 " + error, FirstExport);
				return;
			}
			CheckExportEnd();
		};

		Action<bool, string> md5Callback = (exportResult, error) =>
		{
			Logger.Log("[FirstExport] md5Callback success " + exportResult + "-" + error);
			md5ExportResult = exportResult;
			if (!md5ExportResult)
			{
				ShowErrorInfo("导出MD5错误 " + error, FirstExport);
				return;
			}
			CheckExportEnd();
		};

		Action<float> dllProgressCallback = (progress) =>
		{
			dllProgress = progress;
			SetupUI.instance.SetProgress((dllProgress + md5Progress) / 2);
		};

		Action<float> md5ProgressCallback = (progress) =>
		{
			md5Progress = progress;
			SetupUI.instance.SetProgress((dllProgress + md5Progress) / 2);
		};
		string apkVersionCodeStr = GetVersionCode();
		if (string.IsNullOrEmpty(apkVersionCodeStr))
		{
			ShowErrorInfo("GetVersionCode错误 ", FirstExport);
			return;
		}
		int apkVersionCode = int.Parse(apkVersionCodeStr);

		if (!File.Exists(LocalRootCfgPath))
		{
			Logger.Log("FirstExport");
			DeleteHotUpdateRes();
			ExportRootCfg(_apkRootXML);
			ExportDLL(dllVersion, dllCallback, dllProgressCallback);
			ExportMD5(md5Version, md5Callback, md5ProgressCallback);
			SetAPKVersionCode(_localRootXML, apkVersionCode);
			SaveLocalRootXML();
			return;
		}
		LoadLocalRootCfg();
		ApkVersionName = GetVersionName();

		int currentVersionCode = GetAPKVersionCode(_localRootXML);
		if (apkVersionCode > currentVersionCode) //检查覆盖安装后的版本
		{
			Logger.Log("CoverExport apkVersionCode:" + apkVersionCode + " vs currentVersionCode:" + currentVersionCode);
			DeleteHotUpdateRes();
			ExportRootCfg(_apkRootXML);
			ExportDLL(dllVersion, dllCallback, dllProgressCallback);
			ExportMD5(md5Version, md5Callback, md5ProgressCallback);
			SetAPKVersionCode(_localRootXML, apkVersionCode);
			SaveLocalRootXML();
			return;
		}

		bool existsDllFile = false;
		#if !UNITY_IPHONE || UNITY_EDITOR
		existsDllFile = File.Exists(DLLLoaclPath);
		#else
		existsDllFile = true;
		#endif
		if (existsDllFile && dllVersion <= _localDLLVersion)
		{
			dllProgress = 1;
			SetupUI.instance.SetProgress((dllProgress + md5Progress) / 2);
			dllCallback.Invoke(true, null);
		}
		else
			ExportDLL(dllVersion, dllCallback, dllProgressCallback);

		if (File.Exists(LocalMD5ZipFilePath) && md5Version <= _localMD5Version)
		{
			md5Progress = 1;
			SetupUI.instance.SetProgress((dllProgress + md5Progress) / 2);
			md5Callback.Invoke(true, null);
		}
		else //
		{
			DeleteHotUpdateRes();
			ExportMD5(md5Version, md5Callback, md5ProgressCallback);
		}
	}

	void DeleteHotUpdateRes()
	{
		Caching.ClearCache();
		DeleteDirectory("apkmd5");
		DeleteDirectory("newestmd5");
		DeleteDirectory("settings");
		DeleteDirectory("sfx");
		DeleteDirectory("font");
		DeleteDirectory("raw");
		DeleteDirectory("roles");
		DeleteDirectory("scenes");
		DeleteDirectory("sound");
		DeleteDirectory("ui");
		DeleteFile("bezier.u");
		DeleteFile("manifest.u");
		DeleteFile("shader.u");
		DeleteFile("setting.pkg");
	}

	void DeleteDirectory(string url)
	{
		#if !UNITY_WEBPLAYER
		url = URLs.persistentDataPath + url;
		if (!Directory.Exists(url))
			return;
		Directory.Delete(url, true);
		Logger.Warning("Delete " + url);
		#endif
	}

	void DeleteFile(string url)
	{
		url = URLs.persistentDataPath + url;
		if (!File.Exists(url))
			return;
		File.Delete(url);
		Logger.Warning("Delete " + url);
	}


	public static void SaveLocalRootXML()
	{
		_localRootXML.Save(LocalRootCfgPath);
	}

	void SaveDLL(byte[] data, int version, Action<bool, string> callback)
	{
		bool saveResult = false;
		string errorInfo = null;
		errorInfo = SaveFile(data, DLLLoaclPath, true);
		if (string.IsNullOrEmpty(errorInfo))
		{
			SetDLLVersion(_localRootXML, version);
			SaveLocalRootXML();
			_localDLLVersion = version;
			saveResult = true;
		}
		callback(saveResult, errorInfo);
		Logger.Log("SaveDLL End");
	}

	void LoadLocalDLL()
	{
		Logger.Log("LoadLocalDLL " + DLLLoaclPath);
		Type t;
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			t = GetTypeFromAssembly();
			Logger.Log("WindowsEditor Temp ");
			EnterGame(t);
			return;
		}
		SetupUI.instance.SetHint("载入资源中");
		StartCoroutine(LoadFileForm(URLs.FILE_PROTOCOL + DLLLoaclPath,
			(p) =>
			{
			},
			(www, errorInfo) =>
			{
				if (!string.IsNullOrEmpty(errorInfo))
				{
					Logger.Error("LoadLocalDLL Error " + errorInfo);
					return;
				}
				//Logger.Assert(www != null, "download file bytes empty error:" + errorInfo);
				byte[] dllData = www.assetBundle.LoadAllAssets<TextAsset>()[0].bytes;
				www.assetBundle.Unload(false);
				Action enter = () =>
				{
					t = GetTypeFromAssembly(dllData);
					EnterGame(t);
				};
				GameStart.Invoke(enter);
			}));
	}

	void LoadStreamingDllAndEnterGame()
	{
		SetupUI.instance.SetHint("LoadStreamingDllAndEnterGame");
		var streamingDllPath = URLs.streamingAssetsPath + DLLFileName;
		Logger.Error ("LoadStreamingDllAndEnterGame:"+URLs.FILE_PROTOCOL + streamingDllPath);
		StartCoroutine(LoadFileForm(URLs.FILE_PROTOCOL + streamingDllPath, (p) =>{
			SetupUI.instance.SetHint("LoadStreamingDllAndEnterGame:"+p);
		},
			(www, errorInfo) =>{
				Logger.Log("LoadStreamingDllAndEnterGame final");
				if (!string.IsNullOrEmpty(errorInfo))
				{
					Logger.Error("LoadStreamingDll Error " + errorInfo);
					return;
				}
				SetupUI.instance.SetHint("UnLoadDLL:");
				//Logger.Assert(www != null, "download file bytes empty error:" + errorInfo);
				byte[] dllData = www.assetBundle.LoadAllAssets<TextAsset>()[0].bytes;
				www.assetBundle.Unload(false);
				Action enter = () =>
				{
					SetupUI.instance.SetHint("GetTypeFromAssembly:");
					Type t = GetTypeFromAssembly(dllData);
					EnterGame(t);
				};
				GameStart.Invoke(enter);
			}));
	}

	Type GetTypeFromAssembly()
	{
		Type monoType = typeof(MonoBehaviour);
		Type gameMainType = null;
		Assembly[] assemblyArr = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblyArr)
		{
			Type t = assembly.GetType("GameMain");
			if (t != null && monoType.IsAssignableFrom(t))
			{
				gameMainType = t;
				break;
			}
		}
		if (gameMainType == null)
		{
			Logger.Log("Can not find gameMainType");
		}
		return gameMainType;
	}

	private string networkUrl = string.Empty;
	void LogIpAddressAndMobileIP()
	{
		if (string.IsNullOrEmpty(networkUrl))
			return;

		string wwwUrl = "";
		try
		{
			wwwUrl = networkUrl.Replace("http://", "");
			wwwUrl = wwwUrl.Substring(0, wwwUrl.IndexOf("/"));

			Logger.Info("networkIpv4 " + NetInfo.GetIPAddress(wwwUrl) + " playerIp: " + MobileInfo.GetMobileCurrentIPv4());
		}
		catch (Exception e)
		{
			Logger.Error("解析url ip地址失败或获取玩家ip信息失败！ wwwUrl为：" + wwwUrl + " 错误信息：" + e.Message);
		}
	}

	void DownloadFileFromServer(int version, string fileName, string localPath, Action<string> callback)
	{
		//string md5FileName = _serverRootXML.SelectSingleNode("/root/Version").Attributes["md5url"].Value;
		networkUrl = URLs.GetVersionPath(version, fileName);
		var t = new Thread(LogIpAddressAndMobileIP);
		t.Start();
		Logger.Log("[DownloadFileFromServer] " + networkUrl);

		SetupUI.instance.SetHint("更新" + fileName + "版本至 " + version);
		StartCoroutine(LoadFileForm(networkUrl,
			(p) =>
			{
				SetupUI.instance.SetProgress(p);
				Logger.Log("program updating ..." + p);
			},
			(www, errorInfo) =>
			{
				if (www == null || !string.IsNullOrEmpty(errorInfo))
				{
					Logger.Error("download file bytes empty error:" + errorInfo);

					PluginMsgAction.Instance.ShowMsgBox("download " + networkUrl + " error:" + errorInfo, "确定", "取消", () =>
						{
							PluginMsgAction.Instance.HideMsgBox();
							Application.Quit();
						},
						() =>
						{
							Application.Quit();
						});

					return;
				}
				errorInfo = SaveFile(www.bytes, localPath, true);
				callback.Invoke(errorInfo);
			}));

	}

	void CheckServerVersion()
	{
		SetupUI.instance.SetHint("检查服务器版本");
		SetupUI.instance.SetProgress(0.3f);
		string serverCfgURL = GetServerCfgURL();

		Logger.Log("CheckServerVersion Download " + serverCfgURL);
		DownloadMgr.Instance.AsynDownLoadText(serverCfgURL, (text) =>
			{
				_serverRootXML.LoadXml(text);
				URLs.versionRootPath = _serverRootXML.SelectSingleNode("/root/VersionPath").Attributes["url"].Value;

				bool needReInstall = IsNeedReInstall();
				if (needReInstall)
				{
					Action errorCallback = () =>
					{
						Logger.Log("IsNeedReInstall ");
						PluginMsgAction.Instance.ShowMsgBox("发现新版本, 请下载新版本后覆盖安装", "确定", "退出", () =>
							{
								Application.Quit();
							}, () =>
							{
								Application.Quit();
							});
					};
					GameStart.Invoke(errorCallback);
					return;
				}
				GetNewestMD5Version();
			},
			() =>
			{
				Logger.Error("CheckServerVersion Download serverCfg Failed");
				Action errorCallback = ()=>
				{
					ShowErrorInfo("检查服务器版本出错", CheckServerVersion);
				};
				GameStart.Invoke(errorCallback);
			}
		);
	}


	bool IsNeedReInstall()
	{
		if (string.IsNullOrEmpty(ApkVersionName))
			return false;

		string[] currentVersions = ApkVersionName.Split('.');
		var versionNode = _serverRootXML.SelectSingleNode("root/Version");
		var node = versionNode.Attributes.GetNamedItem("miniapk");
		if (node == null)
			return false;
		Logger.Log("APKVersion " + ApkVersionName + ":" + node.Value);
		string[] miniVersions = node.Value.Split('.');
		for (int i = 0; i < miniVersions.Length; i++)
		{
			string miniV = miniVersions[i];
			if (i >= currentVersions.Length)
				continue;

			string curV = currentVersions[i];
			int n_curV = int.Parse(curV);
			int n_miniV = int.Parse(miniV);
			if (n_miniV > n_curV)  //服务器允许最小版本
			{
				return true;
			}
			if (n_miniV == n_curV)
				continue;
			return false;
		}
		return false;
	}


	void CheckDllVersion()
	{
		//iphone 依然有dll版本，dll版本为基包值。
		_serverDLLVersion = GetDLLVersion(_serverRootXML);
		Logger.Log("[CheckDllVersion] version local:" + _localDLLVersion + " vs server:" + _serverDLLVersion);
		if (_serverDLLVersion <= _localDLLVersion)
		{
			GameStart.Invoke(LoadLocalDLL);
			return;
		}

		Action<string> downloadResult = (error) =>
		{
			if (string.IsNullOrEmpty(error))
			{
				SetDLLVersion(_localRootXML, _serverDLLVersion);
				SaveLocalRootXML();
				_localDLLVersion = _serverDLLVersion;
				GameStart.Invoke(LoadLocalDLL);
			}
		};

		Action downloadDll = () =>
		{
			var t = new Thread(LogIpAddressAndMobileIP);
			t.Start();
			string fileName = _serverRootXML.SelectSingleNode("/root/Version").Attributes["dllurl"].Value;
			DownloadFileFromServer(_serverDLLVersion, fileName, DLLLoaclPath, downloadResult);
		};

		GameStart.Invoke(downloadDll);
	}

	void GetNewestMD5Version()
	{
		_serverMD5Version = GetMD5Version(_serverRootXML);
		Logger.Log("[CheckMD5Version] version local:" + _localMD5Version + " vs server:" + _serverMD5Version);
		if (_serverMD5Version == _localMD5Version && File.Exists(ServerMD5ZipFilePath))
		{
			GameStart.Invoke(CheckDllVersion);
			return;
		}

		Action<string> downloadResult = (error) =>
		{
			if (string.IsNullOrEmpty(error))
			{
				if (File.Exists(ServerMD5FilePath))
					File.Delete(ServerMD5FilePath);
				SetMD5Version(_localRootXML, _serverMD5Version);
				SaveLocalRootXML();
				_localMD5Version = _serverMD5Version;
				GameStart.Invoke(CheckDllVersion);
			}
		};

		Action downloadDll = () =>
		{
			string fileName = _serverRootXML.SelectSingleNode("/root/Version").Attributes["md5url"].Value;
			DownloadFileFromServer(_serverMD5Version, fileName, ServerMD5ZipFilePath, downloadResult);
		};

		GameStart.Invoke(downloadDll);
	}

	Type GetTypeFromAssembly(byte[] deData)
	{
		SetupUI.instance.SetHint("GetTypeFromAssembly1:");
		Assembly ass = Assembly.Load(deData);

		Type t = ass.GetType("GameMain");
		if (t == null)
		{
			SetupUI.instance.SetHint("GetTypeFromAssembly1:NULL");
			Type[] ts = ass.GetTypes();
			for (int i = 0; i < ts.Length; i++ )
			{
				if (ts [i].Name.Contains( "AIControl"))
				{
					Logger.Log(i + "XXX: " + ts[i].Name);
				} else

					Logger.Error(i + " " + ts[i].Name);
			}
			Logger.Error("---------------Reflection GameMain Failed------------------------ " + ts.Length);
		}
		return t;
	}

	public static void Invoke(Action action)
	{
		if (action == null)
			return;
		_actions.Add(action);
	}
}

