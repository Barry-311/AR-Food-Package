using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI fpsText;
    private Canvas fpsCanvas;
    private float deltaTime;

    void Awake()
    {
        CreateFPSCanvas();
    }

    void CreateFPSCanvas()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("FPSCanvas");
        canvasObj.transform.SetParent(this.transform); // 挂在摄像机上
        fpsCanvas = canvasObj.AddComponent<Canvas>();
        fpsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fpsCanvas.sortingOrder = 1000; // 保证在最上层

        // 添加Canvas Scaler
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // 添加GraphicRaycaster（尽管不需要，但Unity推荐加上）
        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建FPS Text
        GameObject textObj = new GameObject("FPSText");
        textObj.transform.SetParent(canvasObj.transform);

        fpsText = textObj.AddComponent<TextMeshProUGUI>();

        // 设置字体大小、颜色、对齐方式
        fpsText.fontSize = 48;
        fpsText.color = Color.green;
        fpsText.alignment = TextAlignmentOptions.TopRight;

        // 设置RectTransform（位于右上角）
        RectTransform rectTransform = fpsText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-50, -70);
        rectTransform.sizeDelta = new Vector2(200, 50);
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = Mathf.Ceil(fps).ToString() + " FPS";
    }
}
