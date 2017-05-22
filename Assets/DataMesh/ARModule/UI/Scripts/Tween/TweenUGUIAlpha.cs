using UnityEngine;
using UnityEngine.UI;

public class TweenUGUIAlpha : UITweener
{
	[Range(0f, 1f)] public float from = 1f;
	[Range(0f, 1f)] public float to = 1f;

	bool mCached = false;
	CanvasRenderer mCr;
    CanvasRenderer[] subCrs = null;

    [System.Obsolete("Use 'value' instead")]
	public float alpha { get { return this.value; } set { this.value = value; } }
    public bool controlSubObject = false;

	void Cache ()
	{
		mCached = true;
		mCr = GetComponent<CanvasRenderer>();

        if (controlSubObject)
        {
            subCrs = GetComponentsInChildren<CanvasRenderer>();
        }

	}

	/// <summary>
	/// Tween's current value.
	/// </summary>

	public float value
	{
		get
		{
			if (!mCached) Cache();
            return mCr != null ? mCr.GetAlpha() : 1f;
		}
		set
		{
			if (!mCached) Cache();

			if (mCr != null)
			{
				mCr.SetAlpha(value);
			}
            if (subCrs != null)
            {
                for (int i = 0; i < subCrs.Length; i++)
                {
                    subCrs[i].SetAlpha(value);
                }
            }
        }
    }

	/// <summary>
	/// Tween the value.
	/// </summary>

	protected override void OnUpdate (float factor, bool isFinished) { value = Mathf.Lerp(from, to, factor); }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenUGUIAlpha Begin (GameObject go, float duration, float alpha)
	{
        TweenUGUIAlpha comp = UITweener.Begin<TweenUGUIAlpha>(go, duration);
		comp.from = comp.value;
		comp.to = alpha;

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}

	public override void SetStartToCurrentValue () { from = value; }
	public override void SetEndToCurrentValue () { to = value; }
}
