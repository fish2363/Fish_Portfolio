using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
/// <summary>
/// 아ㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅁㄴ롱ㅎㄴ덕솢미ㅓㅇㅁㄷ림ㄻ짖ㄷㅁ곡ㄷㅈ가ㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅇㅁㄴㅀ뮺ㅂㅁ더륮ㅁㄷㄹㄷ루모르겟다
/// </summary>
public class BGManager : MonoSingleton<BGManager>
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(uint uiAction, int uiParam, string pvParam, uint fWinIni);

    [DllImport("libwebp")]
    public static extern IntPtr WebPDecodeRGBA(IntPtr data, int dataSize, out int width, out int height);

    public const uint SPI_GETDESKWALLPAPER = 0x0073;
    public const uint SPI_SETDESKWALLPAPER = 0x0014;
    public const uint SPIF_UPDATEINIFILE = 0x0001;
    public const uint SPIF_SENDWININICHANGE = 0x0002;

    private byte[] defaultBGByte;

    private void OnApplicationQuit()
    {
        SetDefaultBG();
    }

    public Texture2D OverlayImages(Texture2D baseImage, Texture2D overlayImage)
    {
        Texture2D result = new Texture2D(2, 2);
        Vector2 offset = new Vector2();
        int width = baseImage.width;
        int height = baseImage.height;

        result = new Texture2D(width, height, TextureFormat.ARGB32, false);
        offset.x = (baseImage.width / 2) - (Screen.width / 2);
        offset.y = (baseImage.height / 2) - (Screen.height / 2);
        Debug.Log("신이시여");

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result.SetPixel(x, y, baseImage.GetPixel(x, y));
            }
        }
        Debug.Log("신이시여");

        for (int y = 0; y < overlayImage.height; y++)
        {
            for (int x = 0; x < overlayImage.width; x++)
            {
                if (x + offset.x < width && y + offset.y < height)
                {
                    Color baseColor = result.GetPixel((int)(x + offset.x), (int)(y + offset.y));
                    Color overlayColor = overlayImage.GetPixel(x, y);
                    Color finalColor = Color.Lerp(baseColor, overlayColor, overlayColor.a / 1.0f);
                    result.SetPixel((int)(x + offset.x), (int)(y + offset.y), finalColor);
                }
            }
        }
        Debug.Log("신이시여");
        return result;
    }

    public void SetModifiedBG(Texture2D origin, Texture2D modified)
    {
        // 배경화면 2개를 곂쳐서 최종 적용해주는 기능
        File.WriteAllBytes(GetCurrentDesktopWallpaperPath(), OverlayImages(origin, modified).EncodeToPNG());


        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, GetCurrentDesktopWallpaperPath(), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
    }

    public Texture2D LoadTextureFromPath(string path)
    {
        Texture2D texture = new Texture2D(2, 2);
        defaultBGByte = File.ReadAllBytes(path);

        if (ImageConversion.LoadImage(texture, defaultBGByte)) // jpg, png, jpeg만 됨
        {
            Color[] bgColors = texture.GetPixels();

            texture.Reinitialize(texture.width, texture.height, TextureFormat.RGBA32, false);

            texture.SetPixels(bgColors);

            texture.Apply();
        }
        else
        {
            Debug.Log("webp 확장자입니다.");
            texture = LoadWebpImage(defaultBGByte);
        }
        Debug.Log("Texture Loaded");
        return texture;
    }

    public Texture2D LoadWebpImage(byte[] webpData)
    {
        int width, height;
        IntPtr dataPtr = Marshal.AllocHGlobal(webpData.Length);
        Marshal.Copy(webpData, 0, dataPtr, webpData.Length);

        IntPtr resultIntPtr = WebPDecodeRGBA(dataPtr, webpData.Length, out width, out height);
        if (resultIntPtr == IntPtr.Zero)
        {
            Debug.LogError("Failed to decode WebP image");
            Marshal.FreeHGlobal(dataPtr);
            return null;
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        byte[] rgbaImage = new byte[width * height * 4];
        Marshal.Copy(resultIntPtr, rgbaImage, 0, rgbaImage.Length);

        Marshal.FreeHGlobal(resultIntPtr);
        Marshal.FreeHGlobal(dataPtr);
        byte[] flippedImage = FlipImageVertically(rgbaImage, width, height);
        texture.LoadRawTextureData(flippedImage);
        texture.Apply();
        return texture;
    }

    public byte[] FlipImageVertically(byte[] rawImage, int width, int height)
    {
        int stride = width * 4;
        byte[] flippedImage = new byte[rawImage.Length];

        for (int i = 0; i < height; i++)
        {
            Array.Copy(rawImage, i * stride, flippedImage, (height - i - 1) * stride, stride);
        }

        return flippedImage;
    }



    public Texture2D GetPCWallpaper()
    {
        Debug.Log("신이시여");
        return LoadTextureFromPath(GetCurrentDesktopWallpaperPath());
    }

    public string GetCurrentDesktopWallpaperPath()
    {
        string curWallpaper = new string('\0', 260);
        SystemParametersInfo(SPI_GETDESKWALLPAPER, curWallpaper.Length, curWallpaper, 0);
        Debug.Log(curWallpaper);
        return curWallpaper.Substring(0, curWallpaper.IndexOf('\0'));

    }

    public void SetDefaultBG()
    {
        if (defaultBGByte == null) return;
        if (defaultBGByte.Equals(null))
        {
            return;
        }

        // 기본 배경화면 롤백
        File.WriteAllBytes(GetCurrentDesktopWallpaperPath(), defaultBGByte);
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, GetCurrentDesktopWallpaperPath(), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
    }
}