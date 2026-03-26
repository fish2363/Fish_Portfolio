using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraZoom : MonoBehaviour
{
    private const float ZoomSpeed = 1.0f; // 한번의 줌 입력의 줌 되는 정도
    private const float MinZoomSize = 2.0f; // 최소 카메라 사이즈
    private const float MaxZoomSize =6.0f;
    private float targetZoomSize;
    public CinemachineVirtualCamera playerCamera;
    public Camera plCamera;

    private void Awake()
    {
        //playerCamera = GetComponent<Camera>();
        targetZoomSize = playerCamera.m_Lens.OrthographicSize;
        plCamera = Camera.main;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ControllerZoom();
        UpdateZoom();
    }

    private void ControllerZoom()
    {
        // 마우스 스크롤 입력 받기
        var scrollInput = Input.GetAxis("Mouse ScrollWheel");
        var hasScrollInput = Mathf.Abs(scrollInput) > Mathf.Epsilon;
        if (!hasScrollInput)
        {
            return;
        }

        // 카메라 크기를 마우스 스크롤 입력에 따라 변경하여 확대/축소
        var newSize = playerCamera.m_Lens.OrthographicSize - scrollInput * ZoomSpeed;

        // 카메라 크기 값을 최소값과 최대값 사이로 유지
        targetZoomSize = Mathf.Clamp(newSize, MinZoomSize, MaxZoomSize);
    }

    private void UpdateZoom()
    {
        if (Mathf.Abs(targetZoomSize - playerCamera.m_Lens.OrthographicSize) < Mathf.Epsilon)
        {
            return;
        }

        var mouseWorldPos = plCamera.ScreenToWorldPoint(Input.mousePosition);
        var cameraTransform = transform;
        var currentCameraPosition = cameraTransform.position;
        var offsetCamera = mouseWorldPos - currentCameraPosition - (mouseWorldPos - currentCameraPosition) / (playerCamera.m_Lens.OrthographicSize / targetZoomSize);

        // 카메라 크기 갱신
        playerCamera.m_Lens.OrthographicSize = targetZoomSize;

        // 줌 비율에 의한 카메라 위치 조정
        currentCameraPosition += offsetCamera;
        cameraTransform.position = currentCameraPosition;
    }
}
