using Intel.RealSense;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.ComponentModel;

namespace FaceApp
{
    public partial class Form1 : Form
    {
        Pipeline pipe;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //RealSense 장치연결
            Context ctx = new Context();
            var list = ctx.QueryDevices(); // Get a snapshot of currently connected devices
            if (list.Count == 0)
                throw new Exception("No device detected. Is it plugged in?");
            Device dev = list[0];

            //프레임 가져오기
            pipe = new Pipeline();
            pipe.Start();

            backgroundWorker1.RunWorkerAsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var bgWorker = (BackgroundWorker)sender;

            while (!bgWorker.CancellationPending)
            {
                using (var frames = pipe.WaitForFrames())
                using (var color = frames.ColorFrame)
                {
                    var test = frames.ColorFrame.DisposeWith(frames);
                    Mat mat = FrameToMat(test);
                    var frameBitmap = BitmapConverter.ToBitmap(mat);
                    bgWorker.ReportProgress(0, frameBitmap);
                    Thread.Sleep(100);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            var frameBitmap = (Bitmap)e.UserState;
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = frameBitmap;
        }


        private Mat FrameToMat(Intel.RealSense.Frame f)
        {
            var vf = f as VideoFrame;
            int w = vf.Width;
            int h = vf.Height;
            Mat m = null;
            if (vf.Profile.Format == Format.Bgr8)
            {
                m = new Mat(w, h, MatType.CV_8UC3, f.Data);
                return m;
            }
            else if (vf.Profile.Format == Format.Rgb8)
            {
                m = new Mat(w, h, MatType.CV_8UC3, f.Data);
                m.CvtColor(ColorConversionCodes.RGB2BGR);
                return m;
            }
            else if (vf.Profile.Format == Format.Z16)
            {
                m = new Mat(w, h, MatType.CV_16UC1, f.Data);
                return m;
            }
            else if (vf.Profile.Format == Format.Y8)
            {
                m = new Mat(w, h, MatType.CV_8UC1, f.Data);
                return m;
            }
            else if (vf.Profile.Format == Format.Disparity32)
            {
                m = new Mat(w, h, MatType.CV_32FC1, f.Data);
                return m;
            }
            else
            {
                MessageBox.Show("Error occurred!");
                return m;
            }
        }
    }
}