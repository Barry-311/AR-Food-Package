using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ObserverBehaviour))]
public class ShowOnTargetFound : MonoBehaviour
{
    [Tooltip("��⵽ ImageTarget ʱҪ������������")]
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

        // �ҵ�ʱ��TRACKED �� EXTENDED_TRACKED���������ݣ�����״̬������
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
            // Ŀ�궪ʧʱ������ֹͣ������Э��
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
