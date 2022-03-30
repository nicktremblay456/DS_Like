using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(UIWindow), true)]
[CanEditMultipleObjects]
public class UIWindowEditor : Editor
{
    private Type[] m_InputHandlers = new Type[0];

    public override void OnInspectorGUI ()
    {
        UIWindow t = (UIWindow)target;
        base.OnInspectorGUI();

        if (t.IsVisible)
        {
            EditorGUILayout.LabelField("Window is Visible");
        }
        else
        {
            EditorGUILayout.LabelField("Window is Hidden");
        }

        if (t.gameObject.GetComponent<IUIWindowInputHandler>() != null)
        {
            EditorGUILayout.HelpBox("No input handler found", MessageType.Warning);
            foreach (Type type in m_InputHandlers)
            {
                Type tempType = type;
                if (GUILayout.Button("Add: " + tempType.Name))
                {
                    t.gameObject.AddComponent(tempType);
                }
            }
        }
    }
}