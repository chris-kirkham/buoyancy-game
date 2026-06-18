public static class MathsUtils
{
    /// <summary>
    /// Modulo function which deals with negative numbers 
    /// </summary>
    public static float Mod(float x, float m)
    {
        if(m < 0)
        {
            m = -m;
        }

        var r = x % m;
        
        if(r < 0)
        {
            r += m;
        }
        
        return r;
    }
}
