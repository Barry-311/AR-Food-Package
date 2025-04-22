using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ObserverBehaviour))]
public class ShowOnTargetFound : MonoBehaviour
{
    [Tooltip("检测到 ImageTarget 时要激活的整个组件")]
    [SerializeField] private ParticleSystem particleSys;
    [SerializeField] private FingertipUIButtonSystem handDetectionSys;

    ObserverBehaviour observer;
    private Coroutine playRoutine;
    private bool playedOnce = false;
    private Canvas canvas = null;

    void Awake()
    {
        observer = GetComponent<ObserverBehaviour>();
        observer.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    void OnDestroy()
    {
        observer.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    void OnTargetStatusChanged(ObserverBehaviour obs, TargetStatus status)
    {
        // 找到时（TRACKED 或 EXTENDED_TRACKED）激活内容，其它状态则隐藏
        bool isActive = status.Status == Status.TRACKED;
        if(isActive)
        { 
            Canvas obsCanvas = obs.GetComponentInChildren<Canvas>(true);
            if(canvas != obsCanvas) // new canvas
            {
                canvas = obsCanvas;
                handDetectionSys.RefreshCanvasButtons(canvas);
            }
        }

        if (playedOnce)
            return;

        // only play 5 seconds
        if (isActive)
        {
            if (playRoutine == null)
            {
                playRoutine = StartCoroutine(PlayForSeconds(3f));
            }
        }
        else
        {
            // 目标丢失时，立即停止并清理协程
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }
            particleSys.Stop();
        }
    }

    private IEnumerator PlayForSeconds(float duration)
    {
        particleSys.Play();
        yield return new WaitForSeconds(duration);
        particleSys.Stop();
        playRoutine = null;
        playedOnce = true;
    }
}
