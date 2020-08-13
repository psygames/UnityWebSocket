using UnityEditor;
using UnityEngine;

namespace UnityWebSocket.Editor
{
    public class About
    {
        [MenuItem("Tools/UnityWebSocket/Help/Home Page")]
        public static void HelpHomePage()
        {
            Application.OpenURL(Settings.GITHUB);
        }

        [MenuItem("Tools/UnityWebSocket/Help/Report an Issue")]
        public static void HelpReportIssue()
        {
            Application.OpenURL(Settings.GITHUB + "/issues/new");
        }

        [MenuItem("Tools/UnityWebSocket/Help/Feedback")]
        public static void HelpContact()
        {
            var uri = new System.Uri(string.Format("mailto:{0}?subject={1}", Settings.EMAIL, "UnityWebSocket Feedback"));
            Application.OpenURL(uri.AbsoluteUri);
        }

        [MenuItem("Tools/UnityWebSocket/Help/QQ群")]
        public static void HelpContactQQ()
        {
            Application.OpenURL(Settings.QQ);
        }

        [MenuItem("Tools/UnityWebSocket/About")]
        public static void AboutDialog()
        {
            var title = "UnityWebSocket";
            var content = "\n"
                + $"Version: {Settings.VERSION}\n\n"
                + $"Author: {Settings.AUHTOR}\n\n"
                + $"E-mail: {Settings.EMAIL}\n\n"
                + $"All rights reserved";
            EditorUtility.DisplayDialog(title, content, "OK");
        }
    }
}