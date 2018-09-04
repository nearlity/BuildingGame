using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode, RequireComponent(typeof(BezierSpline))]
public class BezierAnimation : MonoBehaviour
{
	[SerializeField]
	protected BezierSpline _spline;
	public BezierSpline spline
	{
		get
		{
			return _spline;
		}
	}

	[SerializeField]
	protected bool _balanceViewDir = true;
	public bool balanceViewDir
	{
		get
		{
			return _balanceViewDir;
		}
		set
		{
			_balanceViewDir = value;
		}
	}

	void Awake()
	{
		_spline = GetComponent<BezierSpline>();
	}

	[System.Serializable]
	public struct FrameData
	{
		public float progress;
		public float time;
		public Quaternion viewDir;
		public float fov;
		public AnimationCurve reachCurve;

		public FrameData(float progress, float time):this(progress, time, Quaternion.identity, 60f, AnimationCurve.Linear(0, 0, 1, 1))
		{
		}

		public FrameData(float progress, float time, Quaternion viewDir, float fov, AnimationCurve reachCurve)
		{
			this.progress = progress;
			this.time = time;
			this.viewDir = viewDir;
			this.fov = fov;
			this.reachCurve = reachCurve;
		}

        public FrameData Clone()
        {
            FrameData res = new FrameData();
            res.progress = this.progress;
            res.time = this.time;
            res.viewDir = this.viewDir;
            res.fov = this.fov;
            res.reachCurve = new AnimationCurve(this.reachCurve.keys);

            return res;
        }
	}

	[SerializeField]
	protected List<FrameData> _frameList = new List<FrameData>();
	public int frameCount
	{
		get
		{
			return _frameList.Count;
		}
	}

	public void AddFrame(FrameData data)
	{
		_frameList.Add(data);
	}

	public FrameData GetFrame(int index)
	{
		return _frameList[index];
	}

	public void SetFrame(int index, FrameData data)
	{
		_frameList[index] = data;
	}

	public void InsertFrame(int index, FrameData data)
	{
		_frameList.Insert(index, data);
	}

	public void RemoveFrame(int index)
	{
		_frameList.RemoveAt(index);
	}
}
