using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class HoloSphereIntro : MonoBehaviour
{
    [Header("缩放时间（秒）")]
    public float scaleDuration = 1.5f;
    [Header("旋转到的角度（度）")]
    public float targetYAngle = 135f;
    [Header("旋转时间（秒）")]
    public float rotateDuration = 2f;

    // 动画状态
    private enum State { Idle, Scaling, Rotating }
    private State state = State.Idle;

    private float elapsedTime = 0f;
    private float startYAngle = 0f;

    // 本地锚点
    private Vector3 bottomCenterLocal;

    // 缓存初始本地变换
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
    /// 重置到初始状态，并进入缩放阶段
    /// </summary>
    public void RestartIntro()
    {
        // 恢复本地位置/旋转/缩放
        transform.localPosition = initialLocalPos;
        transform.localRotation = initialLocalRot;
        transform.localScale = Vector3.zero;

        // 先算出世界空间的包围盒底部中心，然后转换到本地空间
        Vector3 bottomCenterWS = ComputeBottomCenterWS();
        bottomCenterLocal = transform.InverseTransformPoint(bottomCenterWS);

        elapsedTime = 0f;
        state = State.Scaling;
    }

    void Update()
    {
        if (state == State.Idle) return;

        // 编辑器下也强制刷新 Scene 视图
#if UNITY_EDITOR
        if (!Application.isPlaying)
            SceneView.RepaintAll();
#endif

        // 缩放阶段
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
        // 旋转阶段
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

    // -------- 工具方法 --------

    // 计算所有子 Renderer 世界包围盒底面中心
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

    // 围绕本地 pivotLocal 缩放
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

    // 围绕本地 pivotLocal 绕 Y 轴旋转 deltaAngle 度
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
