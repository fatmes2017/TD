using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileSlot)), CanEditMultipleObjects]
public class TileSlotEditor : Editor
{ 
    private GUIStyle centeredStyle;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        centeredStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 16,

        };


        float oneButtonWidth = (EditorGUIUtility.currentViewWidth - 25) ;
        float twoButtonWidth = (EditorGUIUtility.currentViewWidth - 25) / 2;
        float threeButtonWidth = (EditorGUIUtility.currentViewWidth - 25) / 3;




        GUILayout.Label("Position and Rotation", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Rotate Left", GUILayout.Width(twoButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).RotateTile(-1);


            }
        }
        if (GUILayout.Button("Rotate Right", GUILayout.Width(twoButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).RotateTile(1);
            }
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();

        if (GUILayout.Button("- 0.1f on the Y", GUILayout.Width(twoButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).AdjustY(-1);


            }
        }
        if (GUILayout.Button("0.1f on the Y", GUILayout.Width(twoButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).AdjustY(1);
            }
        }
        GUILayout.EndHorizontal();












        GUILayout.Label("Tile option", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Road", GUILayout.Width(twoButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileRoad);


            }
        }
        if (GUILayout.Button("Field",GUILayout.Width(twoButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileField);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Sideway", GUILayout.Width(oneButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileSideway);
            }
        }


        GUILayout.EndHorizontal();







        GUILayout.Label("Corner option", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Inner Corner", GUILayout.Width(twoButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileInnerCorner);


            }
        }
        if (GUILayout.Button("Outer Corner", GUILayout.Width(twoButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileOuterCorner);
            }
        }
        GUILayout.EndHorizontal();


        GUILayout.Label("Bridge and hills option", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Hill1", GUILayout.Width(threeButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileHill1);


            }
        }
        if (GUILayout.Button("Hill2", GUILayout.Width(threeButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileHill2);
            }
        }
        if (GUILayout.Button("Hill3", GUILayout.Width(threeButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileHill3);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Bridge with Road", GUILayout.Width(threeButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileBridgeRoad);


            }
        }
        if (GUILayout.Button("Bridge with Field", GUILayout.Width(threeButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileBridgeField);
            }
        }
        if (GUILayout.Button("Bridge with Sideway", GUILayout.Width(threeButtonWidth)))
        {
            TileSetHolder tileSetHolder = FindObjectOfType<TileSetHolder>();
            if (tileSetHolder != null)
            {
                foreach (var targetTile in targets)

                    ((TileSlot)targetTile).SwitchTile(tileSetHolder.tileBridgeSideway);
            }
        }
        GUILayout.EndHorizontal();





    }
}
