using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class CustomHierarchy
{
    static readonly Color darkGrey = new Color(0.3f, 0.3f, 0.3f);

    static CustomHierarchy()
    {
        EditorApplication.hierarchyWindowItemOnGUI = DrawItem;
    }

    static void DrawItem(int instanceID, Rect rect)
    {
        // Get's object for given item
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go != null)
        {
            // Get's style of toggle

            // Sets rect for toggle
            var toggleRect = new Rect(rect);
            toggleRect.width = toggleRect.height;
            toggleRect.x -= 28;

            var oldCol = GUI.color;
            if (go.name.StartsWith("=="))
            {
                var boxRect = new Rect(rect);
                boxRect.height += 2;
                boxRect.y -= 1;
                
                GUI.color = darkGrey;
                GUI.Box(boxRect, go.name.Substring(2));
            }
            GUI.color = oldCol;

            GUIStyle toggleStyle = "OL Toggle";
            // Creates toggle
            bool state = GUI.Toggle(toggleRect, go.activeSelf, GUIContent.none, toggleStyle);
            
            // Sets game's active state to result of toggle
            if (state != go.activeSelf)
            {
                go.SetActive(state);
            }
        }
    }
}