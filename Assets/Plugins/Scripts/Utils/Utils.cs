﻿#region 模块信息
/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：Utils
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.2.1
// 模块描述：通用工具类。
//----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using UnityEngine;
#if !UNITY_WEBPLAYER
using ICSharpCode.SharpZipLib.Zip;
#endif
using System.Security.Cryptography;
using System.Net;
using System.Runtime.InteropServices;

/// <summary>
/// 通用工具类。
/// </summary>
public static class Utils
{
	#region 常量

	/// <summary>
	/// 键值分隔符： ‘:’
	/// </summary>
	private const Char KEY_VALUE_SPRITER = ':';
	/// <summary>
	/// 字典项分隔符： ‘,’
	/// </summary>
	private const Char MAP_SPRITER = ',';
	/// <summary>
	/// 数组分隔符： ','
	/// </summary>
	private const Char LIST_SPRITER = ',';

	public const String RPC_HEAD = "RPC_";

	public const String SVR_RPC_HEAD = "SVR_RPC_";

	#endregion
	#region 时间格式化

	/// <summary>
	/// 格式化日期格式。（yyyy-MM-dd HH:mm:ss）
	/// </summary>
	/// <param name="datetime">日期对象</param>
	/// <returns>日期字符串</returns>
	public static String FormatTime(this DateTime datetime)
	{
		return datetime.ToString("yyyy-MM-dd HH:mm:ss");
	}

