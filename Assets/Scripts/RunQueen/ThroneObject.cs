using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThroneObject : Singleton<ThroneObject>
{
    public GameObject Queen;
    GameManager _gameManager;
    private Collider RightWheelCollider;
    private Collider LeftWheelCollider;
    public float ThroneOffSet = .5f;

    void Start()
    {
        _gameManager = GameManager.request();
        
    }

    public void ThroneMovement()
    {
        RightWheelCollider = _gameManager.RightActiveWheel.GetComponent<MeshCollider>();
        LeftWheelCollider = _gameManager.LeftActiveWheel.GetComponent<MeshCollider>();

        //POSİTİONS
        Vector3 RightColliderCenter = RightWheelCollider.bounds.center;
        RightColliderCenter.y = RightWheelCollider.bounds.max.y + ThroneOffSet;

        Vector3 LeftColliderCenter = LeftWheelCollider.bounds.center;
        LeftColliderCenter.y = LeftWheelCollider.bounds.max.y + ThroneOffSet;

        Vector3 vector = Vector3.Lerp(RightColliderCenter, LeftColliderCenter, 0.05f);

        transform.position = new Vector3(transform.position.x, vector.y, transform.position.z);
        Queen.transform.position = new Vector3(Queen.transform.position.x, vector.y,Queen.transform.position.z);


        //if(LeftColliderCenter.magnitude > RightColliderCenter.magnitude)
        //Quenn.transform.position = new Vector3(Quenn.transform.position.x + 0.20f, vector.y - 0.20f, Quenn.transform.position.z);
        //else if(LeftColliderCenter.magnitude < RightColliderCenter.magnitude)
        //    Quenn.transform.position = new Vector3(Quenn.transform.position.x - 0.20f, vector.y + 0.20f, Quenn.transform.position.z);

        //ROTATİONS
        Vector3 direction = (RightWheelCollider.bounds.max - LeftWheelCollider.bounds.max).normalized;
        Vector3 upwards = Vector3.Cross(direction, transform.forward); 
        upwards = upwards * -1;

        transform.localRotation = Quaternion.LookRotation(transform.forward, upwards);
        Queen.transform.localRotation = Quaternion.LookRotation(transform.forward, upwards);
    }
}
