using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public static class IDGenerator
{
    /// ID 생성에 사용할 문자 집합 (0, O, I, L 제외)
    private const string IdChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static readonly int BaseLength = IdChars.Length;

    // Decode 성능 최적화용
    private static readonly Dictionary<char, int> CharToValueMap = CreateCharMap();

    private static Dictionary<char, int> CreateCharMap()
    {
        var dict = new Dictionary<char, int>(BaseLength);
        for (int i = 0; i < BaseLength; i++)
        {
            dict[IdChars[i]] = i;
        }
        return dict;
    }

    /// <summary>
    /// 지정 길이의 랜덤 ID 생성
    /// </summary>
    public static string GenerateRandom(int length)
    {
        char[] chars = new char[length];

        for (int i = 0; i < length; i++)
        {
            chars[i] = IdChars[UnityEngine.Random.Range(0, BaseLength)];
        }

        return new string(chars);
    }

    /// <summary>
    /// 문자열을 해시 기반으로 고정 길이 ID로 변환
    /// </summary>
    public static string GenerateFromHash(string input, int length)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

        return EncodeBytes(hash, length);
    }

    /// <summary>
    /// ulong 값을 커스텀 base 문자열로 변환 (SteamID 압축용)
    /// </summary>
    public static string EncodeBase(ulong value, int fixedLength = 13)
    {
        char[] buffer = new char[fixedLength];

        for (int i = fixedLength - 1; i >= 0; i--)
        {
            if (value > 0)
            {
                int index = (int)(value % (ulong)BaseLength);
                buffer[i] = IdChars[index];
                value /= (ulong)BaseLength;
            }
            else
            {
                buffer[i] = IdChars[0]; // 'A'
            }
        }

        return new string(buffer);
    }

    /// <summary>
    /// 커스텀 base 문자열을 ulong으로 복원 (잘못된 값은 예외 발생)
    /// </summary>
    public static ulong DecodeBase(string encoded)
    {
        if (string.IsNullOrEmpty(encoded))
        {
            return 0;
        }

        ulong result = 0;

        for (int i = 0; i < encoded.Length; i++)
        {
            if (!CharToValueMap.TryGetValue(encoded[i], out int value))
            {
                return 0;
            }

            result = result * (ulong)BaseLength + (ulong)value;
        }

        return result;
    }

    /// <summary>
    /// 바이트 배열 → 문자열 변환 
    /// </summary>
    private static string EncodeBytes(byte[] bytes, int length)
    {
        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            int index = bytes[i % bytes.Length] % BaseLength;
            result[i] = IdChars[index];
        }

        return new string(result);
    }
}