	/// <summary>
	/// 格式化日期格式。（yyyy-MM-dd HH:mm:ss）
	/// </summary>
	/// <param name="datetime">日期值</param>
	/// <returns>日期字符串</returns>
	public static String FormatTime(this long datetime)
	{
		DateTime data = DateTime.FromBinary(datetime);
        return data.ToString("yyyy-MM-dd HH:mm:ss");
	}
    public static long Get1970ToNowSeconds()
    {
        return (System.DateTime.UtcNow.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
    }
	/// <summary>
	/// 时间戳转为C#格式时间。
	/// </summary>
	/// <param name="timeStamp">时间戳</param>
	/// <returns></returns>
	public static DateTime GetTime(this int timeStamp)
	{
		DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
		return startTime.AddSeconds(timeStamp);
	}

	#endregion

	#region 字典转换

	/// <summary>
	/// 将字典字符串转换为键类型与值类型都为整型的字典对象。
	/// </summary>
	/// <param name="strMap">字典字符串</param>
	/// <param name="keyValueSpriter">键值分隔符</param>
	/// <param name="mapSpriter">字典项分隔符</param>
	/// <returns>字典对象</returns>
	public static Dictionary<Int32, Int32> ParseMapIntInt(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
	{
		Dictionary<Int32, Int32> result = new Dictionary<Int32, Int32>();
		var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
		foreach (var item in strResult)
		{
			int key;
			int value;
			if (int.TryParse(item.Key, out key) && int.TryParse(item.Value, out value))
				result.Add(key, value);
			else
				Logger.Warning(String.Format("Parse failure: {0}, {1}", item.Key, item.Value));
		}
		return result;
	}

	/// <summary>
	/// 将字典字符串转换为键类型为整型，值类型为单精度浮点数的字典对象。
	/// </summary>
	/// <param name="strMap">字典字符串</param>
	/// <param name="keyValueSpriter">键值分隔符</param>
	/// <param name="mapSpriter">字典项分隔符</param>
	/// <returns>字典对象</returns>
	public static Dictionary<Int32, float> ParseMapIntFloat(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
	{
		var result = new Dictionary<Int32, float>();
		var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
		foreach (var item in strResult)
		{
			int key;
			float value;
			if (int.TryParse(item.Key, out key) && float.TryParse(item.Value, out value))
				result.Add(key, value);
			else
				Logger.Warning(String.Format("Parse failure: {0}, {1}", item.Key, item.Value));
		}
		return result;
	}

	/// <summary>
	/// 将字典字符串转换为键类型为整型，值类型为字符串的字典对象。
	/// </summary>
	/// <param name="strMap">字典字符串</param>
	/// <param name="keyValueSpriter">键值分隔符</param>
	/// <param name="mapSpriter">字典项分隔符</param>
	/// <returns>字典对象</returns>
	public static Dictionary<Int32, String> ParseMapIntString(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
	{
		Dictionary<Int32, String> result = new Dictionary<Int32, String>();
		var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
		foreach (var item in strResult)
		{
			int key;
			if (int.TryParse(item.Key, out key))
				result.Add(key, item.Value);
			else
				Logger.Warning(String.Format("Parse failure: {0}", item.Key));
		}
		return result;
	}

	/// <summary>
	/// 将字典字符串转换为键类型为字符串，值类型为单精度浮点数的字典对象。
	/// </summary>
	/// <param name="strMap">字典字符串</param>
	/// <param name="keyValueSpriter">键值分隔符</param>
	/// <param name="mapSpriter">字典项分隔符</param>
	/// <returns>字典对象</returns>
	public static Dictionary<String, float> ParseMapStringFloat(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
	{
		Dictionary<String, float> result = new Dictionary<String, float>();
		var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
		foreach (var item in strResult)
		{
			float value;
			if (float.TryParse(item.Value, out value))
				result.Add(item.Key, value);
			else
				Logger.Warning(String.Format("Parse failure: {0}", item.Value));
		}
		return result;
	}

	/// <summary>
	/// 将字典字符串转换为键类型为字符串，值类型为整型的字典对象。
	/// </summary>
	/// <param name="strMap">字典字符串</param>
	/// <param name="keyValueSpriter">键值分隔符</param>
	/// <param name="mapSpriter">字典项分隔符</param>
	/// <returns>字典对象</returns>
	public static Dictionary<String, Int32> ParseMapStringInt(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
	{
		Dictionary<String, Int32> result = new Dictionary<String, Int32>();
		var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
		foreach (var item in strResult)
		{
			int value;
			if (int.TryParse(item.Value, out value))
				result.Add(item.Key, value);
			else
				Logger.Warning(String.Format("Parse failure: {0}", item.Value));
		}
		return result;
	}

	/// <summary>
	/// 将字典字符串转换为键类型为 T，值类型为 U 的字典对象。
	/// </summary>
	/// <typeparam name="T">字典Key类型</typeparam>
	/// <typeparam name="U">字典Value类型</typeparam>
	/// <param name="strMap">字典字符串</param>
	/// <param name="keyValueSpriter">键值分隔符</param>
	/// <param name="mapSpriter">字典项分隔符</param>
	/// <returns>字典对象</returns>
	public static Dictionary<T, U> ParseMapAny<T, U>(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
	{
		var typeT = typeof(T);
		var typeU = typeof(U);
		var result = new Dictionary<T, U>();
		//先转为字典
		var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
		foreach (var item in strResult)
		{
			try
			{
				T key = (T)GetValue(item.Key, typeT);
				U value = (U)GetValue(item.Value, typeU);

				result.Add(key, value);
			}
			catch (Exception)
			{
				Logger.Warning(String.Format("Parse failure: {0}, {1}", item.Key, item.Value));
			}
		}
		return result;
	}

	/// <summary>
	/// 将字典字符串转换为键类型与值类型都为字符串的字典对象。
	/// </summary>
	/// <param name="strMap">字典字符串</param>
	/// <param name="keyValueSpriter">键值分隔符</param>
	/// <param name="mapSpriter">字典项分隔符</param>
	/// <returns>字典对象</returns>
	public static Dictionary<String, String> ParseMap(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
	{
		Dictionary<String, String> result = new Dictionary<String, String>();
		if (String.IsNullOrEmpty(strMap))
		{
			return result;
		}

		var map = strMap.Split(mapSpriter);//根据字典项分隔符分割字符串，获取键值对字符串
		for (int i = 0; i < map.Length; i++)
		{
			if (String.IsNullOrEmpty(map[i]))
			{
				continue;
			}

			var keyValuePair = map[i].Split(keyValueSpriter);//根据键值分隔符分割键值对字符串
			if (keyValuePair.Length == 2)
			{
				if (!result.ContainsKey(keyValuePair[0]))
					result.Add(keyValuePair[0], keyValuePair[1]);
				else
					Logger.Warning(String.Format("Key {0} already exist, index {1} of {2}.", keyValuePair[0], i, strMap));
			}
			else
			{
				Logger.Warning(String.Format("KeyValuePair are not match: {0}, index {1} of {2}.", map[i], i, strMap));
			}
		}
		return result;
	}

	/// <summary>
	/// 将字典对象转换为字典字符串。
	/// </summary>
	/// <typeparam name="T">字典Key类型</typeparam>
	/// <typeparam name="U">字典Value类型</typeparam>
	/// <param name="map">字典对象</param>
	/// <returns>字典字符串</returns>
	public static String PackMap<T, U>(this IEnumerable<KeyValuePair<T, U>> map, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
	{
		if (map.Count() == 0)
			return "";
		else
		{
			StringBuilder sb = new StringBuilder();
			foreach (var item in map)
			{
				sb.AppendFormat("{0}{1}{2}{3}", item.Key, keyValueSpriter, item.Value, mapSpriter);
			}
			return sb.ToString().Remove(sb.Length - 1, 1);
		}
	}

	#endregion

	#region 列表转换

	/// <summary>
	/// 将列表字符串转换为类型为 T 的列表对象。
	/// </summary>
	/// <typeparam name="T">列表值对象类型</typeparam>
	/// <param name="strList">列表字符串</param>
	/// <param name="listSpriter">数组分隔符</param>
	/// <returns>列表对象</returns>
	public static List<T> ParseListAny<T>(this String strList, Char listSpriter = LIST_SPRITER)
	{
		var type = typeof(T);
		var list = strList.ParseList(listSpriter);
		var result = new List<T>();
		foreach (var item in list)
		{
			result.Add((T)GetValue(item, type));
		}
		return result;
	}

	/// <summary>
	/// 将列表字符串转换为字符串的列表对象。
	/// </summary>
	/// <param name="strList">列表字符串</param>
	/// <param name="listSpriter">数组分隔符</param>
	/// <returns>列表对象</returns>
	public static List<String> ParseList(this String strList, Char listSpriter = LIST_SPRITER)
	{
		var result = new List<String>();
		if (String.IsNullOrEmpty(strList))
			return result;

		var trimString = strList.Trim();
		if (String.IsNullOrEmpty(strList))
		{
			return result;
		}
		var detials = trimString.Split(listSpriter);//.Substring(1, trimString.Length - 2)
		foreach (var item in detials)
		{
			if (!String.IsNullOrEmpty(item))
				result.Add(item.Trim());
		}

		return result;
	}

	/// <summary>
	/// 将列表对象转换为列表字符串。
	/// </summary>
	/// <typeparam name="T">列表值对象类型</typeparam>
	/// <param name="list">列表对象</param>
	/// <param name="listSpriter">列表分隔符</param>
	/// <returns>列表字符串</returns>
	public static String PackList<T>(this List<T> list, Char listSpriter = LIST_SPRITER)
	{
		if (list.Count == 0)
			return "";
		else
		{
			StringBuilder sb = new StringBuilder();
			//sb.Append("[");
			foreach (var item in list)
			{
				sb.AppendFormat("{0}{1}", item, listSpriter);
			}
			sb.Remove(sb.Length - 1, 1);
			//sb.Append("]");

			return sb.ToString();
		}

	}

	public static String PackArray<T>(this T[] array, Char listSpriter = LIST_SPRITER)
	{
		var list = new List<T>();
		list.AddRange(array);
		return PackList(list, listSpriter);
	}

	#endregion

	#region 随机数

	/// <summary>
	/// 创建一个产生不重复随机数的随机数生成器。
	/// </summary>
	/// <returns>随机数生成器</returns>
	public static System.Random CreateRandom()
	{
		long tick = DateTime.Now.Ticks;
		return new System.Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
	}

	public static T Choice<T>(List<T> list)
	{
		if (list.Count == 0)
		{
			return default(T);
		}

		int index = UnityEngine.Random.Range(0, list.Count);
		return list[index];
	}

	#endregion

	#region 文件读取

	public static String LoadResource(String fileName)
	{
		var text = Resources.Load(fileName);
		if (text != null)
		{
			var result = text.ToString();
			Resources.UnloadAsset(text);
			return result;
		}
		else
			return String.Empty;
	}

	public static String LoadFile(String fileName)
	{
		//Logger.Log(fileName);
		if (File.Exists(fileName))
			using (StreamReader sr = File.OpenText(fileName))
				return sr.ReadToEnd();
		else
			return String.Empty;
	}

	public static byte[] LoadByteResource(String fileName)
	{
		TextAsset binAsset = Resources.Load(fileName, typeof(TextAsset)) as TextAsset;
		var result = binAsset.bytes;
		Resources.UnloadAsset(binAsset);

		return result;
	}

#if !UNITY_WEBPLAYER
    public static byte[] LoadByteFile(String fileName)
	{
		if (File.Exists(fileName))
			return File.ReadAllBytes(fileName);
		else
			return null;
	}
#endif


    #endregion

    #region 类型转换

	/// <summary>
	/// 将字符串转换为对应类型的值。
	/// </summary>
	/// <param name="value">字符串值内容</param>
	/// <param name="type">值的类型</param>
	/// <returns>对应类型的值</returns>
	public static object GetValue(String value, Type type)
	{
		if (type == null)
			return null;
		else if (type == typeof(string))
			return value;
		else if (type == typeof(Int32))
			return Convert.ToInt32(Convert.ToDouble(value));
		else if (type == typeof(float))
			return float.Parse(value);
		else if (type == typeof(byte))
			return Convert.ToByte(Convert.ToDouble(value));
		else if (type == typeof(sbyte))
			return Convert.ToSByte(Convert.ToDouble(value));
		else if (type == typeof(UInt32))
			return Convert.ToUInt32(Convert.ToDouble(value));
		else if (type == typeof(Int16))
			return Convert.ToInt16(Convert.ToDouble(value));
		else if (type == typeof(Int64))
			return Convert.ToInt64(Convert.ToDouble(value));
		else if (type == typeof(UInt16))
			return Convert.ToUInt16(Convert.ToDouble(value));
		else if (type == typeof(UInt64))
			return Convert.ToUInt64(Convert.ToDouble(value));
		else if (type == typeof(double))
			return double.Parse(value);
		else if (type == typeof(bool))
		{
			if (value == "0")
				return false;
			else if (value == "1")
				return true;
			else
				return bool.Parse(value);
		}
		else if (type.BaseType == typeof(Enum))
			return GetValue(value, Enum.GetUnderlyingType(type));
		else if (type == typeof(Vector3))
		{
			Vector3 result;
			ParseVector3(value, out result);
			return result;
		}
		else if (type == typeof(Quaternion))
		{
			Quaternion result;
			ParseQuaternion(value, out result);
			return result;
		}
		else if (type == typeof(Color))
		{
			Color result;
			ParseColor(value, out result);
			return result;
		}
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
		{
			Type[] types = type.GetGenericArguments();
			var map = ParseMap(value);
			var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
			foreach (var item in map)
			{
				var key = GetValue(item.Key, types[0]);
				var v = GetValue(item.Value, types[1]);
				type.GetMethod("Add").Invoke(result, new object[] { key, v });
			}
			return result;
		}
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			Type t = type.GetGenericArguments()[0];
			var list = ParseList(value);
			var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
			foreach (var item in list)
			{
				var v = GetValue(item, t);
				type.GetMethod("Add").Invoke(result, new object[] { v });
			}
			return result;
		}
		else
			return null;
	}

	/// <summary>
	/// 将指定格式(255, 255, 255, 255) 转换为 Color 
	/// </summary>
	/// <param name="_inputString"></param>
	/// <param name="result"></param>
	/// <returns>返回 true/false 表示是否成功</returns>
	public static bool ParseColor(string _inputString, out Color result)
	{
		string trimString = _inputString.Trim();
		result = Color.clear;
		if (trimString.Length < 9)
		{
			return false;
		}
		//if (trimString[0] != '(' || trimString[trimString.Length - 1] != ')')
		//{
		//    return false;
		//}
		try
		{
			string[] _detail = trimString.Split(LIST_SPRITER);//.Substring(1, trimString.Length - 2)
			if (_detail.Length != 4)
			{
				return false;
			}
			result = new Color(float.Parse(_detail[0]) / 255, float.Parse(_detail[1]) / 255, float.Parse(_detail[2]) / 255, float.Parse(_detail[3]) / 255);
			return true;
		}
		catch (Exception e)
		{
			Logger.Error("Parse Color error: " + trimString + e.ToString());
			return false;
		}
	}

	/// <summary>
	/// 将指定格式(1.0, 2, 3.4) 转换为 Vector3 
	/// </summary>
	/// <param name="_inputString"></param>
	/// <param name="result"></param>
	/// <returns>返回 true/false 表示是否成功</returns>
	public static bool ParseVector3(string _inputString, out Vector3 result)
	{
		string trimString = _inputString.Trim();
		result = new Vector3();
		if (trimString.Length < 7)
		{
			return false;
		}
		//if (trimString[0] != '(' || trimString[trimString.Length - 1] != ')')
		//{
		//    return false;
		//}
		try
		{
			string[] _detail = trimString.Split(LIST_SPRITER);//.Substring(1, trimString.Length - 2)
			if (_detail.Length != 3)
			{
				return false;
			}
			result.x = float.Parse(_detail[0]);
			result.y = float.Parse(_detail[1]);
			result.z = float.Parse(_detail[2]);
			return true;
		}
		catch (Exception e)
		{
			Logger.Error("Parse Vector3 error: " + trimString + e.ToString());
			return false;
		}
	}

	/// <summary>
	/// 将指定格式(1.0, 2, 3.4) 转换为 Vector3 
	/// </summary>
	/// <param name="_inputString"></param>
	/// <param name="result"></param>
	/// <returns>返回 true/false 表示是否成功</returns>
	public static bool ParseQuaternion(string _inputString, out Quaternion result)
	{
		string trimString = _inputString.Trim();
		result = new Quaternion();
		if (trimString.Length < 9)
		{
			return false;
		}
		//if (trimString[0] != '(' || trimString[trimString.Length - 1] != ')')
		//{
		//    return false;
		//}
		try
		{
			string[] _detail = trimString.Split(LIST_SPRITER);//.Substring(1, trimString.Length - 2)
			if (_detail.Length != 4)
			{
				return false;
			}
			result.x = float.Parse(_detail[0]);
			result.y = float.Parse(_detail[1]);
			result.z = float.Parse(_detail[2]);
			result.w = float.Parse(_detail[3]);
			return true;
		}
		catch (Exception e)
		{
			Logger.Error("Parse Quaternion error: " + trimString + e.ToString());
			return false;
		}
	}

	/// <summary>
	/// 替换字符串中的子字符串。
	/// </summary>
	/// <param name="input">原字符串</param>
	/// <param name="oldValue">旧子字符串</param>
	/// <param name="newValue">新子字符串</param>
	/// <param name="count">替换数量</param>
	/// <param name="startAt">从第几个字符开始</param>
	/// <returns>替换后的字符串</returns>
	public static String ReplaceFirst(this string input, string oldValue, string newValue, int startAt = 0)
	{
		int pos = input.IndexOf(oldValue, startAt);
		if (pos < 0)
		{
			return input;
		}
		return string.Concat(input.Substring(0, pos), newValue, input.Substring(pos + oldValue.Length));
	}

#endregion

    #region 文件路径处理

	public static string GetFileNameWithoutExtention(string fileName, char separator = '/')
	{
		var name = GetFileName(fileName, separator);
		return GetFilePathWithoutExtention(name);
	}

	public static string GetFilePathWithoutExtention(string fileName)
	{
		return fileName.Substring(0, fileName.LastIndexOf('.'));
	}

	public static string GetDirectoryName(string fileName)
	{
		return fileName.Substring(0, fileName.LastIndexOf('/'));
	}

	public static string GetFileName(string path, char separator = '/')
	{
		return path.Substring(path.LastIndexOf(separator) + 1);
	}

	public static string PathNormalize(this string str)
	{
		return str.Replace("\\", "/").ToLower();
	}

#endregion

	/// <summary>
	/// Create a zip archive.
	/// </summary>
	/// <param name="filename">The filename.</param>
	/// <param name="directory">The directory to zip.</param> 
	public static void PackFiles(string filename, string directory, string fileFilter)
	{
#if !UNITY_WEBPLAYER
        try
		{
			FastZip fz = new FastZip();
			fz.CreateEmptyDirectories = true;
			fz.CreateZip(filename, directory, false, fileFilter);
			fz = null;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message + "\n[Track]" + ex.StackTrace);
		}
#endif
	}

#if !UNITY_WEBPLAYER
    public static byte[] UnpackMemory(byte[] zipMemory)
	{
		MemoryStream stream = new MemoryStream(zipMemory);
		stream.Seek(0, SeekOrigin.Begin);
		ZipInputStream s = new ZipInputStream(stream);

		ZipEntry theEntry;
		List<byte> result = new List<byte>();
		while ((theEntry = s.GetNextEntry()) != null)
		{
			string fileName = Path.GetFileName(theEntry.Name);

			if (fileName != String.Empty)
			{
				int size = 2048;
				byte[] data = new byte[2048];
				while (true)
				{
					size = s.Read(data, 0, data.Length);
					if (size > 0)
					{
						var bytes = new Byte[size];
						Array.Copy(data, bytes, size);
						result.AddRange(bytes);
					}
					else
					{
						break;
					}
				}
			}
		}
		s.Close();
		stream.Close();
		return result.ToArray();
	}
#endif

#if !UNITY_WEBPLAYER
	/// <summary>
	/// Unpacks the files.
	/// </summary>
	/// <param name="file">The file.</param>
	/// <returns>if succeed return true,otherwise false.</returns>
	public static bool UnpackFiles(string file, string dir)
	{
		try
		{
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			ZipInputStream s = new ZipInputStream(File.OpenRead(file));

			ZipEntry theEntry;
			while ((theEntry = s.GetNextEntry()) != null)
			{

				string directoryName = Path.GetDirectoryName(theEntry.Name);
				string fileName = Path.GetFileName(theEntry.Name);

				if (directoryName != String.Empty)
					Directory.CreateDirectory(dir + directoryName);

				if (fileName != String.Empty)
				{
					FileStream streamWriter = File.Create(dir + theEntry.Name);

					int size = 2048;
					byte[] data = new byte[2048];
					while (true)
					{
						size = s.Read(data, 0, data.Length);
						if (size > 0)
						{
							streamWriter.Write(data, 0, size);
						}
						else
						{
							break;
						}
					}

					streamWriter.Close();
				}
			}
			s.Close();
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.Message + "\n[Track]" + ex.StackTrace);
			return false;
		}
	}
#endif

#region MD5

	public static Byte[] CreateMD5(Byte[] data)
	{

		using (var md5 = MD5.Create())
		{
			return md5.ComputeHash(data);
		}
	}

	public static string FormatMD5(Byte[] data)
	{
		return System.BitConverter.ToString(data).Replace("-", "").ToLower();
	}

    public static String BuildFileMd5(byte[] bytes)
    {
        String filemd5 = null;
        try
        {
            var md5 = MD5.Create();
            var fileMD5Bytes = md5.ComputeHash(bytes);//计算指定Stream 对象的哈希值                            
            //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”               
            filemd5 = FormatMD5(fileMD5Bytes);
        }
        catch (System.Exception ex)
        {
            Logger.Error(ex.Message + "\n[Track]" + ex.StackTrace);
        }
        return filemd5;
    }

	public static String BuildFileMd5(String filename)
	{
		String filemd5 = null;
		try
		{
			using (var fileStream = File.OpenRead(filename))
			{
				var md5 = MD5.Create();
				var fileMD5Bytes = md5.ComputeHash(fileStream);//计算指定Stream 对象的哈希值                            
				//由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”               
				filemd5 = FormatMD5(fileMD5Bytes);
			}
		}
		catch (System.Exception ex)
		{
			Logger.Error(ex.Message + "\n[Track]" + ex.StackTrace);
		}
		return filemd5;
	}

	public static bool CheckMD5(string fullPathName, string md5)
	{
		if (!File.Exists(fullPathName))
		{
			return false;
		}
#if UNITY_IPHONE
		//ios下如果封装该方法在一个函数中，调用该函数来产生文件的MD5的时候，就会抛JIT异常。
		//如果直接把这个产生MD5的方法体放在直接执行，就可以正常执行，这个原因还不清楚。
		string md5Compute = null;
		using(System.IO.FileStream fileStream = System.IO.File.OpenRead(fullPathName))
		{
			System.Security.Cryptography.MD5 md5Tmp = System.Security.Cryptography.MD5.Create();
			byte[] fileMD5Bytes = md5Tmp.ComputeHash(fileStream);
			md5Compute = System.BitConverter.ToString(fileMD5Bytes).Replace("-", "").ToLower();
		}
#else
		var md5Compute = Utils.BuildFileMd5(fullPathName);
#endif
		bool isCheck = md5Compute.Trim() == md5.Trim();
		if (!isCheck)
		{
			Logger.Log("MD5检验失败！： " + md5Compute + " --- " + md5);
		}
		return isCheck;
	}

#endregion


#region 密钥管理

	//[DllImport("key", CallingConvention = CallingConvention.Cdecl)]
	//public static extern int GetIndexKey(int i);
	//[DllImport("key", CallingConvention = CallingConvention.Cdecl)]
	//public static extern int GetResKey(int i);

	public static byte[] GetResNumber()
	{
		return new byte[] { 231, 20, 185, 13, 20, 127, 81, 79 };
	}

	public static byte[] GetIndexNumber()
	{
		return new byte[] { 123, 52, 53, 9, 12, 6, 23, 26 };
	}
#endregion

	static public string GetFullName(Transform rootTransform, Transform currentTransform)
	{
		string fullName = String.Empty;

		while (currentTransform != rootTransform)
		{
			fullName = currentTransform.name + fullName;

			if (currentTransform.parent != rootTransform)
			{
				fullName = String.Concat('/', fullName);
			}

			currentTransform = currentTransform.parent;
		}

		return fullName;
	}    
}
