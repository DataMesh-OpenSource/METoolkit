using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VR;
using System.IO;
using System.Xml.Linq;

public class BuildApp
{


    [MenuItem("Assets/DataMesh/Build Water Mark")]
    public static void BuildWaterMark()
    {
        if (Selection.objects.Length != 1)
        {
            Debug.LogError("Please select objects!");
            return;
        }

        var sel = Selection.objects[0];
        if (!(sel is Texture2D))
        {
            Debug.LogError("Please select a pic");
            return;
        }

        string assetPath = AssetDatabase.GetAssetOrScenePath(sel);
        Debug.Log(assetPath);

        if (!assetPath.EndsWith(".png"))
        {
            Debug.LogError("Please select a PNG file!");
            return;
        }

        byte[] bt = File.ReadAllBytes(assetPath);

        Debug.Log("length=" + bt.Length);

        string str = System.Convert.ToBase64String(bt);

        Debug.Log("base64 length=" + str.Length);

        string destFile = assetPath.Substring(0, assetPath.Length - 4);
        destFile += ".txt";
        File.WriteAllText(destFile, str);

        Debug.Log("Create Success!");
    }

    /*
    [MenuItem("Assets/DataMesh/Test Water Mark")]
    public static void TestWaterMark()
    {
        byte[] waterMarkPic = System.Convert.FromBase64String(DataMesh.AR.SpectatorView.Png.png);
        Debug.Log(waterMarkPic.Length);

        File.WriteAllBytes(Application.dataPath + "/" + "test.png", waterMarkPic);
        Debug.Log(Application.dataPath + "/" + "test.png");
    }
    */

    public static void Test()
    {
        Debug.Log("old:" + EditorUserBuildSettings.GetPlatformSettings("", "metroPackageVersion"));
        Debug.Log("Test:" + EditorPrefs.GetString("metroPackageVersion"));

        Debug.Log(";;:" + EditorUserSettings.GetConfigValue("metroPackageVersion"));
    }

    public static void BuildAll()
    {
        string appId = GetArg("-AppID");
        string buildScene = GetArg("-BuildScene");
        string targetProjetPath = GetArg("-TargetProjectPath");
        string targetPlatform = GetArg("-TargetPlatform");
        string compilerParam = GetArg("-CompilerParam");
        string version = GetArg("-version");

        Debug.Log("AppID=[" + appId + "]");
        Debug.Log("Build Scene [" + buildScene + "]");
        Debug.Log("Path=[" + targetProjetPath + "]");
        Debug.Log("Platform=[" + targetPlatform + "]");
        Debug.Log("version=" + version);

        if (appId == null)
        {
            Debug.Log("appId can not be null!");
            EditorApplication.Exit(1);
            return;
        }

        PlayerSettings.productName = appId;

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.allowDebugging = false;

        BuildPlayerOptions option = new BuildPlayerOptions();
        option.options = BuildOptions.None;

        // 设置需要编译的场景
        if (buildScene != null)
        {
            option.scenes = new[] { buildScene };
        }
        else
        {
            List<string> sceneList = new List<string>();
            for (int i = 0;i < EditorBuildSettings.scenes.Length;i ++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                if (scene.enabled)
                {
                    sceneList.Add(scene.path);
                    Debug.Log("Need Scene -> " + scene.path);
                }
            }
            option.scenes = sceneList.ToArray();
        }

        // 设置平台信息
        BuildTargetGroup group = BuildTargetGroup.WSA;
        if (targetPlatform == "HoloLens" || targetPlatform == "Surface")
        {
            group = BuildTargetGroup.WSA;

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WSAPlayer);
            EditorUserBuildSettings.wsaSDK = WSASDK.UWP;

            if (targetPlatform == "HoloLens")
            {
                EditorUserBuildSettings.wsaSubtarget = WSASubtarget.HoloLens;
                PlayerSettings.virtualRealitySupported = true;
                //VRSettings.LoadDeviceByName("HoloLens");
                //VRSettings.enabled = true;
            }
            else if (targetPlatform == "Surface")
            {
                EditorUserBuildSettings.wsaSubtarget = WSASubtarget.PC;
                PlayerSettings.virtualRealitySupported = false;
            }
            EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;
            EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.LocalMachine;
            EditorUserBuildSettings.wsaGenerateReferenceProjects = true;

            option.target = BuildTarget.WSAPlayer;

            // 设置产出路径
            option.locationPathName = targetProjetPath;

            PlayerSettings.runInBackground = false;
        }
        else  if (targetPlatform == "PC")
        {
            group = BuildTargetGroup.Standalone;

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64);

            PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;

            option.target = BuildTarget.StandaloneWindows64;

            // 设置产出路径
            string ending = "";
            if (!targetProjetPath.EndsWith("/"))
                ending = "/";
            option.locationPathName = targetProjetPath + ending + appId + ".exe";

