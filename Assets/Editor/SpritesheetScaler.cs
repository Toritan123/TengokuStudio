using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SpritesheetScaler : EditorWindow
{

    Object source;
    float multiplier = 1;
    int inflateX = 0;
    int inflateY = 0;

    // Creates a new option in "Windows"
    [MenuItem("Window/Scale spritesheet pivots and slices")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SpritesheetScaler window = (SpritesheetScaler)EditorWindow.GetWindow(typeof(SpritesheetScaler));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Source texture:", EditorStyles.boldLabel);
        source = EditorGUILayout.ObjectField(source, typeof(Texture2D), false, GUILayout.Width(220));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Multiplier:", EditorStyles.boldLabel);
        multiplier = EditorGUILayout.FloatField(multiplier, GUILayout.Width(220));
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Inflate Quads:", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("X", EditorStyles.label);
        inflateX = EditorGUILayout.IntField(inflateX, GUILayout.Width(220/2));
        GUILayout.Label("Y", EditorStyles.label);
        inflateY = EditorGUILayout.IntField(inflateY, GUILayout.Width(220/2));
        GUILayout.EndHorizontal();

        GUILayout.Space(25f);
        if (GUILayout.Button("Scale pivots and slices"))
        {
            ScalePivotsAndSlices();
        }
    }

    void ScalePivotsAndSlices()
    {
        if (!source || (multiplier <= 0))
        {
            Debug.Log("Missing one object");
            return;
        }

        if (source.GetType() != typeof(Texture2D))
        {
            Debug.Log(source + "needs to be Texture2D!");
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(source);
        TextureImporter ti1 = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
        bool wasReadable = ti1.isReadable;
        ti1.isReadable = true;

        ti1.spritePixelsPerUnit = (ti1.spritePixelsPerUnit * multiplier);

        SerializedObject so = new SerializedObject(ti1);
        SerializedProperty spritesProp = so.FindProperty("m_SpriteSheet.m_Sprites");

        if (spritesProp == null)
        {
            Debug.LogWarning("Could not find spritesheet serialized data.");
        }
        else
        {
            Debug.Log("Amount of slices found: " + spritesProp.arraySize);

            for (int i = 0; i < spritesProp.arraySize; i++)
            {
                SerializedProperty sp = spritesProp.GetArrayElementAtIndex(i);
                SerializedProperty rectProp = sp.FindPropertyRelative("m_Rect");
                SerializedProperty pivotProp = sp.FindPropertyRelative("m_Pivot");
                SerializedProperty alignProp = sp.FindPropertyRelative("m_Alignment");
                SerializedProperty borderProp = sp.FindPropertyRelative("m_Border");

                float rx = rectProp.FindPropertyRelative("x").floatValue;
                float ry = rectProp.FindPropertyRelative("y").floatValue;
                float rw = rectProp.FindPropertyRelative("width").floatValue;
                float rh = rectProp.FindPropertyRelative("height").floatValue;
                Rect rect = new Rect(rx, ry, rw, rh);

                // pivot relative to the origin of the rect in pixels
                Vector2 oldPivot = new Vector2(pivotProp.FindPropertyRelative("x").floatValue * rect.width,
                                               pivotProp.FindPropertyRelative("y").floatValue * rect.height);

                Rect newRect = ScaleRect(rect, multiplier, inflateX, inflateY);

                float bx = borderProp.FindPropertyRelative("x").floatValue;
                float by = borderProp.FindPropertyRelative("y").floatValue;
                float bz = borderProp.FindPropertyRelative("z").floatValue;
                float bw = borderProp.FindPropertyRelative("w").floatValue;

                bx += bx > 0 ? inflateX : 0;
                by += by > 0 ? inflateY : 0;
                bz += bz > 0 ? inflateX : 0;
                bw += bw > 0 ? inflateY : 0;

                if (alignProp.intValue != (int)SpriteAlignment.Center && (inflateX > 0 || inflateY > 0))
                {
                    alignProp.intValue = (int)SpriteAlignment.Custom;
                    oldPivot += new Vector2(inflateX, inflateY);
                }

                Vector2 newPivot = oldPivot * multiplier;
                pivotProp.FindPropertyRelative("x").floatValue = newPivot.x / newRect.width;
                pivotProp.FindPropertyRelative("y").floatValue = newPivot.y / newRect.height;

                borderProp.FindPropertyRelative("x").floatValue = bx * multiplier;
                borderProp.FindPropertyRelative("y").floatValue = by * multiplier;
                borderProp.FindPropertyRelative("z").floatValue = bz * multiplier;
                borderProp.FindPropertyRelative("w").floatValue = bw * multiplier;

                rectProp.FindPropertyRelative("x").floatValue = newRect.x;
                rectProp.FindPropertyRelative("y").floatValue = newRect.y;
                rectProp.FindPropertyRelative("width").floatValue = newRect.width;
                rectProp.FindPropertyRelative("height").floatValue = newRect.height;
            }

            so.ApplyModifiedProperties();
        }

        ti1.isReadable = wasReadable;

        AssetDatabase.ImportAsset(sourcePath, ImportAssetOptions.ForceUpdate);
    }

    Rect ScaleRect(Rect source, float mult, int inflateX, int inflateY)
    {
        var newRect = new Rect();
        newRect.Set((source.x - inflateX) * mult, (source.y - inflateY) * mult, (source.width + inflateX*2) * mult, (source.height + inflateY*2) * mult);
        return newRect;
    }
}
