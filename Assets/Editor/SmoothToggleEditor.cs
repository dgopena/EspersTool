using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(SmoothToggle))]
public class SmoothToggleEditor : Editor
{
    public SerializedProperty
        interactable_Prop,
        startWithDefaultValue_Prop,
        startValue_Prop,
        toggleSpeed_Prop,
        circleRT_Prop,
        knobRT_Prop,
        togglerRT_Prop,
        useChangeableLabel_Prop,
        toggleLabel_Prop,
        onValue_Prop,
        offValue_Prop,
        OnValueChange_Prop;

    private void OnEnable()
    {
        interactable_Prop = serializedObject.FindProperty("interactable");
        startWithDefaultValue_Prop = serializedObject.FindProperty("startWithDefaultValue");
        startValue_Prop = serializedObject.FindProperty("startValue");
        toggleSpeed_Prop = serializedObject.FindProperty("toggleSpeed");
        circleRT_Prop = serializedObject.FindProperty("circleRT");
        knobRT_Prop = serializedObject.FindProperty("knobRT");
        togglerRT_Prop = serializedObject.FindProperty("togglerRT");
        useChangeableLabel_Prop = serializedObject.FindProperty("useChangeableLabel");
        toggleLabel_Prop = serializedObject.FindProperty("toggleLabel");
        onValue_Prop = serializedObject.FindProperty("onValue");
        offValue_Prop = serializedObject.FindProperty("offValue");
        OnValueChange_Prop = serializedObject.FindProperty("OnValueChange");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(interactable_Prop, new GUIContent("Interactable"));

        EditorGUILayout.PropertyField(startWithDefaultValue_Prop, new GUIContent("Start with Set Value?"));
        if (startWithDefaultValue_Prop.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(startValue_Prop, new GUIContent("Starting Value"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(toggleSpeed_Prop, new GUIContent("Toggle Change Time"));

        EditorGUILayout.PropertyField(circleRT_Prop, new GUIContent("Circle RT"));
        EditorGUILayout.PropertyField(knobRT_Prop, new GUIContent("Knob RT"));
        EditorGUILayout.PropertyField(togglerRT_Prop, new GUIContent("Toggler RT"));

        EditorGUILayout.PropertyField(useChangeableLabel_Prop, new GUIContent("Use Toggle Label"));

        if (useChangeableLabel_Prop.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(toggleLabel_Prop, new GUIContent("Toggeable Label"));
            EditorGUILayout.PropertyField(onValue_Prop, new GUIContent("Text Value for TRUE"));
            EditorGUILayout.PropertyField(offValue_Prop, new GUIContent("Text Value for FALSE"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(OnValueChange_Prop, new GUIContent("On Value Changed"));

        serializedObject.ApplyModifiedProperties();
    }
}
