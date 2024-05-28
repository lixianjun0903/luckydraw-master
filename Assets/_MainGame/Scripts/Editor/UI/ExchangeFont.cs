
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;


namespace Pillow.Ediotr
{

    public class ExchangeFont : EditorWindow
    {
        [MenuItem("Tools/UI/更换字体")]
        public static void Open()
        {
            EditorWindow.GetWindow(typeof(ExchangeFont), true);
        }

        static bool isSelectOldFont = false;
        static Font oldTextFont;
        static TMP_FontAsset oldTextMeshProFont;

        static Font selectTextFont;
        static TMP_FontAsset selectTextMeshProFont;


        static bool isChangeLineSpacing = false;

        private static float NewLineSpacing = 1;

        private static string selectedFolderPath;




        private void OnGUI()
        {

            GUILayout.Space(10);

            isSelectOldFont = EditorGUILayout.Toggle("是否需要选中被替换的字体：", isSelectOldFont);

            EditorGUI.BeginDisabledGroup(!isSelectOldFont);
            oldTextFont = (Font)EditorGUILayout.ObjectField("选择被更换的Text字体", oldTextFont, typeof(Font), true);
            GUILayout.Space(5);
            oldTextMeshProFont = (TMP_FontAsset)EditorGUILayout.ObjectField("选择被更换的TextMesh字体", oldTextMeshProFont, typeof(TMP_FontAsset), true);
            GUILayout.Space(5);
            EditorGUI.EndDisabledGroup();


            GUILayout.Space(5);
            isChangeLineSpacing = EditorGUILayout.Toggle("是否修改行间距：", isChangeLineSpacing);

            EditorGUI.BeginDisabledGroup(!isChangeLineSpacing);

            NewLineSpacing = EditorGUILayout.FloatField("新行间距", NewLineSpacing);

            EditorGUI.EndDisabledGroup();






            GUILayout.Space(10);
            selectTextFont = (Font)EditorGUILayout.ObjectField("选择新的Text字体", selectTextFont, typeof(Font), true, GUILayout.MinWidth(100));
            GUILayout.Space(5);
            selectTextMeshProFont = (TMP_FontAsset)EditorGUILayout.ObjectField("选择新的TextMesh字体", selectTextMeshProFont, typeof(TMP_FontAsset), true, GUILayout.MinWidth(100));
            GUILayout.Space(5);

            GUILayout.Space(20);
            if (GUILayout.Button("更换选中的预制体", GUILayout.MaxHeight(30)))
            {
                if (selectTextMeshProFont == null && selectTextFont == null)
                {
                    Debug.LogError("请选择字体！");
                    return;
                }
                if (Selection.gameObjects == null && Selection.gameObjects.Length < 1)
                {
                    Debug.LogError("未选中预制体！");
                    return;
                }

                var selectedObjects = new List<GameObject>();

                selectedObjects.AddRange(Selection.gameObjects);
                ChangeFont(selectedObjects);
            }




            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            EditorGUILayout.TextField("选择的修改路径：", selectedFolderPath);

            if (GUILayout.Button("选中路径", GUILayout.MaxWidth(80)))
            {
                if (Selection.assetGUIDs.Length > 0)
                {
                    string guid = Selection.assetGUIDs[0];
                    selectedFolderPath = AssetDatabase.GUIDToAssetPath(guid);
                }
                else
                {
                    Debug.LogError("没有选中文件！");
                }

            }
            if (GUILayout.Button("选择路径", GUILayout.MaxWidth(80)))
            {
                var paht = EditorUtility.OpenFolderPanel("选择路径", Application.dataPath, "");
                selectedFolderPath = paht.Replace(Application.dataPath, "Assets");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (GUILayout.Button("更换选中路径下所有的预制体", GUILayout.MaxHeight(30)))
            {
                if (selectTextMeshProFont == null && selectTextFont == null)
                {
                    Debug.LogError("请选择字体！");
                    return;
                }
                ChangeSelectFloud();
            }
        }

        public static void ChangeSelectFloud()
        {

            var selectedObjects = new List<GameObject>();

            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { selectedFolderPath });
                foreach (string guid in prefabGUIDs)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                    if (prefab != null)
                    {
                        selectedObjects.Add(prefab);
                    }
                }
            }

            ChangeFont(selectedObjects);
        }














        public static void ChangeFont(List<GameObject> targetkLists)
        {
            if (targetkLists == null || targetkLists.Count < 1)
            {
                Debug.LogError("可修改预制体数量为0！");
                return;
            }

            int maxCount = targetkLists.Count;

            int curCount = 0;



            for (int i = 0; i < targetkLists.Count; i++)
            {
                EditorUtility.DisplayProgressBar("开始替换字体", "Show a progress bar for the given seconds", curCount / maxCount);

                var Texts = targetkLists[i].GetComponentsInChildren<Text>(true);
                ChangeText(Texts);

                var tmpTexts = targetkLists[i].GetComponentsInChildren<TMP_Text>(true);
                ChangeTextMeshPro(tmpTexts);

                curCount++;
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
        }







        static int ChangeText(Text[] lists)
        {

            int count = 0;
            foreach (Text text in lists)
            {
                Undo.RecordObject(text, text.gameObject.name);
                if (isSelectOldFont)
                {
                    if (text.font != null && text.font != oldTextFont)
                    {
                        continue;
                    }
                }

                if (selectTextFont != null)
                    text.font = selectTextFont;
                if (isChangeLineSpacing)
                    text.lineSpacing = NewLineSpacing;
                EditorUtility.SetDirty(text);
                count++;
            }
            return count; ;
        }


        static int ChangeTextMeshPro(TMP_Text[] lists)
        {
            int count = 0;
            foreach (TMP_Text text in lists)
            {
                Undo.RecordObject(text, text.gameObject.name);

                if (isSelectOldFont)
                {
                    if (text.font != null && text.font != oldTextMeshProFont)
                    {
                        continue;
                    }
                }

                if (selectTextMeshProFont != null)
                    text.font = selectTextMeshProFont;
                if (isChangeLineSpacing)
                    text.lineSpacing = NewLineSpacing;
                EditorUtility.SetDirty(text);
                count++;

            }
            return count; ;
        }

    }
}