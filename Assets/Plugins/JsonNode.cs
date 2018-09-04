/// <summary>
/// 鉴于MiniJson反序列化出来的产物是个无类型的咚咚，需要人肉转换类型，使用起来相当繁琐。今天我们有了JsonNode，妈妈再也不用担心我解析json了。
/// </summary>
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;

public class JsonNode
{
    public enum NodeType { NUL, VAL, STR, LST, DICT, Vector2, Vector3, Vector4}
	NodeType nodeType;
	object rawData;
	Dictionary<string,JsonNode> dictData;
	List<JsonNode> listData;
	public static readonly JsonNode null_node = new JsonNode (null);
	static readonly Dictionary<string,JsonNode> null_dict = new Dictionary<string, JsonNode>();
	static readonly List<JsonNode> null_list = new List<JsonNode>();
	
    static readonly Type strType = typeof(String);
    static readonly Type intType = typeof(int);
    static readonly Type floatType = typeof(float);
    static readonly Type doubleType = typeof(double);
    static readonly Type boolType = typeof(bool);
    static readonly Type vector2Type = typeof(Vector2);
    static readonly Type vector3Type = typeof(Vector3);
    static readonly Type vector4Type = typeof(Vector4);
    static readonly Type hashTableType = typeof(Hashtable);

	public JsonNode(object data)
	{
		this.rawData = data;

        if (rawData == null)
            nodeType = NodeType.NUL;
        else if (rawData is IList)
        {
            nodeType = NodeType.LST;
            listData = new List<JsonNode>();
            List<object> tmp = this.rawData as List<object>;
            foreach (object val in tmp)
                listData.Add(new JsonNode(val));
        }
        else if (rawData is IDictionary)
        {
            nodeType = NodeType.DICT;
            dictData = new Dictionary<string, JsonNode>();
            Dictionary<string, object> tmp = this.rawData as Dictionary<string, object>;
            foreach (string key in tmp.Keys)
                dictData[key] = new JsonNode(tmp[key]);
        }
        else if (rawData is String)
            nodeType = NodeType.STR;
        else if (rawData is Vector2)
            nodeType = NodeType.Vector2;
        else if (rawData is Vector3)
            nodeType = NodeType.Vector3;
        else if (rawData is Vector4)
            nodeType = NodeType.Vector4;
        else
			nodeType = NodeType.VAL;
	}

	public bool isNull()
	{
		return this == null_node;
	}

	public NodeType type
	{
		get
		{
			return nodeType;
		}
	}

	//取值转换
    public object toObject(){ return rawData; }
	public bool toBool(){ return toBool (false); }
	public bool toBool(bool def)
	{
		if( nodeType != NodeType.VAL )
			return def;
		return Convert.ToBoolean(this.rawData);
	}
	public int toInt(){ return toInt (0); }
	public int toInt(int def)
	{
		if( nodeType != NodeType.VAL )
			return def;
		return Convert.ToInt32 (this.rawData); 
	}
	public float toFloat() { return toFloat (0f); }
	public float toFloat(float def) 
	{
		if( nodeType != NodeType.VAL )
			return def;
		return Convert.ToSingle(this.rawData); 
	}
	public double toDouble() { return toDouble (.0); }
	public double toDouble(double def)
	{
		if( nodeType != NodeType.VAL )
			return def;
		return Convert.ToDouble(this.rawData); 
	}
	public string toString() { return toString (""); }
	public string toString(string def) 
	{
		if( nodeType != NodeType.STR )
			return def;
		return (string)this.rawData;
	}
	public Vector3 toVector3() { return toVector3 (Vector3.zero); }
	public Vector3 toVector3(Vector3 def)
	{
		if( nodeType != NodeType.Vector3 )
			return def;
		return (Vector3)this.rawData;
	}
	public Vector2 toVector2() { return toVector2 (Vector2.zero); }
	public Vector2 toVector2(Vector2 def)
	{
		if( nodeType != NodeType.Vector2 )
			return def;
        return (Vector2)this.rawData;
	}

    public Vector4 toVector4() { return toVector4(Vector4.zero); }
    public Vector4 toVector4(Vector4 def)
    {
        if (nodeType != NodeType.Vector4)
            return def;
        return (Vector4)this.rawData;
    }

