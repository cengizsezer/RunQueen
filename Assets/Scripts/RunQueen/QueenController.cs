using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

public class QueenController : Singleton<QueenController>
{
    public float QueenSpeed;
    public float HorizontalSpeed;
    private const string H = "Horizontal";
    public GameObject Throne;

    public GameObject Queen;

    public float Sensitivity = 5;
    public float HorizontalRange = 5;

    public Transform CameraPivot;

    public List<Rigidbody> RagdollRb = new List<Rigidbody>();
    GameController _gameController;
    float deltaX;

    private void Start()
    {
        LeanTouch.OnFingerUpdate += OnTouchScreen;
        _gameController = GameController.request();

        _gameController.OnGameLost.AddListener(OpenRagdoll);
        Rigidbody[] rb = Queen.GetComponentsInChildren<Rigidbody>();

        for (int i = 0; i < rb.Length ; i++)
        {
            RagdollRb.Add(rb[i]);
        }
    }

    public void OnTouchScreen(LeanFinger leanFinger)
    {
        if (!_gameController.IsGameStarted) return;

        float slideDistance = leanFinger.ScreenDelta.normalized.x * Sensitivity * Time.deltaTime;

        deltaX = transform.position.x + slideDistance;

        deltaX = Mathf.Clamp(deltaX, -1 * HorizontalRange/2, HorizontalRange/2);
    }

    private void Update()
    {
        

       
        Vector3 pivotPoint = new Vector3(0, transform.position.y, transform.position.z);
        CameraPivot.transform.position = pivotPoint;

        if (!_gameController.IsGameStarted) return;

        Vector3 aimPos = transform.position + transform.forward * QueenSpeed * Time.deltaTime; 
        aimPos.x = deltaX;
        transform.position = aimPos;



    }

    public void OpenRagdoll()
    {
        for (int i = 0; i < RagdollRb.Count; i++)
        {
            RagdollRb[i].isKinematic = false;
        }
        Queen.GetComponent<Animator>().enabled = false;
    }

    private void OnDestroy()
    {
        LeanTouch.OnFingerUpdate -= OnTouchScreen;
    }


}
