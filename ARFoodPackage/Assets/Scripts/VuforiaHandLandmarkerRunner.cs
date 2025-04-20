using UnityEngine;
using Vuforia;
using Mediapipe;
using Mediapipe.Unity;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using Mediapipe.Unity.Sample;
using System.Collections;
using System;

public class VuforiaHandLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
{
    public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig();

    private Mediapipe.Unity.Experimental.TextureFramePool _textureFramePool;
    private Texture2D cameraTexture;
    private Color32[] pixelBuffer;

    public override void Stop()
    {
        base.Stop();
        _textureFramePool?.Dispose();
        _textureFramePool = null;
    }

    IEnumerator WaitForVuforiaVideoTexture()
    {
        while (VuforiaBehaviour.Instance.VideoBackground.VideoBackgroundTexture == null)
        {
            Debug.Log("Waiting for Vuforia Video Background Texture...");
            yield return null;
        }

        Debug.Log("Vuforia Video Background Texture is ready.");
    }

    protected override IEnumerator Run()
    {
        yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

        var options = config.GetHandLandmarkerOptions(OnHandLandmarkDetectionOutput);
        taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);

        yield return WaitForVuforiaVideoTexture();


        // ��Vuforia��ʼ������ͷ����
        var bgTexture = VuforiaBehaviour.Instance.VideoBackground.VideoBackgroundTexture;

        RenderTexture rt = new RenderTexture(bgTexture.width, bgTexture.height, 0, RenderTextureFormat.ARGB32);
        cameraTexture = new Texture2D(bgTexture.width, bgTexture.height, TextureFormat.RGBA32, false);
        pixelBuffer = new Color32[bgTexture.width * bgTexture.height];
        _textureFramePool = new Mediapipe.Unity.Experimental.TextureFramePool(cameraTexture.width, cameraTexture.height, TextureFormat.RGBA32, 5);

        var waitForEndOfFrame = new WaitForEndOfFrame();

        while (true)
        {
            if (isPaused)
            {
                yield return new WaitWhile(() => isPaused);
            }

            yield return waitForEndOfFrame;

            // ��ȡVuforia��ǰ��Ƶ��������
            if (VuforiaBehaviour.Instance.VideoBackground.VideoBackgroundTexture == null)
            {
                Debug.LogWarning("Vuforia Video Background Texture is null");
                continue;
            }

            // ʹ�� Graphics.Blit ��RenderTextureת����Texture2D
            Graphics.Blit(VuforiaBehaviour.Instance.VideoBackground.VideoBackgroundTexture, rt);

            RenderTexture.active = rt;
            cameraTexture.ReadPixels(new UnityEngine.Rect(0, 0, rt.width, rt.height), 0, 0);
            cameraTexture.Apply();
            RenderTexture.active = null;

            if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                continue;

            textureFrame.ReadTextureOnCPU(cameraTexture);
            var image = textureFrame.BuildCPUImage();
            textureFrame.Release();

            taskApi.DetectAsync(image, GetCurrentTimestampMillisec());
        }
    }

    private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Mediapipe.Image image, long timestamp)
    {
        // ���ж������Ƿ����
        if (result.handedness != null && result.handedness.Count > 0)
        {
            Debug.Log("�����Ѽ�⵽��");
        }
    }

}
