using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class BGManager : MonoSingleton<BGManager>
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(uint uiAction, int uiParam, string pvParam, uint fWinIni);

    public const uint SPI_GETDESKWALLPAPER = 0x0073;
    public const uint SPI_SETDESKWALLPAPER = 0x0014;
    public const uint SPIF_UPDATEINIFILE = 0x0001;
    public const uint SPIF_SENDWININICHANGE = 0x0002;

    private byte[] defaultBGByte;

    private void OnApplicationQuit()
    {
        SetDefaultBG();
    }

    public void SetModifiedBG(Texture2D origin, Texture2D modified)
    {
        string wallpaperPath = GetCurrentDesktopWallpaperPath();

        if (File.Exists(wallpaperPath))
        {
            defaultBGByte ??= File.ReadAllBytes(wallpaperPath);
        }

        File.WriteAllBytes(wallpaperPath, modified.EncodeToPNG());
        SystemParametersInfo(
            SPI_SETDESKWALLPAPER,
            0,
            wallpaperPath,
            SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE
        );
    }

    public Texture2D LoadTextureFromPath(string path)
    {
        Texture2D texture = new Texture2D(2, 2);
        defaultBGByte = File.ReadAllBytes(path);

        if (ImageConversion.LoadImage(texture, defaultBGByte))
        {
            texture.Apply();
        }

        return texture;
    }

    public Texture2D GetPCWallpaper()
    {
        return LoadTextureFromPath(GetCurrentDesktopWallpaperPath());
    }

    public string GetCurrentDesktopWallpaperPath()
    {
        string curWallpaper = new string('\0', 260);
        SystemParametersInfo(SPI_GETDESKWALLPAPER, curWallpaper.Length, curWallpaper, 0);
        return curWallpaper.Substring(0, curWallpaper.IndexOf('\0'));
    }

    public void SetDefaultBG()
    {
        if (defaultBGByte == null)
            return;

        string wallpaperPath = GetCurrentDesktopWallpaperPath();
        File.WriteAllBytes(wallpaperPath, defaultBGByte);
        SystemParametersInfo(
            SPI_SETDESKWALLPAPER,
            0,
            wallpaperPath,
            SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE
        );
    }
}