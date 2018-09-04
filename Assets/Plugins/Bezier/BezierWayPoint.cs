using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BezierWayPoint
{
	public Transform _splineTransform;
	public List<Vector3> _pointList = new List<Vector3>();
	public List<float> _lenList = new List<float>();

	public BezierWayPoint(BezierSpline spline, float stepSize)
	{
		if(spline.ControlPointCount <= 0)
		{
			Logger.Error(string.Format("BezierSpline {0} has no control points, can not create waypoint for it", spline.name));
			_pointList.Add(Vector3.zero);
			_lenList.Add(0);
			return;
		}

		_splineTransform = spline.transform;
		AddPointAtProgress(spline, 0);
		for(float begin = 0f; begin < 1f ;begin += stepSize)
		{
			AddPointAtProgress(spline, Mathf.Min(1, begin+stepSize));
		}
	}

	void AddPointAtProgress(BezierSpline curve, float p)
	{
		var point = curve.GetLocalPoint(p);
		var pointCount = _pointList.Count;
		
		if(pointCount >= 1)
		{
			var diff = point - _pointList[pointCount-1];
			_lenList.Add(_lenList[pointCount-1] + diff.magnitude);
			_pointList.Add(point);
		}
		else
		{
			_lenList.Add(0);
			_pointList.Add(point);
		}
	}

	public float GetLength()
	{
		return _lenList[_lenList.Count-1];
	}

	public int GetPointCount()
	{
		return _pointList.Count;
	}

	public Vector3 GetPointByProgress(float progress)
	{
		return GetPointByLen(progress * GetLength());
	}

	public Vector3 GetPointByLen(float len)
	{
		int left = 0;
		int right = _lenList.Count - 1;
		len = Mathf.Clamp(len, 0, GetLength());

		//find first element >= len
		while(left < right)
		{
			int mid = (left + right)/2;
			if(_lenList[mid] >= len)
			{
				right = mid;
			}
			else
			{
				left = mid + 1;
			}
		}

		int pointCount = _pointList.Count;
		int hitIndex = right;
		float hitLen = _lenList[hitIndex];
		var point = Vector3.zero;

		//no hit, return last point
		if(hitLen < len)
		{
			point = _pointList[pointCount - 1];
		}
		//hit first point, len=0, return first point
		else if(hitIndex <= 0)
		{
			point = _pointList[0];
		}
		else
		{
			int preIndex = hitIndex - 1;
			float preLen = _lenList[preIndex];
			var ratio = 1f;
			if(preLen != hitLen)
			{
				ratio = Mathf.InverseLerp(preLen, hitLen, len);
			}
			point = Vector3.Lerp(_pointList[preIndex], _pointList[hitIndex], ratio);
		}

		return _splineTransform.TransformPoint(point);
	}
}

