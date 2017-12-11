using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VuforiaManager : MonoBehaviour
{
    public static VuforiaManager Instance;
    private GameObject holoRoot;
    private GameObject ARCamera;
    private Camera VuforiaCamera;
    private GameObject VuforiaExtend;
    private GameObject TextureBufferCamera;
    public bool VuforiaEnabled;
    void Awake()
    {
        Instance = this;
    }
    // Use this for initialization
    void Start()
    {

        //Init();

    }
    // Use this for initialization
    public void Init()
    {
#if ME_VUFORIA_ACTIVE

        holoRoot = GameObject.Find("VuforiaObjects");
        holoRoot.SetActive(true);
        ARCamera = GameObject.Find("ARCamera");
        AudioListener AudioListener = ARCamera.GetComponent<AudioListener>();
        if (ARCamera != null)
        {
            VuforiaCamera = ARCamera.GetComponentInChildren<Camera>();
            if (VuforiaCamera != null)
            {
                VuforiaCamera.fieldOfView = 60;
                VuforiaCamera.depth = -10;

            }
        }

        if (AudioListener)
        {
            DestroyImmediate(AudioListener);
        }
        holoRoot = GameObject.Find("VuforiaObjects");
        holoRoot.SetActive(true);
        VuforiaExtend = GameObject.Find("VuforiaExtend");
        VuforiaExtend.SetActive(true);
        StartCoroutine(LaterFindObject());

        Vuforia.VuforiaBehaviour.Instance.SetAppLicenseKey(DataMesh.AR.Anchor.VuforiaExtend.Instance.VuforiaKey);
        if (VuforiaEnabled)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
#endif
    }
    [ContextMenu("TurnOn")]
    public void TurnOn()
    {
        if (holoRoot != null)
        {
            holoRoot.SetActive(true);
        }
        if (VuforiaExtend != null)
        {
            VuforiaExtend.SetActive(true);
        }
        if (TextureBufferCamera != null)
        {
            TextureBufferCamera.SetActive(true);
        }

    }
    IEnumerator LaterFindObject()
    {
        int tmp = 0;
        while (true)
        {
            tmp++;
            TextureBufferCamera = GameObject.Find("TextureBufferCamera");
            if (TextureBufferCamera != null)
            {
                break;
            }
            if (tmp > 2000)
            {
                break;
            }
            yield return null;
        }

    }


    [ContextMenu("TurnOff")]
    public void TurnOff()
    {
        if (holoRoot != null)
        {
            holoRoot.SetActive(false);
        }
        if (VuforiaExtend != null)
        {
            VuforiaExtend.SetActive(false);
        }
        if (TextureBufferCamera != null)
        {
            TextureBufferCamera.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

}
