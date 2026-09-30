using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebGLBuild
{
    public static void Build()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        string[] scenes = Array.ConvertAll(
            Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled),
            scene => scene.path);

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes are listed in Build Settings.");
        }

        string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "WebGL"));
        Directory.CreateDirectory(outputPath);

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"WebGL build {report.summary.result}: {report.summary.totalErrors} error(s).");
        }

        string indexPath = Path.Combine(outputPath, "index.html");
        string indexHtml = File.ReadAllText(indexPath);
        const string mobilePixelRatioLine = "        // config.devicePixelRatio = 1;";
        const string optimizedPixelRatioLine = "        config.devicePixelRatio = 1.25;";

        if (!indexHtml.Contains(optimizedPixelRatioLine))
        {
            if (!indexHtml.Contains(mobilePixelRatioLine))
            {
                throw new InvalidOperationException(
                    "Could not enable mobile WebGL resolution scaling in the generated index.html.");
            }

            indexHtml = indexHtml.Replace(mobilePixelRatioLine, optimizedPixelRatioLine);
            File.WriteAllText(indexPath, indexHtml);
        }

        Debug.Log($"WebGL build succeeded: {outputPath} ({report.summary.totalSize / (1024 * 1024)} MB)");
    }
}
