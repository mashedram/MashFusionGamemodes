namespace MashGamemodeLibrary.Util;

public static class MathUtil
{
    public static float InverseLerp(float from, float to, float value)
    {
        if (from > to)
            (from, to) = (to, from);

        return (value - from) / (to - from);
    }
    
    public static float Lerp(float from, float to, float value)
    {
        if (from > to)
            (from, to) = (to, from);
            
        return from + value * (to - from);
    }

    public static float Clamp(float value, float min, float max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;

        return value;
    }
}