            // PC端设置为可背景运行 
            PlayerSettings.runInBackground = true;
        }
        else
        {
            Debug.LogError("[Build]No Such Platform!!");
            return;
        }

        // 设置编译参数 
        string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

        // 先排除可能需要排除的编译参数 
        define = RemoveParam(define, "ME_LIVE_ACTIVE");

        // 再添加编译参数
        if (compilerParam != null)
        {
            define += ";" + compilerParam;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, define);
        }

        // 设置路径
        Debug.Log(targetProjetPath);
        if (!System.IO.Directory.Exists(targetProjetPath))
        {
            System.IO.Directory.CreateDirectory(targetProjetPath);
        }

        // 开始编译
        string buildError = BuildPipeline.BuildPlayer(option);

        if (buildError.StartsWith("Error"))
        {
            Debug.Log("Build Error! " + buildError);
            EditorApplication.Exit(1);
            return;
        }
        else
        {
            Debug.Log("Try to change version!");
        }

        // 修改版本号
        if (targetPlatform == "HoloLens" || targetPlatform == "Surface")
        {
            SetPackageNameAndVersion(targetProjetPath, appId, version);
        }
    }

    private static string RemoveParam(string all, string paramToRemove)
    {
        int index = all.IndexOf(paramToRemove);
        string rs = all;
        if (index >= 0)
        {
            int end = index + paramToRemove.Length;
            rs = all.Substring(0, index);
            rs += all.Substring(end);
        }

        return rs;
    }

    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    private static void SetPackageNameAndVersion(string projectPath, string name, string version)
    {
        // Find the manifest, assume the one we want is the first one
        string[] manifests = Directory.GetFiles(projectPath, "Package.appxmanifest", SearchOption.AllDirectories);
        if (manifests.Length == 0)
        {
            Debug.LogError("Unable to find Package.appxmanifest file for build (in path - " + projectPath + ")");
            return;
        }
        string manifest = manifests[0];

        XElement rootNode = XElement.Load(manifest);
        XNamespace ns = rootNode.GetDefaultNamespace();
        var identityNode = rootNode.Element(ns + "Identity");
        if (identityNode == null)
        {
            Debug.LogError("Package.appxmanifest for build (in path - " + projectPath + ") is missing an <Identity /> node");
            return;
        }

        var nameAttr = identityNode.Attribute(XName.Get("Name"));
        if (nameAttr == null)
        {
            Debug.LogError("Package.appxmanifest for build (in path - " + projectPath + ") is missing a version attribute in the <Identity /> node.");
        }
        else
        {
            nameAttr.Value = name;
        }


        // We use XName.Get instead of string -> XName implicit conversion because
        // when we pass in the string "Version", the program doesn't find the attribute.
        // Best guess as to why this happens is that implicit string conversion doesn't set the namespace to empty
        var versionAttr = identityNode.Attribute(XName.Get("Version"));
        if (versionAttr == null)
        {
            Debug.LogError("Package.appxmanifest for build (in path - " + projectPath + ") is missing a version attribute in the <Identity /> node.");
        }
        else
        {
            versionAttr.Value = version;
        }

        // Assume package version always has a '.'.
        // According to https://msdn.microsoft.com/en-us/library/windows/apps/br211441.aspx
        // Package versions are always of the form Major.Minor.Build.Revision

        rootNode.Save(manifest);
    }
















    static void BuildPCStandalone()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64);

        string projectPath = Application.dataPath;
        projectPath = projectPath.Substring(0, projectPath.IndexOf("Assets")) + "VSProject_SpectatorView/";
        Debug.Log(projectPath);

        if (!System.IO.Directory.Exists(projectPath))
        {
            System.IO.Directory.CreateDirectory(projectPath);
        }

        BuildPlayerOptions option = new BuildPlayerOptions();
        option.locationPathName = projectPath;
        option.scenes = new[] { "Assets/Scenes/DemoShare.unity" };
        option.target = BuildTarget.WSAPlayer;
        option.options = BuildOptions.None;

        PlayerSettings.runInBackground = true;

        BuildPipeline.BuildPlayer(option);

    }

    static void BuildHoloLens()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WSAPlayer);
        EditorUserBuildSettings.wsaSDK = WSASDK.UWP;
        EditorUserBuildSettings.wsaSubtarget = WSASubtarget.HoloLens;
        EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;
        EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.LocalMachine;

        PlayerSettings.virtualRealitySupported = true;

        //VRSettings.LoadDeviceByName("HoloLens");
        //VRSettings.enabled = true;

        string projectPath = Application.dataPath;
        projectPath = projectPath.Substring(0, projectPath.IndexOf("Assets")) + "VSProject_Normal/";
        Debug.Log(projectPath);

        if (!System.IO.Directory.Exists(projectPath))
        {
            System.IO.Directory.CreateDirectory(projectPath);
        }

        BuildPlayerOptions option = new BuildPlayerOptions();
        option.locationPathName = projectPath;
        option.scenes = new[] { "Assets/Scenes/DemoShare.unity" };
        option.target = BuildTarget.WSAPlayer;
        option.options = BuildOptions.None;

        PlayerSettings.runInBackground = false;

        BuildPipeline.BuildPlayer(option);

    }

    static void BuildSurface()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WSAPlayer);
        EditorUserBuildSettings.wsaSDK = WSASDK.UWP;
        EditorUserBuildSettings.wsaSubtarget = WSASubtarget.PC;
        EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;
        EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.LocalMachine;

        PlayerSettings.virtualRealitySupported = false;


        string projectPath = Application.dataPath;
        projectPath = projectPath.Substring(0, projectPath.IndexOf("Assets")) + "VSProject_Normal/";
        Debug.Log(projectPath);

        if (!System.IO.Directory.Exists(projectPath))
        {
            System.IO.Directory.CreateDirectory(projectPath);
        }

        BuildPlayerOptions option = new BuildPlayerOptions();
        option.locationPathName = projectPath;
        option.scenes = new[] { "Assets/Scenes/DemoShare.unity" };
        option.target = BuildTarget.WSAPlayer;
        option.options = BuildOptions.None;

        PlayerSettings.runInBackground = false;

        BuildPipeline.BuildPlayer(option);

    }
}
