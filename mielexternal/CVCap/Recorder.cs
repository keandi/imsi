using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CVCap
{
    public class Recorder
    {
        #region Singleton
        private static Recorder _self = null;
        private static object _instanceLocker = new object();

        public static Recorder Instance
        {
            get
            {
                lock (_instanceLocker)
                {
                    if (_self == null)
                    {
                        _self = new Recorder();
                    }
                }
                return _self;
            }
        }
        #endregion

        #region Record start
        public bool Start(string path, string filename, string fourcc, double fps, int frameCX, int frameCY)
        {
            bool result = false;
            string savePath = "";
            try
            {
                if (_isRecording == true)
                {
                    throw new Exception("Already recording.......");
                }
                _isRecording = true;

                if (string.IsNullOrWhiteSpace(path) == true ||
                    string.IsNullOrWhiteSpace(filename) == true ||
                    string.IsNullOrWhiteSpace(fourcc) == true ||
                    fps <= 0.0 ||
                    frameCX <= 0 ||
                    frameCY <= 0)
                {
                    throw new Exception("Input argument error");
                }

                if (Helper.CreateFolder(path) == false)
                {
                    throw new Exception("path error");
                }

                savePath = path + "\\" + filename;
                Helper.DeleteFile(savePath);

                CvSize size = new CvSize(frameCX, frameCY);

                #region Thread record
                Thread recordThread = new Thread(() => {
                    CvVideoWriter writer = null;

                    try
                    {
                        writer = new CvVideoWriter(savePath, fourcc, fps, size);

                        _threadState = ThreadState.ThreadState_Running;
                        Stopwatch stopWatch = new Stopwatch();
                        int nSleep = (int)(1000.0 / fps);
                        while (_threadState != ThreadState.ThreadState_Running_WantStop)
                        {
                            stopWatch.Start();
                            IplImage image = ConvertToIplImage(Helper.CaptureScreen(true));
                            if (image.Width != frameCX || image.Height != frameCY)
                            {
                                image = ResizeImage(image, frameCX, frameCY);
                            }
                            writer.WriteFrame(image);
                            stopWatch.Stop();
                            /*fps 와 해당 Sleep 를 적당히 잘 조절해야 항상 정확한 속도를 얻을 수 있다.
                             * 아래 수치는 임시 수치임.
                             */
                            if (nSleep > (int)stopWatch.ElapsedMilliseconds)
                            {
                                Thread.Sleep(nSleep + (int)stopWatch.ElapsedMilliseconds);
                            }
                            else
                            {
                                Thread.Sleep(0);
                            }
                            stopWatch.Reset();
                        }
                    }
                    catch (Exception ex)
                    {
                        MethodBase ctxMethod = MethodBase.GetCurrentMethod();
                        string msg = string.Format("[{0}.{1}] {2}", ctxMethod.ReflectedType.FullName, ctxMethod.Name, ex.Message);
                        Debug.WriteLine(msg);
                    }
                    finally
                    {
                        if (writer != null)
                        {
                            writer.Dispose();
                        }
                        _threadState = ThreadState.ThreadState_Stopped;
                        _isRecording = false;
                    }
                });
                #endregion
                recordThread.Start();

                //
                result = true;
            }
            catch (Exception ex)
            {
                Helper.DeleteFile(savePath);
                _isRecording = false;

                MethodBase ctxMethod = MethodBase.GetCurrentMethod();
                string msg = string.Format("[{0}.{1}] {2} (path: {3})", ctxMethod.ReflectedType.FullName, ctxMethod.Name, ex.Message, path);
                Debug.WriteLine(msg);
            }
            finally
            {

            }
            return result;
        }
        #endregion

        #region Record stop
        public bool Stop()
        {
            bool result = false;
            try
            {
                if (_isRecording == true)
                {
                    if (_threadState == ThreadState.ThreadState_Running)
                    {
                        _threadState = ThreadState.ThreadState_Running_WantStop;
                        while (_threadState != ThreadState.ThreadState_Stopped)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }

                result = (_isRecording == true) ? false : true;
            }
            catch (Exception ex)
            {
                MethodBase ctxMethod = MethodBase.GetCurrentMethod();
                string msg = string.Format("[{0}.{1}] {2}", ctxMethod.ReflectedType.FullName, ctxMethod.Name, ex.Message);
                Debug.WriteLine(msg);
            }
            finally
            {

            }
            return result;
        }
        #endregion

        #region Record state
        private bool _isRecording = false;
        public bool IsRecording
        {
            get
            {
                return _isRecording;
            }
        }

        #endregion

        #region Thread state
        private enum ThreadState
        {
            ThreadState_Unknown = 0,
            ThreadState_Running,
            ThreadState_Stopped,
            ThreadState_Running_WantStop
        }
        private ThreadState _threadState = ThreadState.ThreadState_Unknown;
        #endregion

        #region OpenCV Util
        public Bitmap ConvertToBitmap(IplImage src)
        {
            Bitmap bitmap = src.ToBitmap();

            return bitmap;
        }

        public IplImage ConvertToIplImage(Bitmap src)
        {
            IplImage iplimage = src.ToIplImage();
            //IplImage iplimage = new IplImage(src.Width, src.Height, BitDepth.U8, 3);

            return iplimage;
        }

        public IplImage ResizeImage(IplImage src, int wantCX, int wantCY)
        {
            IplImage resize = new IplImage(Cv.Size(wantCX, wantCY), BitDepth.U8, 3);
            Cv.Resize(src, resize, Interpolation.Linear);
            return resize;
        }
        #endregion

    }
}
