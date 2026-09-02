using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class DesktopWindowController : MonoBehaviour
{
    public static DesktopWindowController Instance { get; private set; }

    [Header("Transparency / Chromakey")]
    [Tooltip("สีที่จะทำเป็นโปร่งใส (กำหนดให้ตรงกับ Camera Clear Color หรือ UI background ของคุณ)")]
    public Color transparencyColor = new Color(1f, 0f, 1f, 1f); // ค่าเริ่มต้น: pure magenta
    [Tooltip("เปิดใช้ color-key transparency (Windows build เท่านั้น)")]
    public bool enableColorKey = true;

    [Header("Tamagotchi window size (px)")]
    public int tamagotchiWidth = 800;
    public int tamagotchiHeight = 160;

    [Header("Dungeon window size (px)")]
    public int dungeonWidth = 1280;
    public int dungeonHeight = 720;

    IntPtr _hwnd = IntPtr.Zero;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        InitializeWindow();
#else
        // Editor / non-Windows: ไม่มีการเรียก WinAPI
#endif
    }

    #region Public control methods

    public void ApplyTamagotchiWindow()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (_hwnd == IntPtr.Zero) InitializeWindow();
        MakeWindowBorderless();
        SetTopMost(true);
        if (enableColorKey) SetColorKeyTransparency(transparencyColor);
        LockWindowToBottom(tamagotchiWidth, tamagotchiHeight);
#else
        // Editor: สามารถปรับ GameView ขนาด/ตำแหน่งจำลองได้ ถ้าต้องการ implement ต่อ
#endif
    }

    public void ApplyDungeonWindow()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (_hwnd == IntPtr.Zero) InitializeWindow();
        MakeWindowBorderless();
        SetTopMost(true);
        if (enableColorKey) SetColorKeyTransparency(transparencyColor);
        CenterAndResizeWindow(dungeonWidth, dungeonHeight);
#else
        // Editor: noop
#endif
    }

    public void RestoreWindow()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (_hwnd == IntPtr.Zero) InitializeWindow();
        SetTopMost(true);
#endif
    }

    #endregion

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    void InitializeWindow()
    {
        // Set per-monitor DPI awareness before querying monitor/work area
        DPIHelper.SetPerMonitorDPI();

        _hwnd = GetActiveWindow();
        MakeWindowBorderless();
    }

    #region Win32 interop and helpers

    // Constants
    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;

    const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
    const uint WS_POPUP = 0x80000000;

    const int WS_EX_LAYERED = 0x00080000;

    const uint LWA_COLORKEY = 0x00000001;

    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_NOACTIVATE = 0x0010;

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    static IntPtr GetWindowLongWrapper(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8) return GetWindowLongPtr64(hWnd, nIndex);
        return GetWindowLong32(hWnd, nIndex);
    }

    static IntPtr SetWindowLongWrapper(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        return SetWindowLong32(hWnd, nIndex, dwNewLong);
    }

    [DllImport("user32.dll")]
    static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

    [DllImport("user32.dll")]
    static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);

    const uint MONITOR_DEFAULTTONEAREST = 2;
    const uint SPI_GETWORKAREA = 0x0030;

    [StructLayout(LayoutKind.Sequential)]
    struct RECT { public int left, top, right, bottom; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    void MakeWindowBorderless()
    {
        if (_hwnd == IntPtr.Zero) return;
        IntPtr style = GetWindowLongWrapper(_hwnd, GWL_STYLE);
        long styleVal = style.ToInt64();
        // remove overlapped window flags and apply popup style
        styleVal &= ~((long)WS_OVERLAPPEDWINDOW);
        styleVal |= (long)WS_POPUP;
        SetWindowLongWrapper(_hwnd, GWL_STYLE, new IntPtr(styleVal));

        // ensure layered exstyle for transparency
        IntPtr exStyle = GetWindowLongWrapper(_hwnd, GWL_EXSTYLE);
        long exVal = exStyle.ToInt64();
        exVal |= WS_EX_LAYERED;
        SetWindowLongWrapper(_hwnd, GWL_EXSTYLE, new IntPtr(exVal));
    }

    void SetTopMost(bool top)
    {
        if (_hwnd == IntPtr.Zero) return;
        if (top) SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOZORDER | SWP_NOACTIVATE);
    }

    void SetColorKeyTransparency(Color key)
    {
        if (_hwnd == IntPtr.Zero) return;
        uint crKey = ColorToUInt(key);
        SetLayeredWindowAttributes(_hwnd, crKey, 0, LWA_COLORKEY);
    }

    uint ColorToUInt(Color c)
    {
        byte r = (byte)Mathf.RoundToInt(c.r * 255f);
        byte g = (byte)Mathf.RoundToInt(c.g * 255f);
        byte b = (byte)Mathf.RoundToInt(c.b * 255f);
        return (uint)(r | (g << 8) | (b << 16));
    }

    void LockWindowToBottom(int width, int height)
    {
        if (_hwnd == IntPtr.Zero) return;

        // get work area (พื้นที่ที่ไม่ถูกครอบงำโดย Taskbar)
        RECT workArea = new RECT();
        bool ok = SystemParametersInfo(SPI_GETWORKAREA, 0, ref workArea, 0);
        int screenRight = workArea.right;
        int screenBottom = workArea.bottom;
        int screenLeft = workArea.left;

        int x = screenRight - width; // right-aligned (ปรับได้ตามต้องการ)
        if (x < screenLeft) x = screenLeft;
        int y = screenBottom - height; // อยู่เหนือ Taskbar เพราะ workArea ถูกลดขนาดแล้ว

        SetWindowPos(_hwnd, HWND_TOPMOST, x, y, width, height, 0);
    }

    void CenterAndResizeWindow(int width, int height)
    {
        if (_hwnd == IntPtr.Zero) return;

        IntPtr monitor = MonitorFromWindow(_hwnd, MONITOR_DEFAULTTONEAREST);
        MONITORINFOEX mi = new MONITORINFOEX();
        mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
        if (GetMonitorInfo(monitor, ref mi))
        {
            int workWidth = mi.rcWork.right - mi.rcWork.left;
            int workHeight = mi.rcWork.bottom - mi.rcWork.top;
            int x = mi.rcWork.left + (workWidth - width) / 2;
            int y = mi.rcWork.top + (workHeight - height) / 2;
            SetWindowPos(_hwnd, HWND_TOPMOST, x, y, width, height, 0);
        }
        else
        {
            int x = (Screen.currentResolution.width - width) / 2;
            int y = (Screen.currentResolution.height - height) / 2;
            SetWindowPos(_hwnd, HWND_TOPMOST, x, y, width, height, 0);
        }
    }

    #endregion
#endif
}
