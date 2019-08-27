using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace CommonLib
{
    public class HelperLib
    {
        #region Window 관련

        #region DLL Import
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        #endregion

        #region 상수 선언
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZE = 0x20000;  //최초 최소화된 상태로 윈도우를 만든다.
        #endregion

        #region Maximize 버튼 숨기기
        public static void DisableMaximaizeButton(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
        }
        #endregion

        #region Minimize 버튼 숨기기
        public static void DisableMinimaizeButton(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MINIMIZE));
        }
        #endregion

        #endregion

        #region 파일 폴더 관련

        #region 폴더 생성
        public static bool CreateFolder(string path)
        {
            bool result = false;
            try
            {
                path = path.Replace("/", "\\");
                string[] name = path.Split('\\');
                if (name == null || name.Length <= 1)
                {
                    throw new Exception("Path split failed");
                }

                string fullPath = "";
                for (int idx = 0; idx < name.Length; idx++)
                {
                    if (idx == 0)
                    {
                        fullPath = name[idx];
                        continue;
                    }
                    else
                    {
                        fullPath += "\\" + name[idx];
                    }

                    DirectoryInfo di = new DirectoryInfo(fullPath);
                    if (di.Exists == true) { continue; }

                    di.Create();
                }

                //
                result = true;
            }
            catch (Exception ex)
            {
                MethodBase ctxMethod = MethodBase.GetCurrentMethod();
                string msg = string.Format("[{0}.{1}] {2} (path: {3})", ctxMethod.ReflectedType.FullName, ctxMethod.Name, ex.Message, path);
                Debug.WriteLine(msg);
            }

            return result;
        }
        #endregion

        #endregion

        #region 캡쳐 관련

        public static void Capture(string path, string filename, bool withMousePointer = false)
        {
            if (CreateFolder(path) == false) { return; }

            string filePath = path + "\\" + filename;
            if (withMousePointer == true)
            {
                CaptureWithMousePoiniter(filePath);
            }
            else
            {
                Capture(filePath);
            }
        }

        #region 마우스 포인터 제외 캡쳐 -> 파일 저장
        private static void Capture(string outputFilename)
        {
            // 주화면의 크기 정보 읽기
            System.Drawing.Rectangle rect = Screen.PrimaryScreen.Bounds;
            // 2nd screen = Screen.AllScreens[1]

            // 픽셀 포맷 정보 얻기 (Optional)
            int bitsPerPixel = Screen.PrimaryScreen.BitsPerPixel;
            PixelFormat pixelFormat = PixelFormat.Format32bppArgb;
            if (bitsPerPixel <= 16)
            {
                pixelFormat = PixelFormat.Format16bppRgb565;
            }
            if (bitsPerPixel == 24)
            {
                pixelFormat = PixelFormat.Format24bppRgb;
            }

            // 화면 크기만큼의 Bitmap 생성
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, pixelFormat);

            // Bitmap 이미지 변경을 위해 Graphics 객체 생성
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                // 화면을 그대로 카피해서 Bitmap 메모리에 저장
                gr.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
            }

            // Bitmap 데이타를 파일로 저장
            bmp.Save(outputFilename);
            bmp.Dispose();
        }
        #endregion

        #region 마우스 포인터 포함 캡쳐 -> 파일 저장
        private static void CaptureWithMousePoiniter(string outputFilename)
        {
            // 화면 크기만큼의 Bitmap 생성
            Bitmap bmp = CaptureScreen(true);
            if (bmp == null) { return; }

            // Bitmap 데이타를 파일로 저장
            bmp.Save(outputFilename);
            bmp.Dispose();
        }
        #endregion

        #region 마우스 포인터 포함/미포함 캡쳐 -> Bitmap
        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;

        private static Bitmap CaptureScreen(bool CaptureMouse)
        {
            Bitmap result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

                    if (CaptureMouse)
                    {
                        CURSORINFO pci;
                        pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

                        if (GetCursorInfo(out pci))
                        {
                            if (pci.flags == CURSOR_SHOWING)
                            {
                                DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                                g.ReleaseHdc();
                            }
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }
        #endregion

        #endregion
    }
}
