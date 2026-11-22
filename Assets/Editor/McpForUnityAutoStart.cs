using System.Threading.Tasks;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace GameJam26.Editor
{
    /// <summary>
    /// One-shot bootstrap: enables "Auto-Start on Editor Load" for MCP for Unity,
    /// starts the local HTTP MCP server and retries the bridge until Unity and the
    /// server connect (first run installs Python deps, so it can race). Delete this
    /// file if MCP for Unity handles startup by itself later.
    /// </summary>
    [InitializeOnLoad]
    public static class McpForUnityAutoStart
    {
        private const string SessionKey = "GameJam26.McpForUnityAutoStart.Done2";

        static McpForUnityAutoStart()
        {
            EditorApplication.delayCall += Bootstrap;
        }

        private static async void Bootstrap()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);

            MCPServiceLocator.Server.StartLocalHttpServer(quiet: true);

            // Wait for the HTTP server to boot (uvx may install dependencies on first run).
            for (int i = 0; i < 60; i++)
            {
                await Task.Delay(2000);
                try
                {
                    bool bridge = await MCPServiceLocator.Bridge.StartAsync();
                    if (bridge)
                    {
                        Debug.Log($"[McpForUnityAutoStart] Bridge connected after {i * 2}s.");
                        SessionState.SetBool(SessionKey, true);
                        return;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[McpForUnityAutoStart] Bridge retry {i}: {ex.Message}");
                }
            }

            Debug.LogWarning("[McpForUnityAutoStart] Bridge did not connect in 120s. Open Tools > MCP For Unity to diagnose.");
        }
    }
}
