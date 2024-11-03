using System;
using System.Collections;
using System.Collections.Generic;

public class Utils
{
    public static double MapValue(double value, double fromSource, double toSource, double fromTarget, double toTarget)
    {
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }

    public static bool ChanceByPercent(int percent)
    {
        if (percent > 100)
        {
            return false;
        }

        Random random = new();
        int randomNumber = random.Next(0, 101);

        return randomNumber <= percent;
    }
}
