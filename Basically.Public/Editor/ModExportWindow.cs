using UnityEditor;
using UnityEngine;

namespace Basically.Modding {
    public class ModExportWindow : EditorWindow {
        GUIStyle center;

        [MenuItem("Basically/Modding/Export")]
        static void Init() {
            GetWindow(typeof(ModExportWindow), false, "Mod Exporter").Show();
        }

        private void OnGUI() {
            if (center == null) {
                center = new GUIStyle(GUI.skin.label) {
                    alignment = TextAnchor.MiddleCenter
                };
                center.normal.textColor = Color.white;
            }

            var widthFifth = position.width / 5;
            var fillSize = new Vector2(widthFifth, position.height);

            GUILayout.BeginArea(new Rect(Vector2.zero, fillSize), BackgroundStyle.Get(Color.black));
            EditorGUILayout.LabelField("Basically Mod Export Tool", center);
            GUILayout.EndArea();

            // controls
            GUILayout.BeginArea(new Rect(new Vector2(widthFifth, 0), new Vector2(position.width - widthFifth, position.height)));

            EditorGUILayout.LabelField("trolling", center);

            GUILayout.EndArea();
        }
    }

    public static class BackgroundStyle {
        private static GUIStyle style = new GUIStyle();
        private static Texture2D texture = new Texture2D(1, 1);

        public static GUIStyle Get(Color color) {
            texture.SetPixel(0, 0, color);
            texture.Apply();
            style.normal.background = texture;
            return style;
        }
    }
}
