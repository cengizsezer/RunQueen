using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;

public class CameraController : MonoBehaviour
{
    private const int ACTIVE_VALUE = 10;
    private const int PASSIVE_VALUE = 1;

    public enum CameraStatus
    {
        StartCamera,
        InGameCamera,
        WinCamera,
        LostCamera,
        None
    }

    [System.Serializable]
    public class VirtualCamera
    {
        public CameraStatus CameraStatus;
        public CinemachineVirtualCamera Camera;
    }

    public VirtualCamera[] VirtualCameras;

    [HideInInspector] public CinemachineBrain Brain;

    public CameraStatus CurrentStatus { get; private set; }

    public Camera MainCamera { get; private set; }

    void Start()
    {
        CurrentStatus = CameraStatus.StartCamera;
        SetCameraStatus(CurrentStatus);

        MainCamera = Camera.main;
        Brain = MainCamera.GetComponent<CinemachineBrain>();

        //Camera change calls are added directly in here but according to design they can be changed!
        GameController.Instance.OnGameStarted.AddListener((() => SetCameraStatus(CameraStatus.InGameCamera)));
        GameController.Instance.OnNewLevelLoaded.AddListener((()=>SetCameraStatus(CameraStatus.StartCamera)));
        GameController.Instance.OnGameRestarted.AddListener((()=> SetCameraStatus(CameraStatus.StartCamera)));
        GameController.Instance.OnGameWin.AddListener((() => SetCameraStatus(CameraStatus.WinCamera)));
        GameController.Instance.OnGameLost.AddListener((() => SetCameraStatus(CameraStatus.LostCamera)));
       

    }

    [Button("Set Camera", ButtonStyle.Box)]
    public void SetCameraStatus(CameraStatus status)
    {
        foreach (var virtualCamera in VirtualCameras)
        {
            if (virtualCamera.CameraStatus == status)
            {
                virtualCamera.Camera.Priority = ACTIVE_VALUE;
            }
            else
            {
                virtualCamera.Camera.Priority = PASSIVE_VALUE;
            }
        }

        CurrentStatus = status;
    }



    /// <summary>
    /// Returns whether or not world position can be seen by main camera.
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public static bool IsPositionInCameraField(Vector3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        if (screenPos.x < Screen.width && screenPos.x > 0 && screenPos.y < Screen.height && screenPos.y > 0)
            return true;
        return false;
    }

}
