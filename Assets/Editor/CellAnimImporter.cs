using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites; 
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection; 
using System;

public class CellAnimImporter : EditorWindow
{
    private string cellanimFilePath;
    private Texture2D sourceTexture;
    private DefaultAsset animLabelsFile; 

    private CellAnimData parsedData = null;
    private Dictionary<int, string> animGroupAssignments = new Dictionary<int, string>();
    private Dictionary<string, GroupConfig> groupConfigs = new Dictionary<string, GroupConfig>();
    
    private Vector2 scrollPosition;
    private string bulkGroupName = "AllAnims";
    private float resolutionMultiplier = 1f;

    private enum ImporterState { FileSelection, SpriteGrouping, AnimGrouping }
    private ImporterState currentState = ImporterState.FileSelection;

    private List<Rect> allUniqueRects = new List<Rect>();
    private HashSet<int> selectedRectIndices = new HashSet<int>();
    
    [System.Serializable]
    public class SpriteGroup
    {
        public string Name = "New Group";
        public HashSet<int> RectIndices = new HashSet<int>();
    }
    private List<SpriteGroup> spriteGroups = new List<SpriteGroup>();
    private Vector2 spriteSheetScroll;
    private Vector2 selectionStartPos;
    private bool isSelecting = false;
    private Rect selectionRect;
    private float previewZoom = 1f;

    [MenuItem("Tools/CellAnim Importer")]
    public static void ShowWindow()
    {
        GetWindow<CellAnimImporter>("CellAnim Importer");
    }

    private void OnGUI()
    {
        if (currentState == ImporterState.FileSelection) DrawFileSelectionUI();
        else if (currentState == ImporterState.SpriteGrouping) DrawSpriteGroupingUI();
        else if (currentState == ImporterState.AnimGrouping) DrawGroupingUI();
    }

    private void DrawFileSelectionUI()
    {
        EditorGUILayout.Space();

        if (GUILayout.Button("select cellanim (.brcad / .bccad file)"))
        {
            cellanimFilePath = EditorUtility.OpenFilePanel("select cellanim", "", "");
        }
        
        if (!string.IsNullOrEmpty(cellanimFilePath))
        {
            GUILayout.Label($"current file: {Path.GetFileName(cellanimFilePath)}");
        }

        EditorGUILayout.Space();
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("spritesheet texture", sourceTexture, typeof(Texture2D), false);
        
        EditorGUILayout.Space();
        GUILayout.Label("animation labels (for .brcad only)", EditorStyles.label);
        animLabelsFile = (DefaultAsset)EditorGUILayout.ObjectField("labels (.h file)", animLabelsFile, typeof(DefaultAsset), false);

        EditorGUILayout.Space();
        resolutionMultiplier = EditorGUILayout.FloatField("resolution multiplier", resolutionMultiplier);

        EditorGUILayout.Space();
        
        if (Selection.activeGameObject != null)
        {
            GUILayout.Label($"importing as child of: {Selection.activeGameObject.name}", EditorStyles.helpBox);
        }
        else
        {
            GUILayout.Label("select an object to import into", EditorStyles.helpBox);
        }

        EditorGUILayout.Space();

        GUI.enabled = !string.IsNullOrEmpty(cellanimFilePath) && sourceTexture != null;
        if (GUILayout.Button("load anim data", GUILayout.Height(30)))
        {
            LoadData();
        }
        GUI.enabled = true;
    }

