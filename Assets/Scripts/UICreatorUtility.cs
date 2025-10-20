using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utilidad para crear prefabs de UI desde el editor de Unity
/// Menú: Tools/Calcul Fatal/Create UI Prefabs
/// </summary>
public class UICreatorUtility : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Calcul Fatal/Create TroopUI Prefab")]
    static void CreateTroopUIPrefab()
    {
        // Crear objeto raíz con Canvas
        GameObject root = new GameObject("TroopUI");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Ajustar tamaño del canvas
        RectTransform canvasRect = root.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 1);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        // Añadir CanvasScaler
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        
        // Crear fondo/icono
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(root.transform, false);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = new Color(1f, 1f, 1f, 0.8f);
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(100, 100);
        iconRect.anchoredPosition = Vector2.zero;
        
        // Crear texto
        GameObject textObj = new GameObject("ValueText");
        textObj.transform.SetParent(root.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "1";
        text.fontSize = 80;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        text.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(100, 100);
        textRect.anchoredPosition = Vector2.zero;
        
        // Añadir componente TroopUI
        TroopUI troopUI = root.AddComponent<TroopUI>();
        troopUI.worldCanvas = canvas;
        troopUI.iconImage = iconImage;
        troopUI.valueText = text;
        
        // Crear prefab
        string path = "Assets/Prefabs/TroopUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        
        Debug.Log($"TroopUI prefab creado en: {path}");
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Tools/Calcul Fatal/Create OperationUI Prefab")]
    static void CreateOperationUIPrefab()
    {
        // Crear objeto raíz con Canvas
        GameObject root = new GameObject("OperationUI");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Ajustar tamaño del canvas
        RectTransform canvasRect = root.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(3, 1);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        // Añadir CanvasScaler
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        
        // Crear fondo/icono
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(root.transform, false);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = new Color(1f, 1f, 0.8f, 0.8f);
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(150, 100);
        iconRect.anchoredPosition = Vector2.zero;
        
        // Crear texto
        GameObject textObj = new GameObject("OperationText");
        textObj.transform.SetParent(root.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "1+2";
        text.fontSize = 60;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        text.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(150, 100);
        textRect.anchoredPosition = Vector2.zero;
        
        // Añadir componente OperationUI
        OperationUI operationUI = root.AddComponent<OperationUI>();
        operationUI.worldCanvas = canvas;
        operationUI.iconImage = iconImage;
        operationUI.operationText = text;
        
        // Crear prefab
        string path = "Assets/Prefabs/OperationUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        
        Debug.Log($"OperationUI prefab creado en: {path}");
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Tools/Calcul Fatal/Create TowerHealthBar Prefab")]
    static void CreateTowerHealthBarPrefab()
    {
        // Crear objeto raíz con Canvas
        GameObject root = new GameObject("TowerHealthBar");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Ajustar tamaño del canvas
        RectTransform canvasRect = root.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(3, 0.5f);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        // Añadir CanvasScaler
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        
        // Crear el Slider
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(root.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(200, 30);
        sliderRect.anchoredPosition = Vector2.zero;
        
        // Crear Background del slider
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Crear Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-5, -5);
        
        // Crear Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        // Configurar el Slider
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        slider.interactable = false;
        
        // Añadir componente TowerHealthBar
        TowerHealthBar healthBar = root.AddComponent<TowerHealthBar>();
        healthBar.worldCanvas = canvas;
        healthBar.healthSlider = slider;
        healthBar.sliderFillImage = fillImage;
        
        // Crear prefab
        string path = "Assets/Prefabs/TowerHealthBar.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        
        Debug.Log($"TowerHealthBar prefab creado en: {path}");
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Tools/Calcul Fatal/Create All UI Prefabs")]
    static void CreateAllUIPrefabs()
    {
        CreateTroopUIPrefab();
        CreateOperationUIPrefab();
        CreateTowerHealthBarPrefab();
        
        Debug.Log("Todos los prefabs de UI han sido creados!");
    }
#endif
}
