using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityWebSocket;

namespace UnityWebSocket.Editor
{
    public class VersionChecker
    {
#if UNITY_2018_1_OR_NEWER
        static UnityWebRequest req;
        static bool forceCheck = false;

        [InitializeOnLoadMethod]
        public static void OnInit()
        {
            forceCheck = false;
            BeginCheck();
        }

        private static void Update()
        {
            if (req == null || req.isNetworkError || req.isHttpError)
            {
                EditorApplication.update -= Update;
                return;
            }

            if (req.isDone)
            {
                EditorApplication.update -= Update;
                var latestVersion = req.url.Substring(req.url.LastIndexOf("/v") + 2);
                var vKey = "UnityWebSocket_Version_Skip_v" + latestVersion;
                if (!forceCheck && EditorPrefs.GetBool(vKey, false))
                    return;

                if (EditorPrefs.HasKey(vKey))
                {
                    EditorPrefs.DeleteKey(vKey);
                }

                if (Settings.VERSION != latestVersion)
                {
                    var text = req.downloadHandler.text;
                    var st = text.IndexOf("content=\"v" + latestVersion);
                    st = st > 0 ? text.IndexOf("\n", st) : -1;
                    var end = st > 0 ? text.IndexOf("\" />", st) : -1;
                    var changeLog = "";
                    if (st > 0 && end > st)
                    {
                        changeLog = text.Substring(st + 1, end - st - 1).Trim();
                        changeLog = changeLog.Replace("\r", "");
                        changeLog = changeLog.Replace("\n", "\n- ");
                        changeLog = "\nCHANGE LOG: \n- " + changeLog + "\n";
                    }

                    var code = EditorUtility.DisplayDialogComplex("UnityWebSocket"
                        , "UnityWebSocket new version found v" + latestVersion
                        + ", your current version is v" + Settings.VERSION + ".\n"
                        + "Upgrade UnityWebSocket now?\n"
                        + changeLog,
                        "Upgrade Now", "Remind Me Later", "Skip this Version");

                    if (code == 0)
                    {
                        Application.OpenURL(Settings.GITHUB + "/releases");
                    }
                    else if (code == 2)
                    {
                        EditorPrefs.SetBool(vKey, true);
                    }
                }
                else if (forceCheck)
                {
                    EditorUtility.DisplayDialog("UnityWebSocket", "Your current version v" + Settings.VERSION + " is the Latest version.", "OK");
                }
            }
        }

        [MenuItem("Tools/UnityWebSocket/Check Updates", priority = 10)]
        static void CheckUpdates()
        {
            forceCheck = true;
            BeginCheck();
        }

        static void BeginCheck()
        {
            req = UnityWebRequest.Get(Settings.GITHUB + "/releases/latest");

            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            req.SendWebRequest();
        }
#endif
    }
}