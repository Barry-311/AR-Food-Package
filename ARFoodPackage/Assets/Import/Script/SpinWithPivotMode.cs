using UnityEngine;

[ExecuteAlways]
public class SpinWithPivotMode : MonoBehaviour
{
    public enum PivotMode { Pivot, Center }
    public enum SpinAxis { X, Y, Z }

    [Header("开关")]
    public bool enableRotation = false;

    [Header("旋转模式")]
    public PivotMode pivotMode = PivotMode.Center;

    [Header("旋转轴")]
    public SpinAxis spinAxis = SpinAxis.Z;

    [Header("速率 (度/秒)，只在选定轴上生效")]
    public float speed = 90f;

    // Center 模式下缓存的世界空间几何中心
    private Vector3 centerPoint;

    void Start()
    {
        UpdateCenterPoint();
    }

    void UpdateCenterPoint()
    {
        var rends = GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);
            centerPoint = b.center;
        }
        else centerPoint = transform.position;
    }

    void Update()
    {
        if (!enableRotation) return;

        float dt = Time.deltaTime;
        if (pivotMode == PivotMode.Center)
            UpdateCenterPoint();

        Vector3 pivot = (pivotMode == PivotMode.Center)
            ? centerPoint
            : transform.position;

        // 根据枚举只对单一轴生效
        float angle = speed * dt;
        switch (spinAxis)
        {
            case SpinAxis.X:
                transform.RotateAround(pivot, transform.right, angle);
                break;
            case SpinAxis.Y:
                transform.RotateAround(pivot, transform.up, angle);
                break;
            case SpinAxis.Z:
                transform.RotateAround(pivot, transform.forward, angle);
                break;
        }
    }
}
