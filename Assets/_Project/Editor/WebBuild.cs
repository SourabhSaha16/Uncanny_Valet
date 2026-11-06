using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Builds the Web (WebGL) player. Used by the menu, by tools/build-web.*, and by CI
    /// (game-ci/unity-builder with buildMethod: UncannyValet.EditorTools.WebBuild.BuildFromCommandLine).
    /// </summary>
    public static class WebBuild
    {
        private const string DefaultOutput = "build/WebGL/WebGL";

        [MenuItem("Uncanny Valet/Build Web Player")]
        public static void BuildFromMenu() => Build(DefaultOutput);

        // Called by CI. GameCI passes -customBuildPath; we fall back to the default folder.
        public static void BuildFromCommandLine()
        {
            var output = ReadArg("-customBuildPath") ?? DefaultOutput;
            var report = Build(output);
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static BuildReport Build(string outputPath)
        {
            ApplyWebSettings();
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("No scenes in Build Settings.");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"Web build {report.summary.result}: {outputPath}");
            return report;
        }

        // GitHub Pages can't send Content-Encoding headers, so compressed builds fail to load there.
        private static void ApplyWebSettings()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.runInBackground = true;
        }

        private static string ReadArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
