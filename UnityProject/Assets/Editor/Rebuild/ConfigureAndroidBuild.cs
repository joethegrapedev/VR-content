using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;

/// <summary>
/// Configures the Android/Quest build target.
///
/// Two settings here are black-screen causes in their own right:
/// 1. XR Plug-in Management stores loader configuration PER BUILD TARGET. The shipped build only
///    ever configured Standalone (boot.config pins OculusXRPlugin there), so the Android tab has
///    no loader — and an Android build with no XR loader never starts VR rendering.
/// 2. The previous Quest build targeted ARMv7 + Mono. A 32-bit address space cannot hold the
///    main scene's ~1.1 GB of assets, so it dies on allocation. ARM64 + IL2CPP is required.
///
/// Run: Unity -batchmode -quit -projectPath P -executeMethod ConfigureAndroidBuild.Apply
/// </summary>
public static class ConfigureAndroidBuild
{
    private const string BundleIdentifier = "com.sp.fyp3a25.rsafvrwarehouse";
    private const string OculusLoaderType = "Unity.XR.Oculus.OculusLoader";
    private const int TargetFrameRateHz = 72;

    public static void Apply()
    {
        try
        {
            ConfigureExternalTools();
            ApplyAndroidExternalToolsApi();
            ConfigurePlayerSettings();
            ConfigureXrForAndroid();
            ConfigureTimeStep();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ConfigureAndroidBuild] OK");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConfigureAndroidBuild] FAILED: {e}");
            EditorApplication.Exit(1);
        }
    }


    /// <summary>
    /// Points Unity at the Android toolchain. The playback engine shipped without bundled
    /// SDK/NDK/JDK, so these must be supplied explicitly or the build fails immediately.
    /// Paths come from the environment so this is not tied to one machine, with the standard
    /// install locations as fallbacks.
    /// </summary>
    public static void ConfigureExternalTools()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var jdk = ResolvePath("UNITY_JDK_PATH",
            "/opt/homebrew/opt/openjdk@11/libexec/openjdk.jdk/Contents/Home");
        var sdk = ResolvePath("UNITY_ANDROID_SDK",
            Path.Combine(home, "Library/Android/sdk"));
        var ndk = ResolvePath("UNITY_ANDROID_NDK",
            Path.Combine(home, "Library/Android/sdk/ndk/21.3.6528147"));
        var gradle = ResolvePath("UNITY_GRADLE_PATH",
            "/Applications/Unity/PlaybackEngines/AndroidPlayer/Tools/gradle");

        SetToolPath("JdkPath", "JdkUseEmbedded", jdk, "JDK");
        SetToolPath("AndroidSdkRoot", "SdkUseEmbedded", sdk, "Android SDK");
        // Unity keys the NDK preference by version; r21d is what 2021.3 pins.
        SetToolPath("AndroidNdkRootR21D", "NdkUseEmbedded", ndk, "Android NDK (r21d)");
        SetToolPath("GradlePath", "GradleUseEmbedded", gradle, "Gradle");
    }

    private static string ResolvePath(string environmentVariable, string fallback)
    {
        var fromEnvironment = Environment.GetEnvironmentVariable(environmentVariable);
        return string.IsNullOrEmpty(fromEnvironment) ? fallback : fromEnvironment;
    }

    private static void SetToolPath(string pathKey, string useEmbeddedKey, string path, string label)
    {
        if (!Directory.Exists(path))
        {
            Debug.LogError($"[ConfigureAndroidBuild] {label} not found at '{path}' — build will fail.");
            return;
        }

        EditorPrefs.SetString(pathKey, path);
        EditorPrefs.SetBool(useEmbeddedKey, false);
        Debug.Log($"[ConfigureAndroidBuild] {label}: {path}");
    }


    /// <summary>
    /// Unity 2021.3 reads Android tool locations from UnityEditor.Android.AndroidExternalToolsSettings,
    /// not from EditorPrefs, so the build fails with "JDK not found" if only EditorPrefs are set.
    /// Reflected to avoid a hard dependency on the Android extension assembly.
    /// </summary>
    public static void ApplyAndroidExternalToolsApi()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("UnityEditor.Android.AndroidExternalToolsSettings"))
            .FirstOrDefault(t2 => t2 != null);

        if (type == null)
        {
            Debug.LogWarning("[ConfigureAndroidBuild] AndroidExternalToolsSettings type not found.");
            return;
        }

        SetProperty(type, "jdkRootPath", ResolvePath("UNITY_JDK_PATH",
            "/opt/homebrew/opt/openjdk@11/libexec/openjdk.jdk/Contents/Home"));
        SetProperty(type, "sdkRootPath", ResolvePath("UNITY_ANDROID_SDK",
            Path.Combine(home, "Library/Android/sdk")));
        SetProperty(type, "ndkRootPath", ResolvePath("UNITY_ANDROID_NDK",
            Path.Combine(home, "Library/Android/sdk/ndk/21.3.6528147")));
        SetProperty(type, "gradlePath", ResolvePath("UNITY_GRADLE_PATH",
            "/Applications/Unity/PlaybackEngines/AndroidPlayer/Tools/gradle"));
    }

    private static void SetProperty(Type type, string propertyName, string value)
    {
        var property = type.GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (property == null || !property.CanWrite)
        {
            Debug.LogWarning($"[ConfigureAndroidBuild] property '{propertyName}' not settable.");
            return;
        }

        if (!Directory.Exists(value))
        {
            Debug.LogError($"[ConfigureAndroidBuild] path for '{propertyName}' missing: {value}");
            return;
        }

        // Unity's own validator rejects some otherwise-valid JDK layouts. Treat this API as
        // best-effort: EditorPrefs plus JAVA_HOME still drive the Gradle invocation, so a
        // rejection here must not abort the whole build.
        try
        {
            property.SetValue(null, value);
            Debug.Log($"[ConfigureAndroidBuild] AndroidExternalToolsSettings.{propertyName} = {value}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ConfigureAndroidBuild] {propertyName} rejected ('{value}'): " +
                             $"{e.InnerException?.Message ?? e.Message}");
        }
    }

    private static void ConfigurePlayerSettings()
    {
        var android = NamedBuildTarget.Android;

        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(android, ApiCompatibilityLevel.NET_Standard_2_0);

        // ARM64 only. ARMv7 is what broke the previous build and no current Quest needs it.
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Vulkan first, GLES3 as fallback. GLES2 must not be present: with Linear colour space
        // it produces a black screen on Android.
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]
        {
            GraphicsDeviceType.Vulkan,
            GraphicsDeviceType.OpenGLES3,
        });

        PlayerSettings.colorSpace = ColorSpace.Linear;

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        PlayerSettings.SetApplicationIdentifier(android, BundleIdentifier);
        PlayerSettings.companyName = "SP_FYP_3A25";
        PlayerSettings.productName = "RSAF_VRWarehouse";

        // VR renders its own view; the desktop-style splash and auto-rotation are meaningless.
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

        Debug.Log("[ConfigureAndroidBuild] player settings: IL2CPP, ARM64, Vulkan+GLES3, Linear, ASTC");
    }

    /// <summary>Enables the Oculus XR loader on the Android build target.</summary>
    private static void ConfigureXrForAndroid()
    {
        if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,
                out XRGeneralSettingsPerBuildTarget perTarget) || perTarget == null)
        {
            if (!Directory.Exists("Assets/XR"))
            {
                Directory.CreateDirectory("Assets/XR");
                AssetDatabase.Refresh();
            }
            perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(perTarget, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perTarget, true);
            Debug.Log("[ConfigureAndroidBuild] created XRGeneralSettingsPerBuildTarget");
        }

        perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        var settings = perTarget.SettingsForBuildTarget(BuildTargetGroup.Android);
        if (settings == null)
        {
            throw new InvalidOperationException("No XRGeneralSettings for Android after creation.");
        }

        if (settings.Manager == null)
        {
            settings.Manager = ScriptableObject.CreateInstance<XRManagerSettings>();
            AssetDatabase.AddObjectToAsset(settings.Manager, settings);
        }

        settings.InitManagerOnStart = true;

        var assigned = XRPackageMetadataStore.AssignLoader(
            settings.Manager, OculusLoaderType, BuildTargetGroup.Android);
        if (!assigned)
        {
            throw new InvalidOperationException($"Could not assign XR loader '{OculusLoaderType}' for Android.");
        }

        EditorUtility.SetDirty(settings);
        EditorUtility.SetDirty(perTarget);

        var loaders = settings.Manager.activeLoaders?.Select(l => l.GetType().FullName).ToArray()
                      ?? Array.Empty<string>();
        Debug.Log($"[ConfigureAndroidBuild] Android XR loaders: [{string.Join(", ", loaders)}]");

        if (loaders.Length == 0)
        {
            throw new InvalidOperationException("Android XR loader list is empty after assignment.");
        }
    }

    /// <summary>
    /// The original project ran a 1/72 s fixed timestep, matching Quest's 72 Hz refresh.
    /// Preserve it so physics and comfort behave as authored.
    /// </summary>
    private static void ConfigureTimeStep()
    {
        var expected = 1f / TargetFrameRateHz;
        var actual = Time.fixedDeltaTime;
        if (Mathf.Abs(actual - expected) > 0.0001f)
        {
            Debug.LogWarning($"[ConfigureAndroidBuild] fixed timestep was {actual}, setting to {expected}");
            Time.fixedDeltaTime = expected;
        }
        else
        {
            Debug.Log($"[ConfigureAndroidBuild] fixed timestep already {actual} (~{TargetFrameRateHz} Hz)");
        }
    }
}
