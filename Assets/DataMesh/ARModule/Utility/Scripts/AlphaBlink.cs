using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaBlink : MonoBehaviour
{
    public float alphaMaX = 1f;
    public float alphaMin = 0.3f;

    private  bool blink = false;

    private Material mat;

    private float alpha = 1;
    private Color color;
    private float speed = 0.03f;
    private float factor;

    void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            mat = renderer.material;
        }
    }

    public void StartBlink()
    {
        if (mat != null)
        {
            alpha = 1;
            color = mat.color;
            factor = -speed;
            blink = true;
        }
    }

    public void StopBlink()
    {
        blink = false;
        color.a = alphaMaX;
        mat.color = color;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
		if (blink)
        {
            if (mat != null)
            {

                alpha += factor;
                if (alpha > alphaMaX)
                {
                    alpha = alphaMaX;
                    factor = -speed;
                }
                if (alpha < alphaMin)
                {
                    alpha = alphaMin;
                    factor = speed;
                }
                color.a = alpha;

                mat.color = color;
            }
        }
	}
}