	public Dictionary<string,JsonNode> toDict() 
	{ 
		if( nodeType != NodeType.DICT )
			return null_dict;
		return dictData; 
	}
	public List<JsonNode> toList()
	{
		if( nodeType != NodeType.LST )
			return null_list;
		return listData; 
	}

	//索引器
	public JsonNode get(string key){ return this [key]; }
	public JsonNode get(int index){ return this [index]; }
	public JsonNode this[string key]
	{
		get
		{
			if( nodeType != NodeType.DICT )
				return null_node;
			if( !dictData.ContainsKey(key) )
				return null_node;
			return dictData[key];
		}
		set
		{
			dictData[key] = value;
		}
	}
	public JsonNode this[int index]
	{
		get
		{
			if( nodeType != NodeType.LST )
				return null_node;
			if( index < 0 || index > listData.Count-1 )
				return null_node;
			return listData[index];
		}
		set
		{
			listData[index] = value;
		}
	}

	//带默认值取值器
	public int get(string key, int def) { return this [key].toInt (def); }
	public float get(string key, float def) { return this [key].toFloat (def); }
	public double get(string key, double def) { return this [key].toDouble (def); }
	public bool get(string key, bool def) { return this [key].toBool (def); }
	public string get(string key, string def) { return this [key].toString (def); }
	public Vector2 get(string key, Vector2 def) { return this [key].toVector2 (def); }
	public Vector3 get(string key, Vector3 def) { return this [key].toVector3 (def); }
    public Vector4 get(string key, Vector4 def) { return this[key].toVector4(def); }

	public int get(int index, int def) { return this [index].toInt (def); }
	public float get(int index, float def) { return this [index].toFloat (def); }
	public double get(int index, double def) { return this [index].toDouble (def); }
	public bool get(int index, bool def) { return this [index].toBool (def); }
	public string get(int index, string def) { return this [index].toString (def); }
	public Vector2 get(int index, Vector2 def) { return this [index].toVector2 (def); }
	public Vector3 get(int index, Vector3 def) { return this [index].toVector3 (def); }
    public Vector4 get(int index, Vector4 def) { return this[index].toVector4(def); }

	//隐式赋值转换
	public static implicit operator JsonNode(int val) { return new JsonNode (val); }
	public static implicit operator JsonNode(string val) { return new JsonNode (val); }
	public static implicit operator JsonNode(float val) { return new JsonNode (val); }
	public static implicit operator JsonNode(double val) { return new JsonNode (val); }
	public static implicit operator JsonNode(bool val) { return new JsonNode (val); }
	public static implicit operator JsonNode(Vector2 val) { return new JsonNode (val); }
    public static implicit operator JsonNode(Vector3 val) { return new JsonNode (val); }
    public static implicit operator JsonNode(Vector4 val) { return new JsonNode (val); }

	public static implicit operator int(JsonNode node) { return node.toInt(); }
	public static implicit operator string(JsonNode node) { return node.toString(); }
	public static implicit operator float(JsonNode node) { return node.toFloat(); }
	public static implicit operator double(JsonNode node) { return node.toDouble(); }
	public static implicit operator bool(JsonNode node) { return node.toBool(); }
	public static implicit operator Vector2(JsonNode node) { return node.toVector2(); }
	public static implicit operator Vector3(JsonNode node) { return node.toVector3(); }
    public static implicit operator Vector4(JsonNode node) { return node.toVector4(); }

    public static object thunkVal(Type t, JsonNode json)
    {
        if( t == intType )
            return json.toInt();
        else if( t == floatType )
            return json.toFloat();
        else if( t == doubleType )
            return json.toDouble();
        else if( t == boolType )
            return json.toBool();
        else if( t == strType )
            return json.toString();
        else if (t == vector2Type)
            return json.toVector2();
        else if (t == vector3Type)
            return json.toVector3();
        else if (t == vector4Type)
            return json.toVector4();
        return json.toObject ();
    }

    public static bool isValueType(Type t)
    {
        if (t == intType
        || t == floatType
        || t == doubleType
        || t == boolType
        || t == strType
        || t == vector2Type
        || t == vector3Type
        || t == vector4Type)
            return true;
        return false;
    }
}