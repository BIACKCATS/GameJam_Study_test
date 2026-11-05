#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class SetupTrain
{
    public static void Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/hty/Scenes/TempFolderScene.unity") throw new System.Exception("Wrong scene");
        if (GameObject.Find("TrainPresentation")) throw new System.Exception("Train already exists");
        var camera = Camera.main;
        if (!camera) throw new System.Exception("Main camera missing");
        var root = new GameObject("TrainPresentation");
        Undo.RegisterCreatedObjectUndo(root, "Set up train");
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/hty/TempFolder/sprite/Train_Interior.png");
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/hty/TempFolder/sprite/Train_Interior.png");
        Sprite sprite = null;
        foreach (var asset in sprites) if (asset is Sprite s) { sprite = s; break; }
        if (!sprite) throw new System.Exception("Interior sprite missing");
        float height = camera.orthographic ? camera.orthographicSize * 2 : 2 * 10 * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad / 2);
        float width = height * camera.aspect;
        root.transform.position = camera.transform.position + camera.transform.forward * 10;
        root.transform.rotation = camera.transform.rotation;
        var inside = new GameObject("Train_Interior");
        inside.transform.SetParent(root.transform, false);
        var sr = inside.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = 1;
        sr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        AssetDatabase.CreateAsset(sr.sharedMaterial, "Assets/hty/TempFolder/TempScript/TrainInterior.mat");
        float scale = Mathf.Max(width / sprite.bounds.size.x, height / sprite.bounds.size.y) * 1.015f;
        inside.transform.localScale = Vector3.one * scale;
        var bg = GameObject.CreatePrimitive(PrimitiveType.Quad); bg.name = "Train_BG";
        Object.DestroyImmediate(bg.GetComponent<Collider>());
        bg.transform.SetParent(root.transform, false); bg.transform.localPosition = new Vector3(0,0,0.1f);
        bg.transform.localScale = new Vector3(width * 1.05f,height * 1.05f,1);
        var mat = new Material(Shader.Find("TempFolder/TrainBackground"));
        mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/hty/TempFolder/sprite/Train_BG_Tunnel_Darker.png");
        AssetDatabase.CreateAsset(mat,"Assets/hty/TempFolder/TempScript/TrainBackground.mat");
        var renderer = bg.GetComponent<Renderer>(); renderer.sharedMaterial = mat; renderer.sortingOrder = 0;
        var motion = root.AddComponent<TrainMotion>(); motion.interior = inside.transform; motion.background = renderer;
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Train setup complete: " + width + " x " + height);
    }
}
#endif