    private void DrawSpriteGroupingUI()
    {
        GUILayout.BeginHorizontal();
        
        // spritesheet selection  !!!!!!!!!!!!!!!!!
        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.6f));
        previewZoom = EditorGUILayout.Slider("Preview Zoom", previewZoom, 0.1f, 4f);
        spriteSheetScroll = GUILayout.BeginScrollView(spriteSheetScroll);
        
        if (sourceTexture != null)
        {
            float drawW = sourceTexture.width * previewZoom;
            float drawH = sourceTexture.height * previewZoom;
            Rect texRect = GUILayoutUtility.GetRect(drawW, drawH, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            GUI.DrawTexture(texRect, sourceTexture);

            Event e = Event.current;

            if (e.type == EventType.MouseDown && texRect.Contains(e.mousePosition))
            {
                if (e.button == 0) // mouse 1 (select and drag)
                {
                    isSelecting = true;
                    selectionStartPos = e.mousePosition;
                    selectionRect = new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0);
                    
                    if (!e.shift && !e.control) selectedRectIndices.Clear();
                    e.Use();
                }

                else if (e.button == 1) // mouse 2 (del)
                {
                    int bestMatch = -1; //Ok
                    float smallestArea = float.MaxValue;

                    for (int i = 0; i < allUniqueRects.Count; i++)
                    {
                        Rect raw = allUniqueRects[i];
                        float sX = raw.x * resolutionMultiplier * previewZoom;
                        float sY = raw.y * resolutionMultiplier * previewZoom;
                        float sW = raw.width * resolutionMultiplier * previewZoom;
                        float sH = raw.height * resolutionMultiplier * previewZoom;
                        Rect uiRect = new Rect(texRect.x + sX, texRect.y + sY, sW, sH);
                        
                        if (uiRect.Contains(e.mousePosition))
                        {
                            float area = raw.width * raw.height; // Use unscaled area for priority
                            if (area < smallestArea)
                            {
                                smallestArea = area;
                                bestMatch = i;
                            }
                        }
                    }

                    if (bestMatch != -1)
                    {
                        foreach (var g in spriteGroups)
                        {
                            g.RectIndices.Remove(bestMatch);
                        }
                        selectedRectIndices.Remove(bestMatch);
                    }
                    e.Use();
                }
            }

            else if (e.type == EventType.MouseDrag && isSelecting) // drag select
            {
                float x = Mathf.Min(selectionStartPos.x, e.mousePosition.x);
                float y = Mathf.Min(selectionStartPos.y, e.mousePosition.y);
                float w = Mathf.Abs(selectionStartPos.x - e.mousePosition.x);
                float h = Mathf.Abs(selectionStartPos.y - e.mousePosition.y);
                selectionRect = new Rect(x, y, w, h);
                e.Use();
            }

            else if (e.type == EventType.MouseUp && e.button == 0 && isSelecting) // mouse up
            {
                isSelecting = false;
                
                bool isClick = selectionRect.width < 5f && selectionRect.height < 5f;

                if (isClick)
                {
                    int bestMatch = -1;
                    float smallestArea = float.MaxValue;

                    for (int i = 0; i < allUniqueRects.Count; i++)
                    {
                        Rect raw = allUniqueRects[i];
                        float sX = raw.x * resolutionMultiplier * previewZoom;
                        float sY = raw.y * resolutionMultiplier * previewZoom;
                        float sW = raw.width * resolutionMultiplier * previewZoom;
                        float sH = raw.height * resolutionMultiplier * previewZoom;
                        Rect uiRect = new Rect(texRect.x + sX, texRect.y + sY, sW, sH);
                        
                        if (uiRect.Contains(e.mousePosition))
                        {
                            float area = raw.width * raw.height;
                            if (area < smallestArea)
                            {
                                smallestArea = area;
                                bestMatch = i;
                            }
                        }
                    }

                    if (bestMatch != -1)
                    {
                        if (e.shift || e.control)
                        {
                            if (selectedRectIndices.Contains(bestMatch)) selectedRectIndices.Remove(bestMatch);
                            else selectedRectIndices.Add(bestMatch);
                        }
                        else
                        {
                            selectedRectIndices.Add(bestMatch);
                        }
                    }
                }
                
                else
                {
                    for (int i = 0; i < allUniqueRects.Count; i++)
                    {
                        Rect raw = allUniqueRects[i];
                        float sX = raw.x * resolutionMultiplier * previewZoom;
                        float sY = raw.y * resolutionMultiplier * previewZoom;
                        float sW = raw.width * resolutionMultiplier * previewZoom;
                        float sH = raw.height * resolutionMultiplier * previewZoom;
                        Rect uiRect = new Rect(texRect.x + sX, texRect.y + sY, sW, sH);
                        
                        if (selectionRect.Overlaps(uiRect))
                        {
                            selectedRectIndices.Add(i);
                        }
                    }
                }
                e.Use();
            }

            for (int i = 0; i < allUniqueRects.Count; i++)
            {
                Rect raw = allUniqueRects[i];
                float sX = raw.x * resolutionMultiplier * previewZoom;
                float sY = raw.y * resolutionMultiplier * previewZoom;
                float sW = raw.width * resolutionMultiplier * previewZoom;
                float sH = raw.height * resolutionMultiplier * previewZoom;
                Rect uiRect = new Rect(texRect.x + sX, texRect.y + sY, sW, sH);
                
                bool isSelected = selectedRectIndices.Contains(i);
                SpriteGroup ownerGroup = spriteGroups.FirstOrDefault(g => g.RectIndices.Contains(i));
                bool inAnyGroup = ownerGroup != null;

                Color overlayColor = isSelected ? new Color(0, 0, 1, 0.5f) ://selected 
                                    //grouped
                                    (inAnyGroup ? new Color(0, 1, 0, 0.2f) : new Color(1, 0, 0, 0.2f)); //no group
                
                EditorGUI.DrawRect(uiRect, overlayColor);

                if (inAnyGroup) //label
                {
                    Rect labelRect = new Rect(uiRect.x + 2, uiRect.y + 2, uiRect.width, 20);
                    GUI.color = Color.black;
                    GUI.Label(new Rect(labelRect.x + 1, labelRect.y + 1, labelRect.width, labelRect.height), ownerGroup.Name, EditorStyles.miniBoldLabel);
                    GUI.color = Color.white;
                    GUI.Label(labelRect, ownerGroup.Name, EditorStyles.miniBoldLabel);
                }
            }

            if (isSelecting && selectionRect.width > 5f && selectionRect.height > 5f)
            {
                EditorGUI.DrawRect(selectionRect, new Color(0.5f, 0.8f, 1f, 0.3f));
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();



        // group list panel

        GUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (GUILayout.Button("<- back to file select")) 
        { 
            currentState = ImporterState.FileSelection; 
            return; 
        }
        
        GUILayout.Label("sprite groups", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("select sprites on the left to assign them a group. each group becomes a unique gameobject.", MessageType.Info);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < spriteGroups.Count; i++)
        {
            GUILayout.BeginHorizontal();
            spriteGroups[i].Name = GUILayout.TextField(spriteGroups[i].Name, GUILayout.Width(120));
            
            if (GUILayout.Button("Assign selected"))
            {
                spriteGroups[i].RectIndices.UnionWith(selectedRectIndices);
            }
            if (GUILayout.Button("Clear", GUILayout.Width(48)))
            {
                spriteGroups[i].RectIndices.Clear();
            }
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                spriteGroups.RemoveAt(i);
                i--;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label($"sprites: {spriteGroups[i].RectIndices.Count}", EditorStyles.miniLabel);

            GUILayout.Space(5);
        }
        
        GUILayout.Space(10);
        if (GUILayout.Button("Create new group"))
        {
            spriteGroups.Add(new SpriteGroup { Name = "Group " + spriteGroups.Count });
        }
        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("next -> anim grouping", GUILayout.Height(40)))
        {
            bool allAssigned = true;
            for (int i = 0; i < allUniqueRects.Count; i++)
            {
                if (!spriteGroups.Any(g => g.RectIndices.Contains(i)))
                {
                    allAssigned = false;
                    break;
                }
            }
            
            if (!allAssigned)
            {
                if (EditorUtility.DisplayDialog("warning", "not every sprite has been assigned a group!!! unassigned parts will be created for any groupless sprites. continue anyway?", "yes (start import)", "cancel"))
                {
                    currentState = ImporterState.AnimGrouping;
                }
            }
            else
            {
                currentState = ImporterState.AnimGrouping;
            } // geniunely drove me insane i kept missing sprites
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void DrawGroupingUI()
    {
        if (GUILayout.Button("<- back to sprite grouping"))
        {
            currentState = ImporterState.SpriteGrouping;
            return;
        }

        EditorGUILayout.Space();
        GUILayout.Label("group and combine animations", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("anims with the same target group name will be combined into one object / animator. this is done automatically but you can edit them here.", MessageType.Info);

        GUILayout.BeginHorizontal();
        bulkGroupName = EditorGUILayout.TextField(bulkGroupName);
        if (GUILayout.Button("import all combined", GUILayout.Width(160)))
        {
            for (int i = 0; i < parsedData.Animations.Count; i++)
            {
                animGroupAssignments[i] = bulkGroupName;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Anim Name", EditorStyles.boldLabel, GUILayout.Width(250));
        GUILayout.Label("Target Group", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();

        // list layout
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox, GUILayout.MaxHeight(250));
        for (int i = 0; i < parsedData.Animations.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(parsedData.Animations[i].Name, GUILayout.Width(250));
            animGroupAssignments[i] = EditorGUILayout.TextField(animGroupAssignments[i]);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        EditorGUILayout.Space();
        
        // default arr bullshit
        GUILayout.Label("default arrangement on import", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("choose which animation will be considered the default (how it will be imported in the editor)", MessageType.Info);

        HashSet<string> uniqueGroups = new HashSet<string>();
        foreach (var kvp in animGroupAssignments)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value)) uniqueGroups.Add(kvp.Value);
        }

        foreach (string gName in uniqueGroups)
        {
            if (!groupConfigs.ContainsKey(gName)) groupConfigs[gName] = new GroupConfig();

            List<int> animsInGroup = new List<int>();
            List<string> animNamesInGroup = new List<string>();
            for (int i = 0; i < parsedData.Animations.Count; i++)
            {
                if (animGroupAssignments[i] == gName)
                {
                    animsInGroup.Add(i);
                    animNamesInGroup.Add(parsedData.Animations[i].Name);
                }
            }

            if (animsInGroup.Count == 0) continue;

            int currentIndex = groupConfigs[gName].MainAnimationIndex;
            int selectedPopupIndex = Mathf.Max(0, animsInGroup.IndexOf(currentIndex));

            GUILayout.BeginHorizontal();
            GUILayout.Label(gName, GUILayout.Width(150));
            
            selectedPopupIndex = EditorGUILayout.Popup(selectedPopupIndex, animNamesInGroup.ToArray(), GUILayout.Width(200));
            groupConfigs[gName].MainAnimationIndex = animsInGroup[selectedPopupIndex];

            int frameIndex = groupConfigs[gName].UseLastFrame ? 1 : 0;
            frameIndex = EditorGUILayout.Popup(frameIndex, new string[] { "First Frame", "Last Frame" }, GUILayout.Width(100));
            groupConfigs[gName].UseLastFrame = (frameIndex == 1);

            groupConfigs[gName].CustomFrameIndex = EditorGUILayout.IntField(groupConfigs[gName].CustomFrameIndex, GUILayout.Width(40));

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("import and generate assets", GUILayout.Height(40)))
        {
            GenerateAssets();
        }
    }

    private void LoadData()
    {
        string[] names = null;
        if (animLabelsFile != null)
        {
            string labelPath = AssetDatabase.GetAssetPath(animLabelsFile); 
            names = ParseHeaderLabels(labelPath);
        }

        parsedData = CellAnimParser.Parse(cellanimFilePath, names);
        animGroupAssignments.Clear();
        groupConfigs.Clear();

        for (int i = 0; i < parsedData.Animations.Count; i++)
        {
            animGroupAssignments[i] = GetGroupName(parsedData.Animations[i].Name);
        }

        ExtractUniqueRects();
        spriteGroups.Clear();
        selectedRectIndices.Clear();
        currentState = ImporterState.SpriteGrouping;
    }

    private void ExtractUniqueRects()
    {
        allUniqueRects.Clear();
        foreach (var arr in parsedData.Arrangements)
        {
            foreach (var part in arr.Parts)
            {
                Rect r = new Rect(part.CellX, part.CellY, part.CellW, part.CellH);
                if (!allUniqueRects.Contains(r))
                {
                    allUniqueRects.Add(r);
                }
            }
        }
    }

    private string[] ParseHeaderLabels(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        List<string> namesList = new List<string>();

        foreach (string line in lines)
        {
            if (line.StartsWith("#define"))
            {
                string[] parts = line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    string animName = parts[1];
                    if (int.TryParse(parts[2], out int index))
                    {
                        while (namesList.Count <= index) namesList.Add("");
                        namesList[index] = animName;
                    }
                }
            }
        }
        return namesList.ToArray();
    }

    private string GetGroupName(string animName)
    {
        if (string.IsNullOrEmpty(animName)) return "Main";

        if (animName.Contains("_"))
        {
            string[] parts = animName.Split('_');
            return parts.Length >= 2 ? parts[1] : parts[0];
        }
        else
        {
            int splitIndex = -1;
            for (int i = 1; i < animName.Length; i++)
            {
                if (char.IsUpper(animName[i]) || char.IsDigit(animName[i]))
                {
                    splitIndex = i;
                    break;
                }
            }
            return splitIndex != -1 ? animName.Substring(0, splitIndex) : animName;
        }
    }

    private Dictionary<Rect, Sprite> SpriteSliceAndSetup(CellAnimData data, Texture2D tex)
    {
        string texPath = AssetDatabase.GetAssetPath(tex);
        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(texPath);
        
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(ti);
        dataProvider.InitSpriteEditorDataProvider();

        Dictionary<string, int> groupCounters = new Dictionary<string, int>();
        int ungroupedCounter = 1;

        List<SpriteRect> spriteRects = new List<SpriteRect>();
        for (int i = 0; i < allUniqueRects.Count; i++)
        {
            Rect raw = allUniqueRects[i];
            float cX = raw.x * resolutionMultiplier;
            float cY = raw.y * resolutionMultiplier;
            float cW = raw.width * resolutionMultiplier;
            float cH = raw.height * resolutionMultiplier;

            float unityY = tex.height - cY - cH;
            Rect r = new Rect(cX, unityY, cW, cH);

            string spriteName = null;
            for (int g = 0; g < spriteGroups.Count; g++)
            {
                if (spriteGroups[g].RectIndices.Contains(i))
                {
                    string gName = spriteGroups[g].Name;
                    if (!groupCounters.ContainsKey(gName)) groupCounters[gName] = 1;
                    spriteName = $"{gName}_{groupCounters[gName]++}";
                    break;
                }
            }

            if (spriteName == null) spriteName = $"Ungrouped_{ungroupedCounter++}";

            SpriteRect newSprite = new SpriteRect
            {
                name = spriteName,
                spriteID = GUID.Generate(), 
                rect = r,
                alignment = SpriteAlignment.Center, 
                pivot = new Vector2(0.5f, 0.5f)
            };
            spriteRects.Add(newSprite);
        }

        dataProvider.SetSpriteRects(spriteRects.ToArray());
        dataProvider.Apply();

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();

        Dictionary<Rect, Sprite> map = new Dictionary<Rect, Sprite>();
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texPath);

        foreach (var asset in assets)
        {
            if (asset is Sprite s) map[s.rect] = s;
        }

        return map;
    }

    private void GenerateAssets()
    {
        Dictionary<Rect, Sprite> spriteMap = SpriteSliceAndSetup(parsedData, sourceTexture);
        BuildSceneAndAnimations(parsedData, sourceTexture, spriteMap);

        Debug.Log("cellanim import complete");
        
        parsedData = null;
        animGroupAssignments.Clear();
        groupConfigs.Clear();
        currentState = ImporterState.FileSelection;
    }

    private void BuildSceneAndAnimations(CellAnimData data, Texture2D tex, Dictionary<Rect, Sprite> spriteMap)
    {
        GameObject rootObject = new GameObject(tex.name + "_Rig");
        if (Selection.activeGameObject != null)
        {
            rootObject.transform.SetParent(Selection.activeGameObject.transform, false);
        }

        string texPath = AssetDatabase.GetAssetPath(tex);
        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(texPath);
        float ppu = ti.spritePixelsPerUnit;

        Dictionary<string, List<CellAnimAnimation>> groupedAnims = new Dictionary<string, List<CellAnimAnimation>>();
        
        for (int i = 0; i < data.Animations.Count; i++)
        {
            CellAnimAnimation anim = data.Animations[i];
            
            string objectName = animGroupAssignments.ContainsKey(i) ? animGroupAssignments[i] : "Main";
            if (string.IsNullOrWhiteSpace(objectName)) objectName = "Main";

            if (!groupedAnims.ContainsKey(objectName))
                groupedAnims[objectName] = new List<CellAnimAnimation>();
                
            groupedAnims[objectName].Add(anim);
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Sprites")) AssetDatabase.CreateFolder("Assets/Resources", "Sprites");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Sprites/ImportedAnimations")) AssetDatabase.CreateFolder("Assets/Resources/Sprites", "ImportedAnimations");

        foreach (var group in groupedAnims)
        {
            string groupName = group.Key;
            List<CellAnimAnimation> groupAnimations = group.Value;

            GameObject parentObj = new GameObject(groupName);
            parentObj.transform.SetParent(rootObject.transform, false);
            Animator animator = parentObj.AddComponent<Animator>();

            string controllerPath = $"Assets/Resources/Sprites/ImportedAnimations/{tex.name}_{groupName}.controller";
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            animator.runtimeAnimatorController = controller;

            CellAnimAnimation mainAnim = groupAnimations.Count > 0 ? groupAnimations[0] : null;
            bool useLastFrame = false;
            int customFrameIndex = -1;

            if (groupConfigs.TryGetValue(groupName, out GroupConfig config))
            {
                useLastFrame = config.UseLastFrame;
                customFrameIndex = config.CustomFrameIndex;
                if (config.MainAnimationIndex >= 0 && config.MainAnimationIndex < data.Animations.Count)
                {
                    mainAnim = data.Animations[config.MainAnimationIndex];
                }
            }

            CellAnimKey defaultKey = null;
            if (mainAnim != null && mainAnim.Keys.Count > 0)
            {
                if (customFrameIndex >= 0 && customFrameIndex < mainAnim.Keys.Count)
                {
                    defaultKey = mainAnim.Keys[customFrameIndex];
                }
                else
                {
                    defaultKey = useLastFrame ? mainAnim.Keys[mainAnim.Keys.Count - 1] : mainAnim.Keys[0];
                }
            }

            if (defaultKey != null)
            {
                parentObj.transform.localPosition = new Vector3(-(defaultKey.PositionX * resolutionMultiplier) / ppu, (defaultKey.PositionY * resolutionMultiplier) / ppu, 0);
            }

            GameObject keyContainer = new GameObject("KeyContainer");
            keyContainer.transform.SetParent(parentObj.transform, false);
            
            if (defaultKey != null)
            {
                keyContainer.transform.localPosition = new Vector3((defaultKey.PositionX * resolutionMultiplier) / ppu, -(defaultKey.PositionY * resolutionMultiplier) / ppu, 0);
                keyContainer.transform.localScale = new Vector3(defaultKey.ScaleX, defaultKey.ScaleY, 1f);
                keyContainer.transform.localEulerAngles = new Vector3(0, 0, -defaultKey.Angle);
            }

            // SPRITE GROUP LOGIC
            int[] maxGroupUsage = new int[spriteGroups.Count];
            for (int i = 0; i < spriteGroups.Count; i++) maxGroupUsage[i] = 0;
            int globalMaxUngrouped = 0;

            foreach (var anim in groupAnimations)
            {
                foreach (var key in anim.Keys)
                {
                    CellAnimArrangement arr = data.Arrangements[key.ArrangementIndex];
                    int[] currentUsage = new int[spriteGroups.Count];
                    int ungroupedCount = 0;

                    for (int p = 0; p < arr.Parts.Count; p++)
                    {
                        var part = arr.Parts[p];
                        Rect rawRect = new Rect(part.CellX, part.CellY, part.CellW, part.CellH);
                        int rectIndex = allUniqueRects.IndexOf(rawRect);
                        
                        int bestGroup = -1;
                        int lowestUsage = int.MaxValue;

                        for (int g = 0; g < spriteGroups.Count; g++)
                        {
                            if (spriteGroups[g].RectIndices.Contains(rectIndex))
                            {
                                if (currentUsage[g] < lowestUsage)
                                {
                                    lowestUsage = currentUsage[g];
                                    bestGroup = g;
                                    if (lowestUsage == 0) break;
                                }
                            }
                        }

                        if (bestGroup != -1)
                        {
                            currentUsage[bestGroup]++;
                        }
                        else
                        {
                            ungroupedCount++;
                        }
                    }

                    for (int g = 0; g < spriteGroups.Count; g++)
                    {
                        if (currentUsage[g] > maxGroupUsage[g]) maxGroupUsage[g] = currentUsage[g];
                    }
                    if (ungroupedCount > globalMaxUngrouped) globalMaxUngrouped = ungroupedCount;
                }
            }

            int[] groupStartOffsets = new int[spriteGroups.Count];
            int currentOffset = 0;
            for (int i = 0; i < spriteGroups.Count; i++)
            {
                groupStartOffsets[i] = currentOffset;
                currentOffset += maxGroupUsage[i];
            }
            int ungroupedStartOffset = currentOffset;
            int totalObjects = ungroupedStartOffset + globalMaxUngrouped;

            string[] objNames = new string[totalObjects];
            for (int i = 0; i < spriteGroups.Count; i++)
            {
                int clones = maxGroupUsage[i];
                for (int c = 0; c < clones; c++)
                {
                    int flatIndex = groupStartOffsets[i] + c;
                    objNames[flatIndex] = (c == 0) ? spriteGroups[i].Name : $"{spriteGroups[i].Name}_{c + 1}";
                }
            }
            for (int i = 0; i < globalMaxUngrouped; i++)
            {
                objNames[ungroupedStartOffset + i] = $"Ungrouped_Part_{i}";
            }

            Dictionary<CellAnimKey, int[]> keyToFlatIndexMap = new Dictionary<CellAnimKey, int[]>();

            foreach (var anim in groupAnimations)
            {
                Dictionary<int, Vector2> lastPositions = new Dictionary<int, Vector2>();

                for (int k = 0; k < anim.Keys.Count; k++)
                {
                    var key = anim.Keys[k];
                    CellAnimArrangement arr = data.Arrangements[key.ArrangementIndex];
                    int[] flatIndices = new int[arr.Parts.Count];
                    List<int> ungroupedParts = new List<int>();
                    List<(int pIdx, int flatIdx, float dist)> distances = new List<(int, int, float)>();

                    for (int p = 0; p < arr.Parts.Count; p++)
                    {
                        var part = arr.Parts[p];
                        Rect rawRect = new Rect(part.CellX, part.CellY, part.CellW, part.CellH);
                        int rectIndex = allUniqueRects.IndexOf(rawRect);
                        
                        List<int> eligibleFlatIndices = new List<int>();
                        for (int g = 0; g < spriteGroups.Count; g++)
                        {
                            if (spriteGroups[g].RectIndices.Contains(rectIndex))
                            {
                                for (int c = 0; c < maxGroupUsage[g]; c++)
                                {
                                    eligibleFlatIndices.Add(groupStartOffsets[g] + c);
                                }
                            }
                        }

                        if (eligibleFlatIndices.Count > 0)
                        {
                            Vector2 currentPos = new Vector2(part.PositionX, part.PositionY);
                            foreach (int fIdx in eligibleFlatIndices)
                            {
                                float dist = float.MaxValue;
                                if (k > 0 && lastPositions.TryGetValue(fIdx, out Vector2 lastPos))
                                {
                                    dist = Vector2.Distance(currentPos, lastPos);
                                }
                                else if (k == 0)
                                {
                                    dist = p; 
                                }
                                distances.Add((p, fIdx, dist));
                            }
                        }
                        else
                        {
                            ungroupedParts.Add(p);
                        }
                    }

                    distances.Sort((a, b) => {
                        int cmp = a.dist.CompareTo(b.dist);
                        if (cmp != 0) return cmp;
                        return a.flatIdx.CompareTo(b.flatIdx);
                    });

                    HashSet<int> assignedP = new HashSet<int>();
                    HashSet<int> assignedF = new HashSet<int>();

                    foreach (var match in distances)
                    {
                        if (!assignedP.Contains(match.pIdx) && !assignedF.Contains(match.flatIdx))
                        {
                            flatIndices[match.pIdx] = match.flatIdx;
                            assignedP.Add(match.pIdx);
                            assignedF.Add(match.flatIdx);
                        }
                    }

                    for (int p = 0; p < arr.Parts.Count; p++)
                    {
                        if (ungroupedParts.Contains(p) || assignedP.Contains(p)) continue;
                        
                        var part = arr.Parts[p];
                        Rect rawRect = new Rect(part.CellX, part.CellY, part.CellW, part.CellH);
                        int rectIndex = allUniqueRects.IndexOf(rawRect);
                        
                        for (int g = 0; g < spriteGroups.Count; g++)
                        {
                            if (spriteGroups[g].RectIndices.Contains(rectIndex))
                            {
                                for (int c = 0; c < maxGroupUsage[g]; c++)
                                {
                                    int fIdx = groupStartOffsets[g] + c;
                                    if (!assignedF.Contains(fIdx))
                                    {
                                        flatIndices[p] = fIdx;
                                        assignedP.Add(p);
                                        assignedF.Add(fIdx);
                                        break;
                                    }
                                }
                            }
                            if (assignedP.Contains(p)) break;
                        }
                        
                        if (!assignedP.Contains(p)) ungroupedParts.Add(p);
                    }

                    int currentUngrouped = 0;
                    foreach (int pIdx in ungroupedParts)
                    {
                        flatIndices[pIdx] = ungroupedStartOffset + currentUngrouped;
                        currentUngrouped++;
                    }

                    for (int p = 0; p < arr.Parts.Count; p++)
                    {
                        int flatIdx = flatIndices[p];
                        lastPositions[flatIdx] = new Vector2(arr.Parts[p].PositionX, arr.Parts[p].PositionY);
                    }

                    keyToFlatIndexMap[key] = flatIndices;
                }
            }

            CellAnimArrangement defaultArr = defaultKey != null ? data.Arrangements[defaultKey.ArrangementIndex] : null;
            int[] defaultFlatIndices = defaultKey != null ? keyToFlatIndexMap[defaultKey] : null;

            for (int i = 0; i < totalObjects; i++)
            {
                GameObject partObj = new GameObject(objNames[i]);
                partObj.transform.SetParent(keyContainer.transform, false);
                SpriteRenderer sr = partObj.AddComponent<SpriteRenderer>();
                
                int defaultPartIndex = -1;
                if (defaultFlatIndices != null)
                {
                    defaultPartIndex = Array.IndexOf(defaultFlatIndices, i);
                }

                if (defaultPartIndex != -1)
                {
                    CellAnimPart part = defaultArr.Parts[defaultPartIndex];
                    partObj.SetActive(true);
                    
                    float cX = part.CellX * resolutionMultiplier;
                    float cY = part.CellY * resolutionMultiplier;
                    float cW = part.CellW * resolutionMultiplier;
                    float cH = part.CellH * resolutionMultiplier;

                    float unityY = tex.height - cY - cH;
                    Rect cellRect = new Rect(cX, unityY, cW, cH);
                    
                    if (spriteMap.TryGetValue(cellRect, out Sprite slicedSprite))
                    {
                        sr.sprite = slicedSprite;
                    }

                    float scaledWidth = cW * part.ScaleX;
                    float scaledHeight = cH * part.ScaleY;
                    float centerX = (part.PositionX * resolutionMultiplier) + (scaledWidth / 2f);
                    float centerY = (part.PositionY * resolutionMultiplier) + (scaledHeight / 2f);

                    partObj.transform.localPosition = new Vector3(centerX / ppu, -centerY / ppu, 0);
                    partObj.transform.localScale = new Vector3(part.ScaleX, part.ScaleY, 1f);
                    partObj.transform.localEulerAngles = new Vector3(0, 0, -part.Angle);
                    
                    sr.flipX = part.FlipX;
                    sr.flipY = part.FlipY;
                    sr.sortingOrder = defaultPartIndex;

                    float defaultAlpha = (part.Opacity / 255f) * (defaultKey.Opacity / 255f);
                    sr.color = new Color(1f, 1f, 1f, defaultAlpha);
                }
                else
                {
                    partObj.SetActive(false); 
                }
            }

            foreach (var anim in groupAnimations)
            {
                AnimationClip clip = new AnimationClip { frameRate = 60f };

                AnimationCurve keyPosX = new AnimationCurve();
                AnimationCurve keyPosY = new AnimationCurve();
                AnimationCurve keyScaleX = new AnimationCurve();
                AnimationCurve keyScaleY = new AnimationCurve();
                AnimationCurve keyRotZ = new AnimationCurve();

                PartAnimationData[] partAnims = new PartAnimationData[totalObjects];
                for (int i = 0; i < totalObjects; i++) partAnims[i] = new PartAnimationData();
                bool[] objectEverActive = new bool[totalObjects];

                bool isInterp = false;
                FieldInfo interpField = anim.GetType().GetField("isInterpolated", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                PropertyInfo interpProp = anim.GetType().GetProperty("isInterpolated", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                
                if (interpField != null) isInterp = (bool)interpField.GetValue(anim);
                else if (interpProp != null) isInterp = (bool)interpProp.GetValue(anim);

                float currentTime = 0f;

                for (int k = 0; k < anim.Keys.Count; k++)
                {
                    var key = anim.Keys[k];
                    
                    int iterations = (k == anim.Keys.Count - 1 && key.HoldFrames > 1) ? 2 : 1;
                    
                    for (int iter = 0; iter < iterations; iter++)
                    {
                        float frameOffset = (iter == 0) ? 0f : (key.HoldFrames - 1f);
                        float time = (currentTime + frameOffset) / 60f;

                        AddAnimKey(keyPosX, time, (key.PositionX * resolutionMultiplier) / ppu, isInterp);
                        AddAnimKey(keyPosY, time, -(key.PositionY * resolutionMultiplier) / ppu, isInterp);
                        AddAnimKey(keyScaleX, time, key.ScaleX, isInterp);
                        AddAnimKey(keyScaleY, time, key.ScaleY, isInterp);
                        AddAnimKey(keyRotZ, time, -key.Angle, isInterp);

                        CellAnimArrangement arrangement = data.Arrangements[key.ArrangementIndex];
                        int[] flatIndices = keyToFlatIndexMap[key];
                        bool[] objActiveThisFrame = new bool[totalObjects];

                        for (int p = 0; p < arrangement.Parts.Count; p++)
                        {
                            CellAnimPart part = arrangement.Parts[p];
                            int targetObjIndex = flatIndices[p];
                            
                            objActiveThisFrame[targetObjIndex] = true;
                            objectEverActive[targetObjIndex] = true;
                            var pAnim = partAnims[targetObjIndex];

                            AddSteppedKey(pAnim.active, time, 1f);

                            float cX = part.CellX * resolutionMultiplier;
                            float cY = part.CellY * resolutionMultiplier;
                            float cW = part.CellW * resolutionMultiplier;
                            float cH = part.CellH * resolutionMultiplier;

                            float unityY = tex.height - cY - cH;
                            Rect cellRect = new Rect(cX, unityY, cW, cH);
                            if (spriteMap.TryGetValue(cellRect, out Sprite slicedSprite))
                            {
                                if (pAnim.sprites.Count == 0 || pAnim.sprites[pAnim.sprites.Count - 1].time != time)
                                {
                                    pAnim.sprites.Add(new ObjectReferenceKeyframe { time = time, value = slicedSprite });
                                }
                            }

                            float scaledWidth = cW * part.ScaleX;
                            float scaledHeight = cH * part.ScaleY;
                            float centerX = (part.PositionX * resolutionMultiplier) + (scaledWidth / 2f);
                            float centerY = (part.PositionY * resolutionMultiplier) + (scaledHeight / 2f);

                            AddAnimKey(pAnim.posX, time, centerX / ppu, isInterp);
                            AddAnimKey(pAnim.posY, time, -centerY / ppu, isInterp);
                            AddAnimKey(pAnim.scaleX, time, part.ScaleX, isInterp);
                            AddAnimKey(pAnim.scaleY, time, part.ScaleY, isInterp);
                            AddAnimKey(pAnim.rotZ, time, -part.Angle, isInterp);
                            
                            AddSteppedKey(pAnim.flipX, time, part.FlipX ? 1f : 0f); 
                            AddSteppedKey(pAnim.flipY, time, part.FlipY ? 1f : 0f); 

                            float alpha = (part.Opacity / 255f) * (key.Opacity / 255f);
                            AddSteppedKey(pAnim.colorR, time, 1f);
                            AddSteppedKey(pAnim.colorG, time, 1f);
                            AddSteppedKey(pAnim.colorB, time, 1f);
                            
                            if (isInterp) AddAnimKey(pAnim.colorA, time, alpha, true);
                            else AddSteppedKey(pAnim.colorA, time, alpha);
                            
                            AddSteppedKey(pAnim.sortOrder, time, p); 
                        }

                        for (int i = 0; i < totalObjects; i++)
                        {
                            if (!objActiveThisFrame[i])
                            {
                                AddSteppedKey(partAnims[i].active, time, 0f);
                            }
                        }
                    }

                    currentTime += key.HoldFrames;
                }

                clip.SetCurve("KeyContainer", typeof(Transform), "localPosition.x", keyPosX);
                clip.SetCurve("KeyContainer", typeof(Transform), "localPosition.y", keyPosY);
                clip.SetCurve("KeyContainer", typeof(Transform), "localScale.x", keyScaleX);
                clip.SetCurve("KeyContainer", typeof(Transform), "localScale.y", keyScaleY);
                clip.SetCurve("KeyContainer", typeof(Transform), "localEulerAnglesRaw.z", keyRotZ);

                for (int i = 0; i < totalObjects; i++)
                {
                    if (!objectEverActive[i]) continue;

                    string path = $"KeyContainer/{objNames[i]}";
                    var pAnim = partAnims[i];

                    clip.SetCurve(path, typeof(GameObject), "m_IsActive", pAnim.active);
                    clip.SetCurve(path, typeof(Transform), "localPosition.x", pAnim.posX);
                    clip.SetCurve(path, typeof(Transform), "localPosition.y", pAnim.posY);
                    clip.SetCurve(path, typeof(Transform), "localScale.x", pAnim.scaleX);
                    clip.SetCurve(path, typeof(Transform), "localScale.y", pAnim.scaleY);
                    clip.SetCurve(path, typeof(Transform), "localEulerAnglesRaw.z", pAnim.rotZ);

                    clip.SetCurve(path, typeof(SpriteRenderer), "m_FlipX", pAnim.flipX);
                    clip.SetCurve(path, typeof(SpriteRenderer), "m_FlipY", pAnim.flipY);
                    clip.SetCurve(path, typeof(SpriteRenderer), "m_Color.r", pAnim.colorR);
                    clip.SetCurve(path, typeof(SpriteRenderer), "m_Color.g", pAnim.colorG);
                    clip.SetCurve(path, typeof(SpriteRenderer), "m_Color.b", pAnim.colorB);
                    clip.SetCurve(path, typeof(SpriteRenderer), "m_Color.a", pAnim.colorA);
                    clip.SetCurve(path, typeof(SpriteRenderer), "m_SortingOrder", pAnim.sortOrder);

                    if (pAnim.sprites.Count > 0)
                    {
                        EditorCurveBinding spriteBinding = new EditorCurveBinding
                        {
                            type = typeof(SpriteRenderer),
                            path = path,
                            propertyName = "m_Sprite"
                        };
                        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, pAnim.sprites.ToArray());
                    }
                }

                AssetDatabase.CreateAsset(clip, $"Assets/Resources/Sprites/ImportedAnimations/{anim.Name}.anim");
                controller.AddMotion(clip);
            }
        }
        AssetDatabase.SaveAssets();
    }

    private void AddSteppedKey(AnimationCurve curve, float time, float value)
    {
        if (curve.length > 0 && Mathf.Approximately(curve.keys[curve.length - 1].value, value)) return;
        
        int index = curve.AddKey(new Keyframe(time, value));
        if (index != -1)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(curve, index, AnimationUtility.TangentMode.Constant);
        }
    }

    private void AddAnimKey(AnimationCurve curve, float time, float value, bool isInterpolated)
    {
        if (!isInterpolated && curve.length > 0 && Mathf.Approximately(curve.keys[curve.length - 1].value, value)) return;
        
        int index = curve.AddKey(new Keyframe(time, value));
        if (index != -1) 
        {
            if (isInterpolated)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, index, AnimationUtility.TangentMode.Linear);
            }
            else
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(curve, index, AnimationUtility.TangentMode.Constant);
            }
        }
    }

    private class PartAnimationData
    {
        public AnimationCurve active = new AnimationCurve();
        public AnimationCurve posX = new AnimationCurve();
        public AnimationCurve posY = new AnimationCurve();
        public AnimationCurve scaleX = new AnimationCurve();
        public AnimationCurve scaleY = new AnimationCurve();
        public AnimationCurve rotZ = new AnimationCurve();
        public AnimationCurve flipX = new AnimationCurve();
        public AnimationCurve flipY = new AnimationCurve();
        public AnimationCurve colorR = new AnimationCurve();
        public AnimationCurve colorG = new AnimationCurve();
        public AnimationCurve colorB = new AnimationCurve();
        public AnimationCurve colorA = new AnimationCurve();
        public AnimationCurve sortOrder = new AnimationCurve();
        public List<ObjectReferenceKeyframe> sprites = new List<ObjectReferenceKeyframe>();
    }

    public class GroupConfig 
    {
        public int MainAnimationIndex = -1;
        public bool UseLastFrame = false;
        public int CustomFrameIndex = -1;
    }
}