using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ObserverBehaviour))]
public class ShowOnTargetFound : MonoBehaviour
{
    [Tooltip("检测到 ImageTarget 时要激活的整个组件")]
    [SerializeField] private ParticleSystem particleSystem;

    ObserverBehaviour observer;
    private Coroutine playRoutine;
    private bool playedOnce = false;

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
        if(playedOnce)
            return;

        // 找到时（TRACKED 或 EXTENDED_TRACKED）激活内容，其它状态则隐藏
        bool isFound = status.Status == Status.TRACKED;
        // only paly 5 seconds
        if (isFound)
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
            particleSystem.Stop();
        }
    }

    private IEnumerator PlayForSeconds(float duration)
    {
        particleSystem.Play();
        yield return new WaitForSeconds(duration);
        particleSystem.Stop();
        playRoutine = null;
        playedOnce = true;
    }
}
