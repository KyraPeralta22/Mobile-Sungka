using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

    /*
    Configuration Editor
    */
    [CustomEditor(typeof(ConfigurationManager))]
    public class ConfigurationManagerEditor : Editor
    {

        void OnEnable()
        {
            ConfigurationManager configuration = (ConfigurationManager)target;

            // Make sure the GameMode matches the object
            configuration.aiConfiguration.gameMode = GameConfiguration.GameMode.AGAINST_AI;
            configuration.offlineConfiguration.gameMode = GameConfiguration.GameMode.OFFLINE;
        }

        public override void OnInspectorGUI()
        {
            //serializedObject.Update();

            EditorGUILayout.Space();

            SerializedProperty offlineGameProp = serializedObject.FindProperty("offlineConfiguration");
            SerializedProperty aiGameProp = serializedObject.FindProperty("aiConfiguration");

            if (EditorTools.DrawHeader("Offline Game Configuration"))
            {
                DrawGameConfiguration(offlineGameProp);
            }

            if (EditorTools.DrawHeader("AI Game Configuration"))
            {
                DrawGameConfiguration(aiGameProp);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawList(string propName, SerializedProperty prop, bool hasId, ConfigurationElementType configurationType)
        {
            if (EditorTools.DrawHeader(propName))
            {
                EditorTools.BeginContents();
                for (int i = 0; i < prop.arraySize; i++)
                {
                    SerializedProperty objProp = prop.GetArrayElementAtIndex(i);
                    string objName = objProp.FindPropertyRelative("name").stringValue;

                    string header = objName;

                    if (hasId)
                    {
                        int objId = objProp.FindPropertyRelative("id").intValue;
                        header = "[" + objId.ToString() + "] " + header;
                    }

                    if (EditorTools.DrawHeader(header))
                    {
                        EditorTools.BeginContents();
                        switch (configurationType)
                        {
                            case ConfigurationElementType.GAME:
                                DrawGameConfiguration(objProp);
                                break;
                        }
                        EditorTools.EndContents();
                    }
                    if (GUILayout.Button("Remove " + propName))
                    {
                        prop.DeleteArrayElementAtIndex(i);
                    }
                }

                if (GUILayout.Button("Add " + propName))
                {
                    prop.InsertArrayElementAtIndex(prop.arraySize);
                    if (hasId)
                    {
                        SerializedProperty lastObj = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                        lastObj.FindPropertyRelative("id").intValue++;
                    }
                }
                EditorTools.EndContents();
            }
        }

        private void DrawGameConfiguration(SerializedProperty prop)
        {
            EditorTools.BeginContents();

            SerializedProperty turnProp = prop.FindPropertyRelative("turnTime");
            SerializedProperty sceneNameProp = prop.FindPropertyRelative("sceneName");
            SerializedProperty boardPrefabProp = prop.FindPropertyRelative("boardPrefab");
            SerializedProperty stonesPrefabProp = prop.FindPropertyRelative("stonesPrefab");

            EditorGUILayout.PropertyField(turnProp, new GUIContent("Turn Time"));

            EditorGUILayout.PropertyField(sceneNameProp, new GUIContent("Scene Name"));

            EditorGUILayout.PropertyField(boardPrefabProp, new GUIContent("Board Prefab"));

            if (EditorTools.DrawHeader("Stones"))
            {
                for (int i = 0; i < stonesPrefabProp.arraySize; i++)
                {
                    SerializedProperty objProp = stonesPrefabProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(objProp, new GUIContent("Stone"));

                    if (GUILayout.Button("Remove Stone"))
                    {
                        stonesPrefabProp.DeleteArrayElementAtIndex(i);
                    }
                }

                if (GUILayout.Button("Add Stone"))
                {
                    stonesPrefabProp.InsertArrayElementAtIndex(stonesPrefabProp.arraySize);
                }
            }

            EditorTools.EndContents();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

    }
