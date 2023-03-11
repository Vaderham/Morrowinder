using System.Globalization;

namespace morrowNet;

public static class BitChecker
{
    public static bool IsBitSet(byte b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }
}