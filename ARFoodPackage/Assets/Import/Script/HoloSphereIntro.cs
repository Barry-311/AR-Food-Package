using UnityEngine;
using System.Collections;

public class HoloSphereIntro : MonoBehaviour
{
    [Header("缩放时间（秒）")]
    public float scaleDuration = 1.5f;
    [Header("旋转到的角度（度）")]
    public float targetYAngle = 135f;
    [Header("旋转时间（秒）")]
    public float rotateDuration = 2f;

    // 包围盒底部中心（世界坐标）
    private Vector3 bottomCenterWS;

    void Start()
    {
        // 1) 计算所有子孙 Renderer 的世界包围盒
        var rends = GetComponentsInChildren<Renderer>();
        if (rends.Length == 0)
        {
            Debug.LogWarning("没有找到任何子 Renderer，动画点将使用自身位置。");
            bottomCenterWS = transform.position;
        }
        else
        {
            var b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);
            // 包围盒底面中心 = b.center 向下 b.extents.y
            bottomCenterWS = b.center - Vector3.up * b.extents.y;
        }

        // 一开始缩到 0
        transform.localScale = Vector3.zero;
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        // 2) 缩放动画
        float t = 0f;
        while (t < scaleDuration)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, t / scaleDuration);
            ScaleAroundPoint(f, bottomCenterWS);
            yield return null;
        }
        ScaleAroundPoint(1f, bottomCenterWS);

        // 3) 旋转动画
        float elapsed = 0f;
        float startY = transform.eulerAngles.y;
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, elapsed / rotateDuration);
            float currentY = Mathf.LerpAngle(startY, startY + targetYAngle, f);
            RotateAroundPoint(Vector3.up, currentY - transform.eulerAngles.y);
            yield return null;
        }
        // 保证停在精确值
        RotateAroundPoint(Vector3.up, (startY + targetYAngle) - transform.eulerAngles.y);
    }

    // 把 transform.localScale 设为 (s,s,s)，同时修正 position 使 point 不动
    void ScaleAroundPoint(float s, Vector3 pointWS)
    {
        Vector3 oldScale = transform.localScale;
        Vector3 newScale = Vector3.one * s;

        // 先算出当前位置到锚点的向量
        Vector3 dir = transform.position - pointWS;
        // 缩放该向量
        Vector3 scaledDir = new Vector3(
            dir.x * (newScale.x / (oldScale.x > 0 ? oldScale.x : 1)),
            dir.y * (newScale.y / (oldScale.y > 0 ? oldScale.y : 1)),
            dir.z * (newScale.z / (oldScale.z > 0 ? oldScale.z : 1))
        );
        transform.position = pointWS + scaledDir;
        transform.localScale = newScale;
    }

    // 绕锚点 pointWS 旋转 deltaAngle 度（本地 transform 轴）
    void RotateAroundPoint(Vector3 axisLocal, float deltaAngle)
    {
        transform.RotateAround(bottomCenterWS, transform.TransformDirection(axisLocal), deltaAngle);
    }
}