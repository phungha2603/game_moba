using System;
using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace Project.EditorTools
{
    /// Dev-container-only: this build host's outbound HTTPS goes through a local
    /// forwarding proxy whose port can change between sessions. The JVM that Unity
    /// spawns for Gradle does not inherit the proxy env vars, so dependency
    /// resolution (the Android Gradle Plugin, AndroidX, etc.) fails silently.
    /// Mirrors HTTPS_PROXY/HTTP_PROXY into the generated gradle.properties as
    /// systemProp.*.proxyHost/Port on every build, so it's never stale.
    public class AndroidGradleProxyPatcher : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string proxyUrl = Environment.GetEnvironmentVariable("HTTPS_PROXY")
                               ?? Environment.GetEnvironmentVariable("https_proxy");
            if (string.IsNullOrEmpty(proxyUrl))
            {
                return;
            }

            if (!Uri.TryCreate(proxyUrl, UriKind.Absolute, out Uri proxy))
            {
                Debug.LogWarning($"AndroidGradleProxyPatcher: could not parse HTTPS_PROXY '{proxyUrl}'.");
                return;
            }

            string propsPath = Path.Combine(path, "gradle.properties");
            string proxyLines =
                $"\nsystemProp.http.proxyHost={proxy.Host}\n" +
                $"systemProp.http.proxyPort={proxy.Port}\n" +
                $"systemProp.https.proxyHost={proxy.Host}\n" +
                $"systemProp.https.proxyPort={proxy.Port}\n" +
                "systemProp.http.nonProxyHosts=localhost|127.*|[::1]\n" +
                "systemProp.https.nonProxyHosts=localhost|127.*|[::1]\n";

            File.AppendAllText(propsPath, proxyLines);
            Debug.Log($"AndroidGradleProxyPatcher: wrote proxy {proxy.Host}:{proxy.Port} into {propsPath}");
        }
    }
}
