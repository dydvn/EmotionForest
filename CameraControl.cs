using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControl : MonoBehaviour
{
    private enum coordinate
    {
        XY,
        XZ,
        YZ
    }
    [SerializeField] private coordinate coordi = coordinate.XY;
    [SerializeField] private GameObject goTarget;
    [SerializeField] private bool isMyRoom = false;
    [SerializeField] private float cameraMoveSpeed = 0.01f;
    [SerializeField] private float cameraZoomSpeed = 0.01f;


    public Vector3 min;
    public Vector3 max;

    private Camera mainCam;
    private Vector3 beginMousePos = Vector3.zero;
    private Vector3 preMousePos = Vector3.zero;
    private Vector2 beginTouchPos = Vector2.zero;
    private Vector2 preTouchPos = Vector2.zero;
    private Vector3 beginCamPos = Vector3.zero;
    private Vector3 newCamPos = Vector3.zero;
    private float camZoom;

    public GameObject goTouchBlockCover;
    public Transform targetTransform;
    public bool isFollowUp = false;

    private void Start()
    {
        camZoom = 60;
        mainCam = Camera.main;
        newCamPos = transform.position;
        max.x = (int)MaxValue.moodMoveRangeX;
        min.x = (int)MinValue.moodMoveRangeX;

#if UNITY_EDITOR
        cameraMoveSpeed = 0.05f;
#endif
    }

    private void Update()
    {
        if (isFollowUp)
        {
            var targetPos = new Vector3(targetTransform.position.x, 200, targetTransform.position.z - 370);
            camZoom = Mathf.Lerp(mainCam.orthographicSize, 20, Time.deltaTime * 3);
            mainCam.orthographicSize = camZoom;
            newCamPos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 3);
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
                isFollowUp = false;
            
        }
#elif UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            if (!EventSystem.current.IsPointerOverGameObject(0) && Input.GetTouch(0).phase == TouchPhase.Began)
                isFollowUp = false;
        }
#endif

        CameraMove();
        CamMoveLimit();
    }

    private void CameraMove()
    {
        if (isFollowUp)
            return;


#if UNITY_EDITOR

        if (EventSystem.current.IsPointerOverGameObject() || (isMyRoom && Manager_Building.Instance.isTouchGrabedBuilding))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            beginMousePos = Input.mousePosition;
            beginCamPos = transform.position;
        }
        else if (Input.GetMouseButton(0))
        {
            cameraMoveSpeed = camZoom / 1200;

            switch (coordi)
            {
                case coordinate.XY:
                    preMousePos = -(Input.mousePosition - beginMousePos) * cameraMoveSpeed;
                    break;
                case coordinate.XZ:
                    preMousePos.x = (-(Input.mousePosition - beginMousePos) * cameraMoveSpeed).x;
                    preMousePos.z = (-(Input.mousePosition - beginMousePos) * cameraMoveSpeed).y;
                    break;
                case coordinate.YZ:
                    preMousePos.y = (-(Input.mousePosition - beginMousePos) * cameraMoveSpeed).x;
                    preMousePos.z = (-(Input.mousePosition - beginMousePos) * cameraMoveSpeed).y;
                    break;
            }

            newCamPos = beginCamPos + preMousePos;
        }

        // zoon in/out
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            camZoom -= (int)ConstValue.zoomSensitivity;
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            camZoom += (int)ConstValue.zoomSensitivity;


#elif UNITY_ANDROID

        // 여기다가 터치구현
        if (Input.touchCount > 0)
        {
            if (EventSystem.current.IsPointerOverGameObject(0) || (isMyRoom && Manager_Building.Instance.isTouchGrabedBuilding))
                return;
        }

        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                beginTouchPos = Input.GetTouch(0).position;
                beginCamPos = transform.position;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                cameraMoveSpeed = camZoom / 1000;

                switch (coordi)
                {
                    case coordinate.XY:
                        preMousePos = -(Input.GetTouch(0).position - beginTouchPos) * cameraMoveSpeed;
                        break;
                    case coordinate.XZ:
                        preMousePos.x = (-(Input.GetTouch(0).position - beginTouchPos) * cameraMoveSpeed).x;
                        preMousePos.z = (-(Input.GetTouch(0).position - beginTouchPos) * cameraMoveSpeed).y;
                        break;
                    case coordinate.YZ:
                        preMousePos.y = (-(Input.GetTouch(0).position - beginTouchPos) * cameraMoveSpeed).x;
                        preMousePos.z = (-(Input.GetTouch(0).position - beginTouchPos) * cameraMoveSpeed).y;
                        break;
                }

                newCamPos = beginCamPos + preMousePos;
            }
        }
        else if (Input.touchCount == 2) // 손가락 2개가 눌렸을 때
        {
            Touch touchZero = Input.GetTouch(0); // 첫번째 손가락 터치를 저장
            Touch touchOne = Input.GetTouch(1); // 두번째 손가락 터치를 저장

            // 터치에 대한 이전 위치값을 각각 저장함
            // 처음 터치한 위치(touchZero.position)에서 이전 프레임에서의 터치 위치와 이번 프레임에서 터치 위치의 차이를 뺌
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition; //deltaPosition는 이동방향 추적할 때 사용
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // 각 프레임에서 터치 사이의 벡터 거리 구함
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude; //magnitude는 두 점간의 거리 비교(벡터)
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // 거리 차이 구함(거리가 이전보다 크면(마이너스가 나오면)손가락을 벌린 상태_줌인 상태)
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;


            // zoon in/out
            camZoom += deltaMagnitudeDiff * cameraZoomSpeed;

            camZoom = Mathf.Clamp(camZoom, (int)MinValue.camZoom, (int)MaxValue.camZoom);
            mainCam.orthographicSize = camZoom;
        }
#endif
    }

    private void CamMoveLimit()
    {
        switch (coordi)
        {
            case coordinate.XY:
                break;
            case coordinate.XZ:
                newCamPos.x = Mathf.Clamp(newCamPos.x, mainCam.ViewportToWorldPoint(new Vector2(mainCam.WorldToViewportPoint(min).x + 0.6f, 0)).x, mainCam.ViewportToWorldPoint(new Vector2(mainCam.WorldToViewportPoint(max).x - 0.6f, 0)).x);
                newCamPos.z = Mathf.Clamp(newCamPos.z, mainCam.ViewportToWorldPoint(new Vector2(0, mainCam.WorldToViewportPoint(min).y + 0.6f)).z, mainCam.ViewportToWorldPoint(new Vector2(0, mainCam.WorldToViewportPoint(max).y - 0.6f)).z);
                break;
            case coordinate.YZ:
                break;
        }
        // pos
        newCamPos.y = 200;
        transform.position = newCamPos;

        // zoom
        float maxVal = (Screen.height + 0.0f) / (Screen.width + 0.0f) * (int)MaxValue.camZoom;
        maxVal = maxVal > (int)MaxValue.camZoom ? (int)MaxValue.camZoom : maxVal;
        camZoom = Mathf.Clamp(camZoom, (int)MinValue.camZoom, maxVal);
        mainCam.orthographicSize = camZoom;
    }
}
