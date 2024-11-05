#if UNITY_EDITOR
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;

[InitializeOnLoad]
public static class VisionOsTargetConfiguration {
    const string OCULUS_SDK_AVAILABLE = "OCULUS_SDK_AVAILABLE";

    static VisionOsTargetConfiguration() {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.VisionOS) {
            CheckMetaContext();
        }
    }

    static void CheckMetaContext() {
        var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        if (defines.Contains(OCULUS_SDK_AVAILABLE))
        {
            DisplayErrorWithDefine();
        }
        else {
            Fusion.XRShared.Tools.PackagePresenceCheck.LookForPackage(packageToSearch: "com.meta.xr.sdk.core", packageLookupCallback: (packageInfo) => {
                if (packageInfo != null)
                {
                    DisplayErrorWithoutDefine();
                }
            });
        } 
    }

    static void DisplayErrorWithDefine()
    {
        Debug.LogError("Meta Core SDK is not compatible with visionOs builds. Before building, some changes are required.");
        Debug.LogError("1) Uninstall com.meta.xr.sdk.platform, com.meta.xr.mrutilitykit and finally com.meta.xr.sdk.core");
        Debug.LogError("2) Make sure that OCULUS_SDK_AVAILABLE is not present in the scripting define symbols for the visionOS target");
    }

    static void DisplayErrorWithoutDefine()
    {
        Debug.LogError("Meta Core SDK is not compatible with visionOs builds. Before building, some changes are required.");
        Debug.LogError("- Uninstall com.meta.xr.sdk.platform, com.meta.xr.mrutilitykit and finally com.meta.xr.sdk.core");
    }
}
#endif
