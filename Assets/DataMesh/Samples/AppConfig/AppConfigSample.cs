using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataMesh.AR.Utility;

public class AppConfigSample : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {

        AppConfig config = AppConfig.Instance;
        config.LoadConfig("test_config.ini");

        Debug.Log("Prop1=" + config.GetConfigByFileName("test_config.ini", "Prop1"));
        Debug.Log("Prop2=" + config.GetConfigByFileName("test_config.ini", "Prop2", "tttttt"));
        Debug.Log("Prop4=" + config.GetConfigByFileName("test_config.ini", "Prop4", "tttttt"));


    }
}
