using System;
using UnityEngine;

class TestDetection : Checks
{

    private void Awake()
    {
    }

    public override void ClockDesync(int TimeDiff)
    {
        Debug.Log(TimeDiff);
    }

    public override void InterUpdate(int TimeDiff)
    {
        Debug.Log("InterUpdate " + TimeDiff);
    }
}
