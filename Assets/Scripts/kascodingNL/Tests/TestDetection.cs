using System;
using UnityEngine;

class TestDetection : Checks
{
    /*
     * Does your class derive from MonoBehaviour? No problemo! The Checks.cs class also derives from MonoBehaviour, so you can just use the functions like normal.
     * Do you use a custom MonoBehaviour adoption? Then you will need to implement this in a other class that doesn't have to 
     * derive from that specific class!
     */

    private void Awake()
    {
        Debug.Log("See? You can just use all things!");

        //You can even request a debugging check!
        RequestDebuggerCheck();
    }

    public override void ClockDesync(int TimeDiff)
    {
        Debug.Log(TimeDiff);
    }

    public override void InterUpdate(int TimeDiff)
    {
        Debug.Log("InterUpdate " + TimeDiff);
    }

    public override void DebuggerFound(DateTime timeStamp, int methodId)
    {
        Debug.Log(string.Format("TimeStamp: {0}, methodId: {1}", timeStamp, methodId));
    }
}
