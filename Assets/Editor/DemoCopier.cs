using UnityEngine;
using UnityEditor;

public class DemoCopier : EditorWindow
{
    private static GameObject _parentObject;

    [MenuItem("Tools/Demo Copier/Set Parent &%#q")]
    private static void SetParent()
    {
        Debug.Log("run");
        if (Selection.activeGameObject != null)
        {
            _parentObject = Selection.activeGameObject;
            Debug.Log($"Parent object set to: {_parentObject.name}");
        }
        else
        {
            Debug.LogWarning("No object selected to set as parent");
        }
    }

    [MenuItem("Tools/Demo Copier/Duplicate as Child &%#c")]
    private static void DuplicateAsChild()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("No object selected to duplicate");
            return;
        }

        if (_parentObject == null)
        {
            Debug.LogWarning("No parent object set. Use Ctrl+Shift+Alt+Q to set a parent first");
            return;
        }

        GameObject selectedObject = Selection.activeGameObject;
        GameObject duplicate;
        
        // Check if the selected object is a prefab instance
        GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(selectedObject);
        if (prefabAsset != null)
        {
            // Instantiate from the original prefab asset to maintain prefab connection
            duplicate = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, _parentObject.transform);
        }
        else
        {
            // Regular instantiate for non-prefab objects
            duplicate = Instantiate(selectedObject, _parentObject.transform);
        }
        
        duplicate.name = selectedObject.name + " (Copy)";
        
        Selection.activeGameObject = duplicate;
        EditorGUIUtility.PingObject(duplicate);
        
        Debug.Log($"Duplicated {selectedObject.name} as child of {_parentObject.name}");
    }

    [MenuItem("Tools/Demo Copier/Set Parent %&q", true)]
    private static bool ValidateSetParent()
    {
        return Selection.activeGameObject != null;
    }

    [MenuItem("Tools/Demo Copier/Duplicate as Child %&c", true)]
    private static bool ValidateDuplicateAsChild()
    {
        return Selection.activeGameObject != null && _parentObject != null;
    }
}