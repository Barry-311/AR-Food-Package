using UnityEngine;
using Vuforia;

public class VuforiaTargetHandler : MonoBehaviour
{
    private ObserverBehaviour observerBehaviour;
    public UnityAndGeminiV3 geminiScript;

    private bool promptSent = false;

    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        if (promptSent) return;
        if (geminiScript == null) return;

        if (targetStatus.Status == Status.TRACKED ||
            targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            string targetName = behaviour.TargetName;
            string prompt = $"Tell me something about {targetName} in 100 words.";
            Debug.Log($"[Vuforia] prompt: {prompt}");

            geminiScript.userMessage = prompt;
            promptSent = true;         // Lock the prompt to prevent multiple sends
            // Automatically send the chat message
            // geminiScript.SendChat();
        }
    }

    void OnDestroy()
    {
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }
}
