using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;

namespace UnityWebSocket.Editor
{
    public class SettingsWindow : EditorWindow
    {
        static SettingsWindow window = null;
        [MenuItem("Tools/UnityWebSocket", priority = 1)]
        public static void Open()
        {
            if (window != null)
            {
                window.Close();
            }

            window = GetWindow<SettingsWindow>(true, "UnityWebSocket");
            window.minSize = window.maxSize = new Vector2(600, 310);
            window.Show();
            window.BeginCheck();
        }

        private void OnGUI()
        {
            DrawLogo();
            DrawVersion();
            DrawSeparator(80);
            DrawFixSettings();
            DrawSeparator(186);
            DrawHelper();
            DrawFooter();
        }

        private void DrawLogo()
        {
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/UnityWebSocket/Scripts/Editor/logo.png");
            if (logo == null)
            {
                logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UnityWebSocket/Scripts/Editor/logo.png");
            }

            if (logo != null)
            {
                var logoPos = new Rect(10, 10, 66, 66);
                GUI.DrawTexture(logoPos, logo);
            }

            var title = "<color=#3A9AD8><b>UnityWebSocket</b></color>";
            var titlePos = new Rect(80, 28, 500, 50);
            GUI.Label(titlePos, title, TextStyle(24));
        }

        private void DrawSeparator(int y)
        {
            EditorGUI.DrawRect(new Rect(10, y, 580, 1), Color.white * 0.5f);
        }

        private GUIStyle TextStyle(int fontSize = 10, TextAnchor alignment = TextAnchor.UpperLeft)
        {
            var style = new GUIStyle();
            style.fontSize = fontSize;
            style.normal.textColor = Color.white * 0.85f;
            style.alignment = alignment;
            style.richText = true;
            return style;
        }

        private void DrawVersion()
        {
            GUI.Label(new Rect(440, 10, 150, 10), "Current Version: " + Settings.VERSION, TextStyle(alignment: TextAnchor.MiddleCenter));
            if (string.IsNullOrEmpty(latestVersion))
            {
                GUI.Label(new Rect(440, 30, 150, 10), "Checking Update...", TextStyle(alignment: TextAnchor.MiddleCenter));
            }
            else
            {
                GUI.Label(new Rect(440, 30, 150, 10), "Latest Version: " + latestVersion, TextStyle(alignment: TextAnchor.MiddleCenter));
                if (Settings.VERSION == latestVersion)
                {
                    if (GUI.Button(new Rect(440, 50, 150, 18), "Check Update"))
                    {
                        latestVersion = "";
                        changeLog = "";
                        BeginCheck();
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(440, 50, 150, 18), "Update to | " + latestVersion))
                    {
                        ShowUpdateDialog();
                    }
                }
            }
        }

        private void ShowUpdateDialog()
        {
            var code = EditorUtility.DisplayDialog("UnityWebSocket",
                "Update UnityWebSocket now?\n" + changeLog,
                "Update Now", "Cancel");

            if (code)
            {
                UpdateVersion();
            }
        }

        private void UpdateVersion()
        {
            // via git
            var packagePath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            if (File.Exists(packagePath))
            {
                var txt = File.ReadAllText(packagePath);
                var index = txt.IndexOf("\"" + Settings.PACKAGE_NAME + "\"");
                if (index != -1)
                {
                    var end_index = txt.IndexOf(",", index);
                    var old_str = txt.Substring(index, end_index - index);
                    var new_str = string.Format("\"{0}\": \"{1}#{2}\"", Settings.PACKAGE_NAME, Settings.UPM_URL, latestVersion);
                    txt = txt.Replace(old_str, new_str);
                    File.WriteAllText(packagePath, txt);
                    AssetDatabase.Refresh();
                    return;
                }
            }

            // via releases
            Application.OpenURL(Settings.GITHUB);
        }

