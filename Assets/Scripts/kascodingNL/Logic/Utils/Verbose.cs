using System;
using System.Collections.Generic;

class Verbose
{
    private int needed;
    private int flags;

    public Verbose(int needed)
    {
        this.needed = needed;
    }

    public bool flag(int toAdd)
    {
        flags+=toAdd;

        return flags >= needed;
    }

    public int getVerbose()
    {
        return flags;
    }
}
