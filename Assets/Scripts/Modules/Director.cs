using UnityEngine;

public class Director : MonoBehaviour
{
    public void CameraShake()
    {
        MainCamera.Instance.Shake();
    }
    public void CameraZoomIn()
    {
        MainCamera.Instance.ZoomIn(1.5f, 0.99f, true);
    }
    public void CameraZoomOut()
    {
        MainCamera.Instance.ZoomOut(1.5f, true);
    }
    public void SetDisable()
    {
        gameObject.SetActive(false);
    }
}
