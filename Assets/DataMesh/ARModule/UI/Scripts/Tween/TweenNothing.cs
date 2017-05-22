//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's position.
/// </summary>

[AddComponentMenu("NGUI/Tween/Tween Nothing")]
public class TweenNothing : UITweener
{
    [HideInInspector]

    /// <summary>
    /// Tween's current value.
    /// </summary>

	public System.Action<float,bool> OnChanged;

    protected override void OnUpdate(float factor, bool isFinished)
    {
		if (OnChanged != null) {OnChanged(factor,isFinished);}
    }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

    static public TweenNothing Begin(GameObject go, float duration, float x)
	{
        TweenNothing comp = UITweener.Begin<TweenNothing>(go, duration);

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}

}
