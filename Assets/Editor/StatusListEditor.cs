using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(StatusList))]
public class StatusListEditor : Editor
{
    public SerializedProperty
        type_Prop,
        contentDescriptionRect_Prop,
        titleOnly_Prop,
        iconSeparation_Prop,
        iconColor_Prop,
        maximumIcons_Prop,
        listPanel_Prop,
        dropDownListProportions_Prop,
        dropDownListPosCorrection_Prop,
        OnListChange_Prop;

    private void OnEnable()
    {
        type_Prop = serializedObject.FindProperty("type");
        contentDescriptionRect_Prop = serializedObject.FindProperty("contentDescriptionRect");
        titleOnly_Prop = serializedObject.FindProperty("titleOnly");
        iconSeparation_Prop = serializedObject.FindProperty("iconSeparation");
        iconColor_Prop = serializedObject.FindProperty("iconColor");
        maximumIcons_Prop = serializedObject.FindProperty("maximumIcons");
        listPanel_Prop = serializedObject.FindProperty("listPanel");
        dropDownListProportions_Prop = serializedObject.FindProperty("dropDownListProportions");
        dropDownListPosCorrection_Prop = serializedObject.FindProperty("dropDownListPosCorrection");
        OnListChange_Prop = serializedObject.FindProperty("OnListChange");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(type_Prop, new GUIContent("Type of Icon List"));

        EditorGUILayout.PropertyField(contentDescriptionRect_Prop, new GUIContent("Description Container"));
        EditorGUILayout.PropertyField(titleOnly_Prop, new GUIContent("Show Only Title"));

        EditorGUILayout.PropertyField(iconSeparation_Prop, new GUIContent("Icon Separation"));

        EditorGUILayout.PropertyField(iconColor_Prop, new GUIContent("Icon Color"));

        EditorGUILayout.PropertyField(maximumIcons_Prop, new GUIContent("Max Icons in Row"));

        EditorGUILayout.PropertyField(listPanel_Prop, new GUIContent("List Panel"));
        EditorGUILayout.PropertyField(dropDownListProportions_Prop, new GUIContent("Drop Down List Screen Proportions"));
        EditorGUILayout.PropertyField(dropDownListPosCorrection_Prop, new GUIContent("Drop Down List Position Corrrection"));

        EditorGUILayout.PropertyField(OnListChange_Prop, new GUIContent("On List Change Event"));

        serializedObject.ApplyModifiedProperties();
    }
}
