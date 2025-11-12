using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnusedAssetCleaner : EditorWindow
{
    private DefaultAsset _targetFolder;
    private List<SceneAsset> _sceneAssets = new List<SceneAsset>();
    private Vector2 _scrollPosition;
    private bool _showPreview = false;
    private List<string> _unusedAssets = new List<string>();
    private bool _isAnalyzing = false;
    private float _progress = 0f;
    private string _currentOperation = "";

    [MenuItem("Tools/Unused Asset Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<UnusedAssetCleaner>("Unused Asset Cleaner");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Unused Asset Cleaner", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Target folder selection
        EditorGUILayout.LabelField("Target Folder:", EditorStyles.label);
        _targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(_targetFolder, typeof(DefaultAsset), false);
        
        if (_targetFolder != null && !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(_targetFolder)))
        {
            EditorGUILayout.HelpBox("Please select a valid folder.", MessageType.Warning);
            _targetFolder = null;
        }

        EditorGUILayout.Space();

        // Scene list
        EditorGUILayout.LabelField("Scenes to Check:", EditorStyles.label);
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));
        
        for (int i = 0; i < _sceneAssets.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _sceneAssets[i] = (SceneAsset)EditorGUILayout.ObjectField(_sceneAssets[i], typeof(SceneAsset), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                _sceneAssets.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Scene"))
        {
            _sceneAssets.Add(null);
        }

        EditorGUILayout.Space();

        // Progress bar
        if (_isAnalyzing)
        {
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), _progress, _currentOperation);
            EditorGUILayout.Space();
        }

        // Buttons
        EditorGUI.BeginDisabledGroup(_targetFolder == null || _sceneAssets.Count == 0 || _sceneAssets.Any(s => s == null) || _isAnalyzing);
        
        if (GUILayout.Button("Analyze Unused Assets"))
        {
            AnalyzeUnusedAssets();
        }

        EditorGUI.EndDisabledGroup();

        // Preview section
        if (_unusedAssets.Count > 0 && !_isAnalyzing)
        {
            EditorGUILayout.Space();
            _showPreview = EditorGUILayout.Foldout(_showPreview, $"Unused Assets Found ({_unusedAssets.Count})");
            
            if (_showPreview)
            {
                EditorGUILayout.BeginVertical("box");
                foreach (string assetPath in _unusedAssets)
                {
                    EditorGUILayout.LabelField(assetPath);
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            
            if (GUILayout.Button($"Delete {_unusedAssets.Count} Unused Assets", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Confirm Deletion", 
                    $"Are you sure you want to delete {_unusedAssets.Count} unused assets? This action cannot be undone.", 
                    "Delete", "Cancel"))
                {
                    DeleteUnusedAssets();
                }
            }
        }
    }

    private void AnalyzeUnusedAssets()
    {
        _isAnalyzing = true;
        _unusedAssets.Clear();
        _progress = 0f;

        try
        {
            string folderPath = AssetDatabase.GetAssetPath(_targetFolder);
            
            // Get all assets in the target folder
            _currentOperation = "Finding assets in folder...";
            EditorUtility.DisplayProgressBar("Analyzing", _currentOperation, 0.1f);
            
            string[] allAssetGuids = AssetDatabase.FindAssets("", new[] { folderPath });
            List<string> allAssetPaths = allAssetGuids.Select(AssetDatabase.GUIDToAssetPath).ToList();
            
            // Filter out folders and meta files
            allAssetPaths = allAssetPaths.Where(path => 
                !AssetDatabase.IsValidFolder(path) && 
                !path.EndsWith(".meta") &&
                File.Exists(path)).ToList();

            _progress = 0.2f;
            
            // Get all dependencies from scenes
            _currentOperation = "Analyzing scene dependencies...";
            EditorUtility.DisplayProgressBar("Analyzing", _currentOperation, _progress);
            
            HashSet<string> usedAssets = GetUsedAssetsFromScenes();
            
            _progress = 0.7f;
            
            // Find unused assets
            _currentOperation = "Finding unused assets...";
            EditorUtility.DisplayProgressBar("Analyzing", _currentOperation, _progress);
            
            for (int i = 0; i < allAssetPaths.Count; i++)
            {
                string assetPath = allAssetPaths[i];
                
                if (!usedAssets.Contains(assetPath))
                {
                    _unusedAssets.Add(assetPath);
                }
                
                _progress = 0.7f + (0.3f * i / allAssetPaths.Count);
                if (i % 10 == 0) // Update progress bar every 10 items
                {
                    EditorUtility.DisplayProgressBar("Analyzing", $"Checking asset {i + 1}/{allAssetPaths.Count}", _progress);
                }
            }
            
            _progress = 1f;
            _currentOperation = "Analysis complete";
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            _isAnalyzing = false;
        }
        
        Debug.Log($"Analysis complete. Found {_unusedAssets.Count} unused assets in {AssetDatabase.GetAssetPath(_targetFolder)}");
    }

    private HashSet<string> GetUsedAssetsFromScenes()
    {
        HashSet<string> usedAssets = new HashSet<string>();
        HashSet<string> usedMeshes = new HashSet<string>();
        
        // Store the current scene to restore later
        Scene currentScene = EditorSceneManager.GetActiveScene();
        string currentScenePath = currentScene.path;
        
        for (int i = 0; i < _sceneAssets.Count; i++)
        {
            if (_sceneAssets[i] == null) continue;
            
            string scenePath = AssetDatabase.GetAssetPath(_sceneAssets[i]);
            _currentOperation = $"Analyzing scene {i + 1}/{_sceneAssets.Count}: {Path.GetFileNameWithoutExtension(scenePath)}";
            EditorUtility.DisplayProgressBar("Analyzing", _currentOperation, 0.2f + (0.5f * i / _sceneAssets.Count));
            
            // Get all dependencies for this scene (for non-mesh assets)
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            
            foreach (string dependency in dependencies)
            {
                // Skip built-in Unity assets and the scene itself
                if (!dependency.StartsWith("Assets/") || dependency == scenePath)
                    continue;
                    
                // For mesh assets, we'll check them specifically, so skip adding them here
                if (!IsMeshAsset(dependency))
                {
                    usedAssets.Add(dependency);
                }
            }
            
            // Load the scene to check for actual mesh usage
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            // Find all GameObjects with mesh components
            GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
            
            foreach (GameObject go in allGameObjects)
            {
                // Check MeshFilter components
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    string meshPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
                    if (!string.IsNullOrEmpty(meshPath) && meshPath.StartsWith("Assets/"))
                    {
                        usedMeshes.Add(meshPath);
                    }
                }
                
                // Check SkinnedMeshRenderer components
                SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
                {
                    string meshPath = AssetDatabase.GetAssetPath(skinnedMeshRenderer.sharedMesh);
                    if (!string.IsNullOrEmpty(meshPath) && meshPath.StartsWith("Assets/"))
                    {
                        usedMeshes.Add(meshPath);
                    }
                }
            }
        }
        
        // Restore the original scene
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
        }
        
        // Combine used assets and used meshes
        foreach (string mesh in usedMeshes)
        {
            usedAssets.Add(mesh);
        }
        
        return usedAssets;
    }

    private bool IsMeshAsset(string assetPath)
    {
        string extension = Path.GetExtension(assetPath).ToLower();
        return extension == ".fbx" || extension == ".obj" || extension == ".dae" || 
               extension == ".3ds" || extension == ".blend" || extension == ".ma" || 
               extension == ".mb" || extension == ".max" || extension == ".c4d" ||
               extension == ".asset"; // Unity mesh assets
    }

    private void DeleteUnusedAssets()
    {
        int deletedCount = 0;
        int failedCount = 0;
        
        for (int i = 0; i < _unusedAssets.Count; i++)
        {
            string assetPath = _unusedAssets[i];
            EditorUtility.DisplayProgressBar("Deleting Assets", $"Deleting {Path.GetFileName(assetPath)}", (float)i / _unusedAssets.Count);
            
            if (AssetDatabase.DeleteAsset(assetPath))
            {
                deletedCount++;
            }
            else
            {
                failedCount++;
                Debug.LogWarning($"Failed to delete asset: {assetPath}");
            }
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        
        _unusedAssets.Clear();
        _showPreview = false;
        
        string message = $"Deletion complete!\nDeleted: {deletedCount} assets";
        if (failedCount > 0)
        {
            message += $"\nFailed: {failedCount} assets (check console for details)";
        }
        
        EditorUtility.DisplayDialog("Deletion Complete", message, "OK");
        Debug.Log(message);
    }
}