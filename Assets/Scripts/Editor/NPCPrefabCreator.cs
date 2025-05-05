using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Animations;

public class NPCPrefabCreator : EditorWindow
{
    Texture2D spriteSheet;
    string npcName = "";
    int columns = 8;
    int rows = 3;
    float frameRate = 6f;
    bool isStatic = false;

    [MenuItem("Tools/NPC Prefab Creator")]
    public static void ShowWindow()
    {
        GetWindow<NPCPrefabCreator>("NPC Prefab Creator");
    }

    void OnGUI()
    {
        GUILayout.Label("NPC Prefab Creator", EditorStyles.boldLabel);
        spriteSheet = (Texture2D)EditorGUILayout.ObjectField("Spritesheet", spriteSheet, typeof(Texture2D), false);
        npcName = EditorGUILayout.TextField("NPC Name", npcName);
        isStatic = EditorGUILayout.Toggle("Is Static", isStatic);
        if (GUILayout.Button("Oluştur") && spriteSheet != null && !string.IsNullOrEmpty(npcName))
        {
            CreateNPCPrefab();
        }
    }

    void CreateNPCPrefab()
    {
        string assetPath = AssetDatabase.GetAssetPath(spriteSheet);
        // Determine prefix from spritesheet asset name for sprite naming
        string sheetPrefix = Path.GetFileNameWithoutExtension(assetPath);
        TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        // Klasör yapısını oluştur
        string baseFolder = "Assets/NPC";
        if (!AssetDatabase.IsValidFolder(baseFolder))
            AssetDatabase.CreateFolder("Assets", "NPC");
        string npcFolder = $"{baseFolder}/{npcName}";
        if (!AssetDatabase.IsValidFolder(npcFolder))
            AssetDatabase.CreateFolder(baseFolder, npcName);
        string animFolder = $"{npcFolder}/Animation";
        if (!AssetDatabase.IsValidFolder(animFolder))
            AssetDatabase.CreateFolder(npcFolder, "Animation");
        // Spritesheet'i NPC klasörüne kopyala
        string sheetName = Path.GetFileName(assetPath);
        string destSheet = $"{npcFolder}/{sheetName}";
        AssetDatabase.CopyAsset(assetPath, destSheet);
        AssetDatabase.ImportAsset(destSheet);
        assetPath = destSheet;
        spriteSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (ti.spriteImportMode != SpriteImportMode.Multiple)
        {
            ti.spriteImportMode = SpriteImportMode.Multiple;
            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            int w = spriteSheet.width / columns;
            int h = spriteSheet.height / rows;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    SpriteMetaData meta = new SpriteMetaData();
                    meta.rect = new Rect(x * w, spriteSheet.height - (y + 1) * h, w, h);
                    // Use original prefix for sprite names
                    meta.name = $"{sheetPrefix}_{y * columns + x}";
                    meta.pivot = new Vector2(0.5f, 0f);
                    metas.Add(meta);
                }
            }
            ti.spritesheet = metas.ToArray();
            ti.SaveAndReimport();
        }
        AssetDatabase.Refresh();
        Object[] loadedAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        List<Sprite> spriteList = new List<Sprite>();
        foreach (var asset in loadedAssets)
        {
            if (asset is Sprite s)
                spriteList.Add(s);
        }
        if (spriteList.Count != columns * rows)
        {
            Debug.LogError($"Sprite slicing başarısız oldu. Bulunan sprite sayısı: {spriteList.Count}");
            return;
        }
        Sprite[] sprites = new Sprite[columns * rows];
        for (int i = 0; i < columns * rows; i++)
        {
            // Lookup sprites by original sheet prefix
            string spName = $"{sheetPrefix}_{i}";
            Sprite spr = spriteList.Find(x => x.name == spName);
            if (spr == null)
            {
                Debug.LogError($"Sprite bulunamadı: {spName}");
                return;
            }
            sprites[i] = spr;
        }
        // Direction names for animation naming
        string[] directions = { "Bottom", "BottomLeft", "Left", "TopLeft", "Top", "TopRight", "Right", "BottomRight" };
        // Animasyonları yarat
        List<AnimationClip> idleClips = new List<AnimationClip>();
        List<AnimationClip> moveClips = new List<AnimationClip>();
        for (int dir = 0; dir < columns; dir++)
        {
            string dirName = directions[dir];
            // Idle clip
            AnimationClip idle = new AnimationClip(); idle.frameRate = frameRate;
            var bind = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
            var kfIdle = new ObjectReferenceKeyframe[1] { new ObjectReferenceKeyframe { time = 0, value = sprites[dir] } };
            AnimationUtility.SetObjectReferenceCurve(idle, bind, kfIdle);
            string idlePath = $"{animFolder}/{npcName}_{dirName}_Idle.anim";
            AssetDatabase.CreateAsset(idle, idlePath);
            idleClips.Add(idle);
            if (!isStatic)
            {
                // Move clip
                AnimationClip mv = new AnimationClip(); mv.frameRate = frameRate;
                ObjectReferenceKeyframe[] kfMv = new ObjectReferenceKeyframe[2];
                for (int s = 1; s <= 2; s++) kfMv[s-1] = new ObjectReferenceKeyframe { time = (s-1)/frameRate, value = sprites[s*columns + dir] };
                AnimationUtility.SetObjectReferenceCurve(mv, bind, kfMv);
                string mvPath = $"{animFolder}/{npcName}_{dirName}_Move.anim";
                AssetDatabase.CreateAsset(mv, mvPath);
                // Enable Loop Time on move animation
                var mvSettings = AnimationUtility.GetAnimationClipSettings(mv);
                mvSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(mv, mvSettings);
                moveClips.Add(mv);
            }
        }
        // Animator Controller
        string controllerPath = $"{npcFolder}/{npcName}_Animator.controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("InputX", AnimatorControllerParameterType.Float);
        controller.AddParameter("InputY", AnimatorControllerParameterType.Float);
        if (isStatic)
        {
            // Tek blend tree: idle
            var root = controller.layers[0].stateMachine;
            var state = root.AddState("Idle"); root.defaultState = state;
            BlendTree bt = new BlendTree { name = "IdleTree", blendType = BlendTreeType.FreeformDirectional2D, blendParameter = "InputX", blendParameterY = "InputY" };
            AssetDatabase.AddObjectToAsset(bt, controllerPath);
            state.motion = bt;
            Vector2[] vecs = new Vector2[] {
                new Vector2(0, -1),
                new Vector2(-1, -1),
                new Vector2(-1, 0),
                new Vector2(-1, 1),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(1, -1)
            };
            for(int i=0;i<idleClips.Count;i++) bt.AddChild(idleClips[i], vecs[i]);
        }
        else
        {
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            var root = controller.layers[0].stateMachine;
            var stIdle = root.AddState("Idle");
            var stMove = root.AddState("Move");
            root.defaultState = stIdle;
            // Idle Tree
            BlendTree btI = new BlendTree { name="IdleTree", blendType=BlendTreeType.FreeformDirectional2D, blendParameter="InputX", blendParameterY="InputY" };
            AssetDatabase.AddObjectToAsset(btI, controllerPath);
            stIdle.motion = btI;
            // Move Tree
            BlendTree btM = new BlendTree { name="MoveTree", blendType=BlendTreeType.FreeformDirectional2D, blendParameter="InputX", blendParameterY="InputY" };
            AssetDatabase.AddObjectToAsset(btM, controllerPath);
            stMove.motion = btM;
            Vector2[] vecs2 = new Vector2[] {
                new Vector2(0, -1),
                new Vector2(-1, -1),
                new Vector2(-1, 0),
                new Vector2(-1, 1),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(1, -1)
            };
            for(int i=0;i<idleClips.Count;i++) { btI.AddChild(idleClips[i], vecs2[i]); btM.AddChild(moveClips[i], vecs2[i]); }
            // Geçişler
            var t1 = stIdle.AddTransition(stMove);
            t1.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
            t1.hasExitTime = false;
            var t2 = stMove.AddTransition(stIdle);
            t2.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
            t2.hasExitTime = false;
        }
        // Prefab
        GameObject go = new GameObject(npcName);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;
        // Add BaseNPCController for static facing
        go.AddComponent<BaseNPCController>();
        // Add trigger collider
        var col = go.AddComponent<CapsuleCollider2D>();
        col.isTrigger = true;
        // Add movement script for dynamic NPCs
        if (!isStatic)
        {
            var npcMv = go.AddComponent<NPCMovement>();
        }
        string prefabPath = $"{npcFolder}/{npcName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Başarılı", $"{npcName} prefab ve animasyonları oluşturuldu!", "Tamam");
    }
}
