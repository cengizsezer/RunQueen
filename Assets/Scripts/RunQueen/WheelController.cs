using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum WheelType{Blank, rightWhell, leftWhell};

public class WheelController : MonoBehaviour
{
    public WheelType wheelType;
    public int WheelNumber;
    public int activeWheelIndex;
    public TMPro.TextMeshProUGUI WheelNumberText;

    GameManager _gamaManager;
    GameController _gameController;
    QueenController _queenController;
    ThroneObject _throneObject;

    
    public Transform Queen;
    public GameObject PersonBridgePrefebs;
    public GameObject PersonPrefebs;
    public Animator QueeenAnimation;

    private void Start()
    {
        
        _gamaManager = GameManager.request();
        _gameController = GameController.request();
        _queenController = QueenController.request();
        _throneObject = ThroneObject.request();
    }

    private void Update()
    {
        WheelRotate();
    }

    private void OnTriggerEnter(Collider other)
    {

        activeWheelIndex = -1;
        ObstacleControl obstacleControl = other.gameObject.GetComponent<ObstacleControl>();
        Vector3 collesionPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);


        switch (other.gameObject.layer)
        {
            #region PERSON
            case 8:
                ParticleSystem HeartCollectedParticle = other.gameObject.GetComponent<CollectiblePersonObjcet>().HeartCollectedParticle;
                HeartCollectedParticle.gameObject.SetActive(true);
                HeartCollectedParticle.Play();
                HeartCollectedParticle.gameObject.transform.SetParent(null);
                Destroy(other.gameObject);
                activeWheelIndex = WheelNumber - 1;
                ChangeWheel(wheelType, activeWheelIndex);

                DOVirtual.DelayedCall(3, () =>
                {
                    Destroy(HeartCollectedParticle.gameObject);
                });

                break;
            #endregion

            #region THORN OBSTACLE
            case 9:

                if (wheelType == WheelType.rightWhell && obstacleControl.RightWheelEntered == false)
                {
                    obstacleControl.RightWheelEntered = true;
                    Destroy(other.gameObject);
                    ReduceInWheel(WheelNumber);
                    ChangeWheel(wheelType, activeWheelIndex);
                    InstantiatePerson(PersonPrefebs, collesionPoint);
                    
                }
                else if (wheelType == WheelType.leftWhell && obstacleControl.RightWheelEntered == false)
                {
                    obstacleControl.RightWheelEntered = true;
                    Destroy(other.gameObject);
                    ReduceInWheel(WheelNumber);
                    ChangeWheel(wheelType, activeWheelIndex);
                    InstantiatePerson(PersonPrefebs, collesionPoint);
                   
                }

                
                break;
            #endregion

            #region TRANSİTİON OBSTACLE
            case 10:

                float ofSetX = 1f;
                Vector3 spawnPoint = new Vector3(transform.position.x - ofSetX, 0.2f, collesionPoint.z);

                if (wheelType == WheelType.rightWhell && obstacleControl.RightWheelEntered == false)
                {
                    obstacleControl.RightWheelEntered = true;
                    ReduceInWheel(WheelNumber);
                    ChangeWheel(wheelType, activeWheelIndex);
                    InstantiatePerson(PersonBridgePrefebs, spawnPoint);
                }

                if (wheelType == WheelType.leftWhell && obstacleControl.LeftWheelEntered == false)
                {
                    obstacleControl.LeftWheelEntered = true;
                    ReduceInWheel(WheelNumber);
                    ChangeWheel(wheelType, activeWheelIndex);
                    InstantiatePerson(PersonBridgePrefebs, spawnPoint);

                }

                break;
            #endregion

            #region WALL OBSTACLE
            case 11:


                if (wheelType == WheelType.rightWhell && obstacleControl.RightWheelEntered == false)
                {
                    obstacleControl.RightWheelEntered = true;
                    //GameObject newperson = Instantiate(PersonPrefebs, collesionPoint, PersonPrefebs.transform.rotation) as GameObject;
                    ReduceInWheel(WheelNumber);
                    ChangeWheel(wheelType, activeWheelIndex);
                    InstantiatePerson(PersonPrefebs, collesionPoint);
                    //DOVirtual.DelayedCall(3, () =>
                    //{
                    //    Destroy(newperson.gameObject);
                    //});
                }
                else if (wheelType == WheelType.leftWhell && obstacleControl.LeftWheelEntered == false)
                {
                    obstacleControl.LeftWheelEntered = true;
                    //GameObject newperson = Instantiate(PersonPrefebs, collesionPoint, PersonPrefebs.transform.rotation) as GameObject;
                    ReduceInWheel(WheelNumber);
                    ChangeWheel(wheelType, activeWheelIndex);
                    InstantiatePerson(PersonPrefebs, collesionPoint);
                    //DOVirtual.DelayedCall(3, () =>
                    //{
                    //    Destroy(newperson.gameObject);
                    //});
                }

                break;
            #endregion

            #region FİNİSH

            case 14:

                
                Queen.transform.DOLocalRotate(new Vector3(0, 180, 0), 3f);
                QueeenAnimation.SetTrigger("Dancing");
                _gameController.WinGame();
                break;

            #endregion

            default: break;
        }
    }


    private void InstantiatePerson(GameObject Prefeb,Vector3 Point)
    {
        GameObject newperson = Instantiate(Prefeb, Point, Prefeb.transform.rotation) as GameObject;
        DOVirtual.DelayedCall(3, () =>
        {
            Destroy(newperson.gameObject);
        });
    }

    private void ReduceInWheel(int WheelNumber)
    {


        if (WheelNumber != 2)
        {
            activeWheelIndex = WheelNumber - 3;
        }
        else
        {
            _gameController.LostGame();
        }
    }

    private void ChangeWheel(WheelType wheelType, int activeWheelIndex)
    {

        if(_gameController.IsGameStarted)
        {
            if (wheelType == WheelType.rightWhell)
            {
                _gamaManager.RightWhellList[activeWheelIndex].Activate();
                _gamaManager.RightActiveWheel = _gamaManager.RightWhellList[activeWheelIndex];
                WheelNumberText.text = "" + activeWheelIndex;
                _throneObject.ThroneMovement();

            }
            else if (wheelType == WheelType.leftWhell)
            {
                _gamaManager.LeftWhellList[activeWheelIndex].Activate();
                _gamaManager.LeftActiveWheel = _gamaManager.LeftWhellList[activeWheelIndex];
                WheelNumberText.text = "" + activeWheelIndex;
                _throneObject.ThroneMovement();

            }
        }


        AnimationPosition();
        Deactivate();

        
    }

    public void AnimationPosition()
    {
        float wheelDifference = _gamaManager.RightActiveWheel.WheelNumber - _gamaManager.LeftActiveWheel.WheelNumber;
        Vector3 pos = Queen.transform.localPosition;
        if (_gameController.IsGameStarted == true)
        {
            if (wheelDifference == 0)
            {
                pos.x = 0;
                QueeenAnimation.SetTrigger("Dancing");
            }
            else if (wheelDifference == -1)
            {
                pos.x = 0.25f;
                QueeenAnimation.SetTrigger("Balance_1");
            }
            else if (wheelDifference == 1)
            {
                pos.x = -0.25f;
                QueeenAnimation.SetTrigger("Balance_1");
            }
            else if (wheelDifference == -2)
            {
                pos.x = 0.5f;
                QueeenAnimation.SetTrigger("Balance_2");
            }
            else if (wheelDifference == 2)
            {
                pos.x = -0.5f;
                QueeenAnimation.SetTrigger("Balance_2");
            }
            else if (wheelDifference == -3)
            {
                pos.x = 0.75f;
                QueeenAnimation.SetTrigger("Balance_3");
            }
            else if (wheelDifference == 3)
            {
                pos.x = -0.75f;
                QueeenAnimation.SetTrigger("Balance_3");
            }
            else if (wheelDifference == 4 || wheelDifference == -4)
            {
                _gameController.LostGame();
            }

            Queen.transform.localPosition = pos;
        }

    }

    public void WheelRotate()
    {
        if(_gameController.IsGameStarted==true)
        {
            transform.Rotate(0f, 0f, 10f, Space.Self);
        }
       
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

}

