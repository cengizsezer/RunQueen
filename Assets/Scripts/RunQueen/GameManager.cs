using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<WheelController> RightWhellList;
    public List<WheelController> LeftWhellList;

    public WheelController RightActiveWheel;
    public WheelController LeftActiveWheel;

    private void Start()
    {
        RightActiveWheel = RightWhellList[0];
        LeftActiveWheel = LeftWhellList[0];
    }


   
  

}
