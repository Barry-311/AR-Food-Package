using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mediapipe.Tasks.Vision.HandLandmarker;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

public class FingertipUIButtonSystem : MonoBehaviour
{
    [SerializeField] private Camera uiCamera;
    [SerializeField] private float buttonCooldownSeconds = 1.0f;

    private readonly ConcurrentQueue<Vector2[]> fingertipQueue = new ConcurrentQueue<Vector2[]>();
    private Dictionary<Button, float> buttonCooldowns = new Dictionary<Button, float>();
    private List<Button> canvasButtons = new List<Button>();

    void Awake()
    {

    }

    void Update()
    {
        while (fingertipQueue.TryDequeue(out var screenPoints))
        {
            foreach (var btn in canvasButtons)
            {
                var rectTransform = btn.GetComponent<RectTransform>();

                foreach (var point in screenPoints)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, point, uiCamera))
                    {
                        if (Time.time >= buttonCooldowns[btn])
                        {
                            Debug.Log($"[{btn.name}] pressed by fingertip!");
                            btn.onClick.Invoke();
                            buttonCooldowns[btn] = Time.time + buttonCooldownSeconds;
                        }
                        else
                        {
                            Debug.Log($"[{btn.name}] is on cooldown.");
                        }
                        break; // 当前按钮被命中，无需检查其他手
                    }
                }
            }
        }
    }

    public void QueueFingerScreenPoints(Vector2[] screenPoints)
    {
        fingertipQueue.Enqueue(screenPoints);
    }

    public void RefreshCanvasButtons(Canvas canvasRoot)
    {
        canvasButtons.Clear();
        if(canvasRoot == null)
        {
            Debug.LogWarning("Canvas root is null, cannot refresh buttons.");
            return;
        }

        canvasButtons.AddRange(canvasRoot.GetComponentsInChildren<Button>(true));
        Debug.Log($"Refreshed and registered {canvasButtons.Count} buttons in canvas.");
        // 初始化冷却状态
        foreach (var btn in canvasButtons)
        {
            buttonCooldowns[btn] = -Mathf.Infinity;
        }
    }
}
