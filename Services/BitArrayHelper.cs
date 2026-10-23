using System.Collections;
using System.Numerics;

namespace ColorChanger.Services;

internal static class BitArrayHelper
{
    /// <summary>
    /// 空のBitArrayを取得する
    /// </summary>
    internal static BitArray Empty()
        => new BitArray(0);

    /// <summary>
    /// 指定された値のビットの個数を取得する
    /// </summary>
    internal static int CountOf(BitArray bits, bool value = true)
    {
        int[] words = ToWords(bits);
        int fullWords = bits.Length / 32;
        int remainingBits = bits.Length % 32;

        int ones = 0;
        for (int i = 0; i < fullWords; i++)
        {
            ones += BitOperations.PopCount((uint)words[i]);
        }

        if (remainingBits > 0)
        {
            int mask = (1 << remainingBits) - 1;
            ones += BitOperations.PopCount((uint)(words[fullWords] & mask));
        }

        return value ? ones : bits.Length - ones;
    }

    /// <summary>
    /// BitArrayの内容が一致するかを比較する
    /// </summary>
    internal static bool AreEqual(BitArray bits1, BitArray bits2)
    {
        if (bits1.Length != bits2.Length) return false;

        return ToWords(bits1).AsSpan().SequenceEqual(ToWords(bits2));
    }

    /// <summary>
    /// 2つの選択範囲を合成した新しいBitArrayを返す。isEraserの場合は target から source の範囲を削除する。
    /// </summary>
    internal static BitArray Merge(BitArray target, BitArray source, bool isEraser)
    {
        if (target.Length == 0) target = new BitArray(source.Length);

        BitArray result = new BitArray(target);
        if (isEraser)
        {
            result.And(new BitArray(source).Not());
        }
        else
        {
            result.Or(source);
        }

        return result;
    }

    private static int[] ToWords(BitArray bits)
    {
        int[] words = new int[(bits.Length + 31) / 32];
        bits.CopyTo(words, 0);
        return words;
    }
}
