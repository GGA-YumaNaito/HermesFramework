using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// ビルド
/// </summary>
public static class JenkinsBuild
{
    static string applicationIdentifier = "com.gga.hermes-framework";
    static string productName = "HermesFramework";
    static string companyName = "GGA";
    static string buildName = "Build";
    static string defineSymbols = string.Empty;
    static string environment = string.Empty;

    /// <summary>
    /// ビルド
    /// </summary>
    public static void Build()
    {
        var platform = BuildTarget.StandaloneWindows;

        // 引数取得
        var args = System.Environment.GetCommandLineArgs();

        int i, len = args.Length;
        for (i = 0; i < len; i++)
        {
            switch (args[i])
            {
                // develop, stage, release
                case "/environment":
                    environment = args[i + 1];
                    break;
                // Android, iOS
                case "/platform":
                    platform = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), args[i + 1]);
                    break;
                // Application Identifier
                case "/applicationIdentifier":
                    applicationIdentifier = args[i + 1];
                    break;
                // ProductName
                case "/productName":
                    productName = args[i + 1];
                    break;
                // Company Name
                case "/companyName":
                    companyName = args[i + 1];
                    break;
                // Build Name
                case "/buildName":
                    buildName = args[i + 1];
                    break;
                // Define Symbols
                case "/defineSymbols":
                    defineSymbols = args[i + 1];
                    break;
            }
        }

        if (platform == BuildTarget.Android)
            BuildAndroid();
    }

    [MenuItem("Hermes/Build/ApplicationBuild/Android")]
    /// <summary>
    /// Androidビルド
    /// </summary>
    public static void BuildAndroid()
    {
        // Switch Platform
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        // デファインシンボル
        var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        if (string.IsNullOrEmpty(defines))
        {
            defines = defineSymbols;
        }
        else
        {
            defines += string.IsNullOrEmpty(defineSymbols) ? "" : ";" + defineSymbols;
        }
        defines = string.Join(";", defines.Split(";").Distinct());
        PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        //PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, "");
        ////PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "");
        ////PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, "");

        //PlayerSettings.applicationIdentifier = applicationIdentifier;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationIdentifier);
        PlayerSettings.productName = productName;
        PlayerSettings.companyName = companyName;

        var scene_name_array = CreateBuildTargetScenes().ToArray();

        //Splash Screenをオフにする(Personalだと動かないよ)
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;

        //AppBundleは使用しない(本番ビルドのときだけ使うイメージ)
        EditorUserBuildSettings.buildAppBundle = false;

        BuildOptions buildOptions;
        if (environment == "develop")
            buildOptions = BuildOptions.Development;
        else
            buildOptions = BuildOptions.None;
        var report = BuildPipeline.BuildPlayer(scene_name_array, buildName + ".apk", BuildTarget.Android, buildOptions);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            Debug.Log($"Build succeeded: {summary.totalSize} bytes");
        else if (summary.result == BuildResult.Failed)
            Debug.LogError("Build failed");
    }

    #region Util
    /// <summary>
    /// ビルドターゲットシーン作成
    /// </summary>
    /// <returns>ビルドターゲットシーン</returns>
    static IEnumerable<string> CreateBuildTargetScenes()
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                yield return scene.path;
        }
    }

    #endregion
}