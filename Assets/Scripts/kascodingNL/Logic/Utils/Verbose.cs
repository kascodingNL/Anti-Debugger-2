using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

class Verbose : MonoBehaviour
{
    private int needed;
    private int flags;
    private float resetProgress;
    private float resetTime;
    /// <summary>
    /// Initializer
    /// </summary>
    /// <param name="needed">How much verboses are needed before penalty?</param>
    /// <param name="resetTime">What is the reset time interval(seconds!)</param>
    public Verbose(int needed, float resetTime)
    {
        this.needed = needed;
        this.resetTime = resetTime;
    }

    ///<summary>How much flags need to be added</summary>///
    public bool flag(int toAdd)
    {
        flags+=toAdd;

        return flags >= needed;
    }

    public int getVerbose()
    {
        return flags;
    }

    private void Update()
    {
        resetProgress += Time.deltaTime;

        if(resetProgress >= resetTime)
        {
            flags = 0;
        }
    }
}
