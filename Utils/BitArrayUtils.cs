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
    /// BitArrayをマージする
    /// </summary>
    /// <param name="bitArray1"></param>
    /// <param name="bitArray2"></param>
    internal static void Merge(ref BitArray bitArray1, BitArray bitArray2)
    {
        if (bitArray1.Length == 0) bitArray1 = new BitArray(bitArray2.Length);

        for (int i = 0; i < bitArray2.Count; i++)
        {
            if (bitArray2[i])
            {
                bitArray1[i] = true;
            }
        }
    }

    /// <summary>
    /// BitArrayの内容を比較する
    /// </summary>
    /// <param name="bitArray1"></param>
    /// <param name="bitArray2"></param>
    /// <returns></returns>
    internal static bool Equals(BitArray bitArray1, BitArray bitArray2)
    {
        if (bitArray1.Length != bitArray2.Length) return false;

        for (int i = 0; i < bitArray1.Count; i++)
        {
            if (bitArray1[i] != bitArray2[i])
            {
                return false;
            }
        }

        return true;
    }
}
