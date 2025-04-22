using UnityEngine;
using UnityEngine.UI;

public class RecordToggleButton : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("挂在同一个 GameObject 上的 Button")]
    [SerializeField] private Button toggleButton;
    [Tooltip("切换图标用的 Image 组件")]
    [SerializeField] private Image iconImage;

    [SerializeField] private UnityAndGeminiV3 AI;

    [Header("图标资源")]
    [Tooltip("开始录音时显示的图标")]
    [SerializeField] private Sprite startIcon;
    [Tooltip("结束录音时显示的图标")]
    [SerializeField] private Sprite stopIcon;

    private bool isRecording = false;

    void Awake()
    {
        // 1. 给 Button 添加一个回调，所有逻辑都放到同一个方法里
        toggleButton.onClick.AddListener(OnToggleButtonClicked);

        // 2. 初始时显示“开始录音”图标
        iconImage.sprite = startIcon;
    }

    void OnDestroy()
    {
        toggleButton.onClick.RemoveListener(OnToggleButtonClicked);
    }

    private void OnToggleButtonClicked()
    {
        if (!isRecording)
        {
            // --- 切到“录音中”状态 ---
            isRecording = true;

            // 执行开始录音的逻辑
            AI.StartVoiceInput();


            // 切换图标到“结束说话”
            iconImage.sprite = stopIcon;
        }
        else
        {
            // --- 切到“已停止”状态 ---
            isRecording = false;

            // 执行停止录音并发送的逻辑
            AI.StopVoiceInput();

            // 切回图标到“开始录音”
            iconImage.sprite = startIcon;
        }
    }

}
