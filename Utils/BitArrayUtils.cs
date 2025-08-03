using System.Collections;
using System.Reflection;

internal static unsafe class BitArrayUtils
{
    private static readonly BitArray _emptyBitArray = new BitArray(0);

    /// <summary>
    /// 指定されたBitArrayのvalue値の個数を取得する
    /// </summary>
    /// <param name="bitArray"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static int GetCount(BitArray bitArray, bool value = true)
    {
        var m_arrayField = typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | BindingFlags.Instance);
        int[]? arr = (int[]?)m_arrayField?.GetValue(bitArray);
        if (arr == null || arr.Length == 0) return 0;

        int count = 0;
        int length = bitArray.Length;
        int intLength = arr.Length;

        fixed (int* p = arr)
        {
            int fullInts = length / 32;
            int remainingBits = length % 32;

            for (int i = 0; i < fullInts; i++)
            {
                int bits = p[i];
                count += CountBits(value ? bits : ~bits);
            }

            if (remainingBits > 0)
            {
                int mask = (1 << remainingBits) - 1;
                int bits = p[fullInts] & mask;
                count += CountBits(value ? bits : ~bits & mask);
            }
        }

        return count;
    }

    private static int CountBits(int v)
    {
        // Brian Kernighan's Bit Counting Algorithm
        // Reference: https://yuminlee2.medium.com/brian-kernighans-algorithm-count-set-bits-in-a-number-18ab05edca93
        int c = 0;
        while (v != 0)
        {
            v &= v - 1;
            c++;
        }
        return c;
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

        var m_arrayField = typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | BindingFlags.Instance);

        int[]? arr1 = (int[]?)m_arrayField?.GetValue(bitArray1);
        int[]? arr2 = (int[]?)m_arrayField?.GetValue(bitArray2);

        if (arr1 == null || arr2 == null) return false;

        int length = (bitArray1.Length + 31) / 32;

        fixed (int* p1 = arr1)
        fixed (int* p2 = arr2)
        {
            for (int i = 0; i < length; i++)
            {
                if (p1[i] != p2[i]) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// BitArrayをマージする。Eraserレイヤーの場合はマージ時に削除する。
    /// </summary>
    /// <param name="bitArray1"></param>
    /// <param name="bitArray2"></param>
    /// <param name="isEraser"></param>
    internal static void Merge(ref BitArray bitArray1, BitArray bitArray2, bool isEraser = false)
    {
        if (bitArray1.Length == 0) bitArray1 = new BitArray(bitArray2.Length);

        var m_arrayField = typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | BindingFlags.Instance);

        int[]? arrSrc = (int[]?)m_arrayField?.GetValue(bitArray2);
        int[]? arrDst = (int[]?)m_arrayField?.GetValue(bitArray1);

        if (arrSrc == null || arrDst == null) return;

        int length = (bitArray2.Length + 31) / 32;

        fixed (int* pSrc = arrSrc)
        fixed (int* pDst = arrDst)
        {
            if (isEraser)
            {
                for (int i = 0; i < length; i++)
                {
                    pDst[i] &= ~pSrc[i];
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    pDst[i] |= pSrc[i];
                }
            }
        }
    }

    /// <summary>
    /// 空のBitArrayを取得する
    /// </summary>
    /// <returns></returns>
    internal static BitArray GetEmpty()
        => _emptyBitArray;
}