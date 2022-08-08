using rsid;

namespace RealSendID
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Authentication callbacks
        static void OnAuthHint(rsid.AuthStatus hint, IntPtr ctx)
        {
            Console.WriteLine("OnHint " + hint);
        }

        static void OnAuthResult(rsid.AuthStatus status, string userId, IntPtr ctx)
        {
            Console.WriteLine("OnResults " + status);
            if (status == AuthStatus.Success)
            {
                Console.WriteLine("Authenticated " + userId);
            }
        }

        static void OnFaceDeteced(IntPtr facesArr, int faceCount, uint timestamp, IntPtr ctx)
        {
            Console.WriteLine($"OnFaceDeteced: {faceCount} face(s)");
            //convert to face rects
            var faces = rsid.Authenticator.MarshalFaces(facesArr, faceCount);
            foreach (var face in faces)
            {
                Console.WriteLine($"*** OnFaceDeteced {face.x},{face.y}, {face.width}x{face.height} (ts {timestamp})");
            }
        }

        static void OnPreviewCallback(PreviewImage image, IntPtr ctx)
        {
            var test = $"OnPreview: {image.width}x{image.height}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var auth = new Authenticator();
            if (auth.Connect(new SerialConfig { port = "COM9" }) != Status.Ok) 
            {
                MessageBox.Show("Error connection to device");
                return;
            }
            Thread.Sleep(3000);
            auth.Standby();
            Thread.Sleep(100);
            auth.Disconnect();
            Thread.Sleep(3000);
            if (auth.Connect(new SerialConfig { port = "COM9" }) != Status.Ok)
            {
                MessageBox.Show("Error connection to device");
                return;
            }

            Preview preview = new Preview(new PreviewConfig()
            {
                previewMode = PreviewMode.MJPEG_1080P,
            });
            preview.Start(OnPreviewCallback);
            Thread.Sleep(2000);

            var authArgs = new AuthArgs { hintClbk = OnAuthHint, resultClbk = OnAuthResult, faceDetectedClbk = OnFaceDeteced };
            auth.Authenticate(authArgs);
        }
    }
}