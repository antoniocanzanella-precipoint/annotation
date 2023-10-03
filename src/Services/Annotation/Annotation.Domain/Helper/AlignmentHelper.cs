using System;

namespace PreciPoint.Ims.Services.Annotation.Domain.Helper;

/// <summary>
/// Helper class for aligning byte offsets
/// </summary>
public class AlignmentHelper
{
    /// <summary>
    /// Aligns the offset to the next 4 byte address
    /// </summary>
    /// <param name="offset">The offset that should be aligned</param>
    /// <returns>The offset aligned to the next 4 byte address</returns>
    public static int AlignTo4ByteOffset(int offset)
    {
        if (offset % 4 != 0)
        {
            // finds the next highest number for currLen, that is divisible by 4
            return (int) Math.Ceiling((double) offset / 4) * 4;
        }

        return offset;
    }
}