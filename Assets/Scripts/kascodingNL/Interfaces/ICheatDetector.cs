using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class ICheatDetector : MonoBehaviour
{
    public abstract void ClockDesync(int TimeDiff);
    public abstract void InterUpdate(int TimeDiff);
}