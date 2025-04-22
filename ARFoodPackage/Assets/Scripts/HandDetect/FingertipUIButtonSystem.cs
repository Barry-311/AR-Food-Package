using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mediapipe.Tasks.Vision.HandLandmarker;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

public class FingertipUIButtonSystem : MonoBehaviour
{
    [SerializeField] private Canvas canvasRoot;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private float buttonCooldownSeconds = 1.0f;

    private readonly ConcurrentQueue<Vector2[]> fingertipQueue = new ConcurrentQueue<Vector2[]>();
    private Dictionary<Button, float> buttonCooldowns = new Dictionary<Button, float>();
    private List<Button> canvasButtons = new List<Button>();

    void Awake()
    {
        if (canvasRoot == null)
        {
            Debug.LogError("Canvas root not assigned!");
            return;
        }

        // ��ȡCanvas������Button
        canvasButtons.AddRange(canvasRoot.GetComponentsInChildren<Button>(true));
        Debug.Log($"Registered {canvasButtons.Count} buttons in canvas.");

        // ��ʼ����ȴ״̬
        foreach (var btn in canvasButtons)
        {
            buttonCooldowns[btn] = -Mathf.Infinity;
        }
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
                        break; // ��ǰ��ť�����У�������������
                    }
                }
            }
        }
    }

    public void QueueFingerScreenPoints(Vector2[] screenPoints)
    {
        fingertipQueue.Enqueue(screenPoints);
    }
}