        private void DrawFixSettings()
        {
            bool isRuntimeVersionFixed;
            bool isLinkTargetFixed;
            bool isMemorySizeFixed;
            bool isDecompressionFallbackFixed;
            PlayerSettingsChecker.GetSettingsFixed(out isRuntimeVersionFixed, out isLinkTargetFixed, out isMemorySizeFixed, out isDecompressionFallbackFixed);
            bool isAllFixed = isRuntimeVersionFixed && isLinkTargetFixed && isMemorySizeFixed && isDecompressionFallbackFixed;
            if (isAllFixed)
            {
                var str = "All Settings Fixed:";
                str += "\n√  PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Both;";
                str += "\n√  PlayerSettings.WebGL.memorySize = " + PlayerSettings.WebGL.memorySize + ";";
                str += "\n√  PlayerSettings.WebGL.decompressionFallback = true;";
#if !UNITY_2019_3_OR_NEWER
                str += "\n√  PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;";
#endif
                EditorGUI.HelpBox(new Rect(10, 90, 580, 60), str, MessageType.Info);
                GUI.enabled = false;
                GUI.Button(new Rect(440, 158, 150, 18), "Auto Fix");
                GUI.enabled = true;
                return;
            }

            var fixStr = "In order to run UnityWebSocket normally, we must fix some SETTINGS below:";
            if (isLinkTargetFixed)
                fixStr += "\n√  PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Both;";
            else
                fixStr += "\n×  PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Both;";

            if (isMemorySizeFixed)
                fixStr += "\n√  PlayerSettings.WebGL.memorySize = " + PlayerSettings.WebGL.memorySize + ";";
            else
                fixStr += "\n×  PlayerSettings.WebGL.memorySize = [Appropriate Value];";

            if (isDecompressionFallbackFixed)
                fixStr += "\n√  PlayerSettings.WebGL.decompressionFallback = true;";
            else
                fixStr += "\n×  PlayerSettings.WebGL.decompressionFallback = true;";

#if !UNITY_2019_3_OR_NEWER
            if (isRuntimeVersionFixed)
                fixStr += "\n√  PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;";
            else
                fixStr += "\n×  PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest; (Need Manual Fix)";
#endif

            EditorGUI.HelpBox(new Rect(10, 90, 580, 60), fixStr, MessageType.Warning);

            if (GUI.Button(new Rect(440, 158, 150, 18), "Auto Fix"))
            {
#if UNITY_2018_1_OR_NEWER
                if (!isLinkTargetFixed)
                    PlayerSettings.WebGL.linkerTarget = (WebGLLinkerTarget)2;
#endif

#if UNITY_2019_1_OR_NEWER
                if (!isMemorySizeFixed)
                    PlayerSettings.WebGL.memorySize = 128;
#endif

#if UNITY_2020_1_OR_NEWER
                if (!isDecompressionFallbackFixed)
                    PlayerSettings.WebGL.decompressionFallback = true;
#endif

                if (!isRuntimeVersionFixed)
                    EditorUtility.DisplayDialog("UnityWebSocket", "ScriptingRuntimeVersion (.Net 4.x) Need Manual Fix.", "OK");
            }
        }

        private void DrawHelper()
        {
            GUI.Label(new Rect(330, 200, 100, 18), "GitHub:", TextStyle(10, TextAnchor.MiddleRight));
            if (GUI.Button(new Rect(440, 200, 150, 18), "UnityWebSocket"))
            {
                Application.OpenURL(Settings.GITHUB);
            }

            GUI.Label(new Rect(330, 225, 100, 18), "Report:", TextStyle(10, TextAnchor.MiddleRight));
            if (GUI.Button(new Rect(440, 225, 150, 18), "Report an Issue"))
            {
                Application.OpenURL(Settings.GITHUB + "/issues/new");
            }

            GUI.Label(new Rect(330, 250, 100, 18), "Email:", TextStyle(10, TextAnchor.MiddleRight));
            if (GUI.Button(new Rect(440, 250, 150, 18), Settings.EMAIL))
            {
                var uri = new System.Uri(string.Format("mailto:{0}?subject={1}", Settings.EMAIL, "UnityWebSocket Feedback"));
                Application.OpenURL(uri.AbsoluteUri);
            }

            GUI.Label(new Rect(330, 275, 100, 18), "QQ群:", TextStyle(10, TextAnchor.MiddleRight));
            if (GUI.Button(new Rect(440, 275, 150, 18), Settings.QQ_GROUP))
            {
                Application.OpenURL(Settings.QQ_GROUP_LINK);
            }
        }

