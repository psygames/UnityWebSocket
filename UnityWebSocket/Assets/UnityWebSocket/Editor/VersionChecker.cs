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
        static bool checkSkip = true;

        [InitializeOnLoadMethod]
        public static void OnInit()
        {
            checkSkip = true;
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
                if (checkSkip && EditorPrefs.GetBool(vKey, false))
                    return;

                if (EditorPrefs.HasKey(vKey))
                {
                    EditorPrefs.DeleteKey(vKey);
                }

                if (Settings.VERSION != latestVersion)
                {
                    if (EditorUtility.DisplayDialog("UnityWebSocket"
                        , "UnityWebSocket new version found v" + latestVersion
                        + ", your current version is v" + Settings.VERSION + ".\n"
                        + "Upgrade UnityWebSocket now?",
                        "Upgrade Now", "Skip this Version"))
                    {
                        Application.OpenURL(Settings.GITHUB + "/releases");
                    }
                    else
                    {
                        EditorPrefs.SetBool(vKey, true);
                    }
                }
            }
        }

        [MenuItem("Tools/UnityWebSocket/Check Updates")]
        static void CheckUpdates()
        {
            checkSkip = false;
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