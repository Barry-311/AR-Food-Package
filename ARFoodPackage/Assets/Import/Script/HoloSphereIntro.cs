using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class HoloSphereIntro : MonoBehaviour
{
    [Header("����ʱ�䣨�룩")]
    public float scaleDuration = 1.5f;
    [Header("��ת���ĽǶȣ��ȣ�")]
    public float targetYAngle = 135f;
    [Header("��תʱ�䣨�룩")]
    public float rotateDuration = 2f;

    // ����״̬
    private enum State { Idle, Scaling, Rotating }
    private State state = State.Idle;

    private float elapsedTime = 0f;
    private float startYAngle = 0f;

    // ����ê��
    private Vector3 bottomCenterLocal;

    // �����ʼ���ر任
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;

    void Awake()
    {
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
    }

    void OnEnable()
    {
        RestartIntro();
    }

    void OnDisable()
    {
        state = State.Idle;
    }

    /// <summary>
    /// ���õ���ʼ״̬�����������Ž׶�
    /// </summary>
    public void RestartIntro()
    {
        // �ָ�����λ��/��ת/����
        transform.localPosition = initialLocalPos;
        transform.localRotation = initialLocalRot;
        transform.localScale = Vector3.zero;

        // ���������ռ�İ�Χ�еײ����ģ�Ȼ��ת�������ؿռ�
        Vector3 bottomCenterWS = ComputeBottomCenterWS();
        bottomCenterLocal = transform.InverseTransformPoint(bottomCenterWS);

        elapsedTime = 0f;
        state = State.Scaling;
    }

    void Update()
    {
        if (state == State.Idle) return;

        // �༭����Ҳǿ��ˢ�� Scene ��ͼ
#if UNITY_EDITOR
        if (!Application.isPlaying)
            SceneView.RepaintAll();
#endif

        // ���Ž׶�
        if (state == State.Scaling)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / scaleDuration);
            float f = Mathf.SmoothStep(0f, 1f, t);
            ScaleAroundLocalPoint(f, bottomCenterLocal);

            if (t >= 1f)
            {
                state = State.Rotating;
                elapsedTime = 0f;
                startYAngle = transform.localEulerAngles.y;
            }
        }
        // ��ת�׶�
        else if (state == State.Rotating)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rotateDuration);
            float f = Mathf.SmoothStep(0f, 1f, t);
            float currentY = Mathf.LerpAngle(startYAngle, startYAngle + targetYAngle, f);
            float delta = currentY - transform.localEulerAngles.y;
            RotateAroundLocalPoint(delta);

            if (t >= 1f)
                state = State.Idle;
        }
    }

    // -------- ���߷��� --------

    // ���������� Renderer �����Χ�е�������
    Vector3 ComputeBottomCenterWS()
    {
        var rends = GetComponentsInChildren<Renderer>();
        if (rends.Length == 0)
            return transform.position;

        var b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            b.Encapsulate(rends[i].bounds);

        return b.center - Vector3.up * b.extents.y;
    }

    // Χ�Ʊ��� pivotLocal ����
    void ScaleAroundLocalPoint(float s, Vector3 pivotLocal)
    {
        Vector3 oldScale = transform.localScale;
        Vector3 newScale = Vector3.one * s;

        Vector3 dir = transform.localPosition - pivotLocal;
        Vector3 scaledDir = new Vector3(
            dir.x * (newScale.x / (oldScale.x > 0 ? oldScale.x : 1)),
            dir.y * (newScale.y / (oldScale.y > 0 ? oldScale.y : 1)),
            dir.z * (newScale.z / (oldScale.z > 0 ? oldScale.z : 1))
        );

        transform.localPosition = pivotLocal + scaledDir;
        transform.localScale = newScale;
    }

    // Χ�Ʊ��� pivotLocal �� Y ����ת deltaAngle ��
    void RotateAroundLocalPoint(float deltaAngle)
    {
        Vector3 pivot = bottomCenterLocal;
        Vector3 pos = transform.localPosition;
        Vector3 dir = pos - pivot;

        dir = Quaternion.Euler(0, deltaAngle, 0) * dir;
        transform.localPosition = pivot + dir;

        transform.localRotation = Quaternion.Euler(0, deltaAngle, 0) * transform.localRotation;
    }
}
