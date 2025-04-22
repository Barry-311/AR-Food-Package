using UnityEngine;

public class FaceARCamera : MonoBehaviour
{
    private Transform arCameraTransform;

    void Start()
    {
        GameObject arCamera = GameObject.Find("ARCamera");
        if (arCamera != null)
        {
            arCameraTransform = arCamera.transform;
        }
        else
        {
            Debug.LogError("ARCamera not found! Make sure it's named 'ARCamera' in the scene.");
        }
    }

    void LateUpdate()
    {
        if (arCameraTransform != null)
        {
            transform.LookAt(transform.position + arCameraTransform.forward, arCameraTransform.up);
        }
    }
}