        private void DrawFooter()
        {
            GUI.Label(new Rect(10, 230, 400, 10), "Developed by " + Settings.AUHTOR, TextStyle(alignment: TextAnchor.MiddleCenter));
            GUI.Label(new Rect(10, 250, 400, 10), "All rights reserved", TextStyle(alignment: TextAnchor.MiddleCenter));
        }

        UnityWebRequest req;
        string changeLog = "";
        string latestVersion = "";
        void BeginCheck()
        {
            req = UnityWebRequest.Get(Settings.GITHUB + "/releases/latest");
            EditorApplication.update -= VersionCheckUpdate;
            EditorApplication.update += VersionCheckUpdate;
            req.SendWebRequest();
        }
        private void VersionCheckUpdate()
        {
            if (req == null || req.isNetworkError || req.isHttpError)
            {
                EditorApplication.update -= VersionCheckUpdate;
                return;
            }

            if (req.isDone)
            {
                EditorApplication.update -= VersionCheckUpdate;
                latestVersion = req.url.Substring(req.url.LastIndexOf("/") + 1);

                if (Settings.VERSION != latestVersion)
                {
                    var text = req.downloadHandler.text;
                    var st = text.IndexOf("content=\"" + latestVersion);
                    st = st > 0 ? text.IndexOf("\n", st) : -1;
                    var end = st > 0 ? text.IndexOf("\" />", st) : -1;
                    if (st > 0 && end > st)
                    {
                        changeLog = text.Substring(st + 1, end - st - 1).Trim();
                        changeLog = changeLog.Replace("\r", "");
                        changeLog = changeLog.Replace("\n", "\n- ");
                        changeLog = "\nCHANGE LOG: \n- " + changeLog + "\n";
                    }
                }

                Repaint();
            }
        }
    }


    public static class PlayerSettingsChecker
    {
        [InitializeOnLoadMethod]
        public static void OnInit()
        {
            bool isLinkTargetFixed;
            bool isMemorySizeFixed;
            bool isDecompressionFallbackFixed;
            bool isRuntimeVersionFixed;
            GetSettingsFixed(out isRuntimeVersionFixed, out isLinkTargetFixed, out isMemorySizeFixed, out isDecompressionFallbackFixed);
            bool isAllFixed = isRuntimeVersionFixed && isLinkTargetFixed && isMemorySizeFixed && isDecompressionFallbackFixed;

            if (!isAllFixed)
            {
                SettingsWindow.Open();
            }
        }

        internal static void GetSettingsFixed(out bool isRuntimeVersionFixed, out bool isLinkTargetFixed, out bool isMemorySizeFixed, out bool isDecompressionFallbackFixed)
        {
            isRuntimeVersionFixed = true;
            isLinkTargetFixed = true;
            isMemorySizeFixed = true;
            isDecompressionFallbackFixed = true;

#if UNITY_2018_1_OR_NEWER
            isLinkTargetFixed = PlayerSettings.WebGL.linkerTarget == (WebGLLinkerTarget)2;
#endif

#if !UNITY_2019_3_OR_NEWER
            isRuntimeVersionFixed = PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Latest;
#endif

#if UNITY_2019_1_OR_NEWER
            isMemorySizeFixed = PlayerSettings.WebGL.memorySize >= 16;
#endif

#if UNITY_2020_1_OR_NEWER
            isDecompressionFallbackFixed = PlayerSettings.WebGL.decompressionFallback;
#endif
        }

    }
}