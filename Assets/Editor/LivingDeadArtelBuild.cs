using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class LivingDeadArtelBuild
{
    public static void BuildWindows64()
    {
        var output = Environment.GetEnvironmentVariable("LIVING_DEAD_ARTEL_OUTPUT")
            ?? "Build/ARTEL/LivingDead.exe";
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
                .Select(scene => scene.path).ToArray(),
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("ARTEL development build failed: " + report.summary.result);
    }
}
