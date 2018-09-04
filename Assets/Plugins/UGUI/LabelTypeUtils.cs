using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public static class LabelTypeUtils
{
	public static Dictionary<string, System.Object> defaultTypeConfig = null;

	public static Dictionary<UILabel.LabelType, Dictionary<string, System.Object>> typeConfigMap = null;

	public static System.Object GetProperty(Dictionary<string, System.Object> data, string key)
	{
		System.Object val = null;
		data.TryGetValue(key, out val);
		val = val ?? defaultTypeConfig[key];

		return val;
	}

	public static void ApplyLabelType(UILabel label, UILabel.LabelType t)
	{
		Dictionary<string, System.Object> data = null;
		if(typeConfigMap != null && typeConfigMap.TryGetValue(t, out data))
		{
			//font
			label.color = (Color)GetProperty(data, "fontColor");
			label.fontStyle = (FontStyle)GetProperty(data, "fontStyle");
			//gradient
			label.useGradient = (bool)GetProperty(data, "useGradient");
			label.gradientTop = (Color)GetProperty(data, "gradientTop");
			label.gradientBottom = (Color)GetProperty(data, "gradientBottom");
			//effect
			label.effectType = (UILabel.EffectType)GetProperty(data, "effectType");
			label.effectColor = (Color)GetProperty(data, "effectColor");
			label.effectDistance = (Vector2)GetProperty(data, "effectDistance");
		}
	}

    public static Dictionary<string, System.Object> GetConfig(UILabel.LabelType t)
    {
        Dictionary<string, System.Object> data = null;
        typeConfigMap.TryGetValue(t, out data);
        return data;
    }
}

