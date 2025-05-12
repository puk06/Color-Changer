using System.Collections;

namespace ColorChanger.Utils;

internal class BitArrayUtils
{
    /// <summary>
    /// 指定されたBitArrayのvalue値の個数を取得する
    /// </summary>
    /// <param name="bitArray"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static int GetCount(BitArray bitArray, bool value = true)
    {
        int count = 0;

        for (int i = 0; i < bitArray.Count; i++)
        {
            if (bitArray[i] != value) continue;
            count++;
        }

        return count;
    }

    /// <summary>
    /// 空のBitArrayを取得する
    /// </summary>
    /// <returns></returns>
    internal static BitArray GetEmpty()
        => new BitArray(0);

    /// <summary>
    /// 指定されたBitArrayのクローンを取得する
    /// </summary>
    /// <param name="bitArray"></param>
    /// <returns></returns>
    internal static BitArray GetClone(BitArray bitArray)
        => new BitArray(bitArray);
}
