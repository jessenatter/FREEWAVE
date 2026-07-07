using UnityEngine;

public class BackgroundCamera : MonoBehaviour
{

    [SerializeField] Material mirror;

    Camera cameraComponent;

    void Start()
    {
        cameraComponent = GetComponent<Camera>();
    }
    void Update()
    {
        mirror.SetMatrix("_CameraVP",cameraComponent.projectionMatrix * cameraComponent.worldToCameraMatrix);
    }
}
