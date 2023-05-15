using System.Collections;

namespace morrowNet.CellMethods;

public class CellMethods
{
    public static bool IsInterior(dynamic espType)
    {
        var flagByte = new BitArray(new int[] { espType.data.flags });
        return flagByte[0];
    }
}