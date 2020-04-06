using System;
using UnityEngine;

class TestDetection : Checks
{
    [Tooltip("The scene where the player goes to on detection, -1 means that it doesn't do anything!")]
    public int ToSceneId_;

    /*
* Does your class derive from MonoBehaviour? No problemo! The Checks.cs class also derives from MonoBehaviour, so you can just use the functions like normal.
* Do you use a custom MonoBehaviour adoption? Then you will need to implement this in a other class that doesn't have to 
* derive from that specific class!
*/

    private void Awake()
    {
        ToSceneId = ToSceneId_;

        Debug.Log("See? You can just use all things!");

        //You can even request a debugging check!
        RequestDebuggerCheck();
    }

    public override void ClockDesync(int TimeDiff)
    {
    }

    public override void InterUpdate(int TimeDiff)
    {
    }

    public override void DebuggerFound(DateTime timeStamp, int methodId)
    {
    }

    public override void SmoothAim(Vector2 delta)
    {
        //Debug.Log("SmoothAim detected with " + delta);
    }

    public override void MovementSnapshotterUpdate(Quaternion quat)
    {
        Debug.Log(quat);
    }
}
