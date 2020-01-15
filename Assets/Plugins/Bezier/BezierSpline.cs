using UnityEngine;
using System;
using System.Collections.Generic;

public class BezierSpline : MonoBehaviour 
{
	[SerializeField]
	private List<Vector3> points;

	[SerializeField]
	private List<BezierControlPointMode> modes;

	[SerializeField]
	private bool loop;

	protected BezierWayPoint _waypoint;
	public BezierWayPoint waypoint
	{
		get
		{
			if(_waypoint == null)
			{
				UpdateWayPoint();
			}
			return _waypoint;
		}
	}

	public bool Loop
	{
		get 
		{
			return loop;
		}
		set 
		{
			loop = value;
			if (value == true)
			{
				modes[modes.Count - 1] = modes[0];
				SetControlPoint(0, points[0]);
			}
		}
	}

	public int ControlPointCount 
	{
		get 
		{
			return points.Count;
		}
	}

	public void UpdateWayPoint()
	{
		_waypoint = new BezierWayPoint(this, 0.01f);
	}

	void Awake()
	{
		if(points == null || points.Count <= 0)
		{
			Logger.Warning(string.Format("BezierSpline {0} has no control points", this.name));
		}
	}

	public Vector3 GetControlPoint (int index)
	{
		return points[index];
	}
    public void ModifyPoint(int index, Vector3 pos)
    {
        points[index] = pos;
    }

	public void SetControlPoint (int index, Vector3 point)
	{
		if (index % 3 == 0) 
		{
			Vector3 delta = point - points[index];
			if (loop)
			{
				if (index == 0) {
					points[1] += delta;
					points[points.Count - 2] += delta;
					points[points.Count - 1] = point;
				}
				else if (index == points.Count - 1)
				{
					points[0] = point;
					points[1] += delta;
					points[index - 1] += delta;
				}
				else 
				{
					points[index - 1] += delta;
					points[index + 1] += delta;
				}
			}
			else 
			{
				if (index > 0)
				{
					points[index - 1] += delta;
				}
				if (index + 1 < points.Count)
				{
					points[index + 1] += delta;
				}
			}
		}
		points[index] = point;
		EnforceMode(index);
		UpdateWayPoint();
	}

	public BezierControlPointMode GetControlPointMode (int index) 
	{
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode)
	{
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop)
		{
			if (modeIndex == 0) 
			{
				modes[modes.Count - 1] = mode;
			}
			else if (modeIndex == modes.Count - 1)
			{
				modes[0] = mode;
			}
		}
		EnforceMode(index);
		UpdateWayPoint();
	}

	private void EnforceMode (int index)
	{
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Count - 1)) 
		{
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) 
		{
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0)
			{
				fixedIndex = points.Count - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Count)
			{
				enforcedIndex = 1;
			}
		}
		else
		{
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Count)
			{
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0)
			{
				enforcedIndex = points.Count - 2;
			}
		}

		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned)
		{
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}
		points[enforcedIndex] = middle + enforcedTangent;
	}

	public int CurveCount 
	{
		get 
		{
			return (points.Count - 1) / 3;
		}
	}

	public Vector3 GetPoint (float t) 
	{
		return transform.TransformPoint(GetLocalPoint(t));
	}

	public Vector3 GetLocalPoint(float t)
	{
		int i;
		if (t >= 1f)
		{
			t = 1f;
			i = points.Count - 4;
		}
		else 
		{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}

		return Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
	}

	public Vector3 GetVelocity (float t)
	{
		int i;
		if (t >= 1f)
		{
			t = 1f;
			i = points.Count - 4;
		}
		else 
		{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}
	
	public Vector3 GetDirection (float t) 
	{
		return GetVelocity(t).normalized;
	}

	public void RemoveGroupPoints(int index)
	{
		//not support loop curve now...
		if(!loop)
		{
			int modeIndex = (index + 1) / 3;
			int middleIndex = modeIndex * 3;
			int preIndex = middleIndex - 1;
			int nextIndex = middleIndex + 1;

			if(nextIndex < points.Count)
			{
				points.RemoveAt(nextIndex);
			}
			points.RemoveAt(middleIndex);
			if(preIndex >= 0)
			{
				points.RemoveAt(preIndex);
			}
			if(modeIndex == modes.Count - 1)
			{
				points.RemoveAt(middleIndex - 2);
			}
			else if(modeIndex == 0)
			{
				points.RemoveAt(middleIndex + 2);
			}
			modes.RemoveAt(modeIndex);
			UpdateWayPoint();
		}
	}

	public void DivGroupPoints(int index)
	{
		int modeIndex = (index + 1) / 3;
		if(modeIndex > 0)
		{
			int lastModeIndex = modeIndex - 1;
			int pointInsertIndex = modeIndex * 3 - 1;
			float preProgress = lastModeIndex * 1f / (modes.Count - 1);
			float nextProgress = modeIndex * 1f / (modes.Count - 1);
			float divProgress = (preProgress + nextProgress) / 2f;

			var divMiddlePos = GetPoint(divProgress);
			var divDir = GetDirection(divProgress);
			var divPrePos = divMiddlePos - divDir;
			var divNextPos = divMiddlePos + divDir;

			points.Insert(pointInsertIndex, transform.InverseTransformPoint(divNextPos));
			points.Insert(pointInsertIndex, transform.InverseTransformPoint(divMiddlePos));
			points.Insert(pointInsertIndex, transform.InverseTransformPoint(divPrePos));

			modes.Insert(modeIndex, BezierControlPointMode.Aligned);
			UpdateWayPoint();
		}
	}

	public void AddCurve ()
	{
		Vector3 point = points[points.Count - 1];

		point.x += 1f;
		points.Add(point);
		point.x += 1f;
		points.Add(point);
		point.x += 1f;
		points.Add(point);

		modes.Add(modes[modes.Count - 1]);
		EnforceMode(points.Count - 4);

		if (loop) 
		{
			points[points.Count - 1] = points[0];
			modes[modes.Count - 1] = modes[0];
			EnforceMode(0);
		}

		UpdateWayPoint();
	}
	
	public void Reset () 
	{
		points = new List<Vector3> 
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
		modes = new List<BezierControlPointMode> 
		{
			BezierControlPointMode.Aligned,
			BezierControlPointMode.Aligned
		};
		UpdateWayPoint();
	}
}