using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace FaceRecognition
{
    public partial class Form1 : Form
    {
        private VideoCapture videoCapture;
        private CascadeClassifier haarCascade;

        private Image<Bgr, Byte> bgrFrame = null;
        private Image<Gray, Byte> detectedFace = null;

        private List<FaceData> faceList = new List<FaceData>();
        private VectorOfMat imageList = new VectorOfMat();
        private List<string> nameList = new List<string>();
        private VectorOfInt labelList = new VectorOfInt();

        private EigenFaceRecognizer recognizer;

        private string faceName;
        public string FaceName
        { 
            get { return faceName; }
            set
            { 
                faceName = value;
                nameLabel.Text = faceName;
            }
        }

        private Bitmap cameraCapture;
        public Bitmap CameraCapture
        {
            get { return cameraCapture; }
            set
            { 
                cameraCapture = value;
                pictureBox1.Image = cameraCapture;
            }
        }

        private Bitmap cameraCaptureFace;
        public Bitmap CameraCaptureFace
        { 
            get { return cameraCaptureFace; }
            set
            {
                cameraCaptureFace = value;
                pictureBox2.Image = cameraCaptureFace;
            }
        }


        public Form1()
        {
            InitializeComponent();
        }

        #region Method
        public void GetFacesList()
        {
            if (!File.Exists(Config.HaarCascadePath))
            { 
                string text = "Cannot find Haar cascade data file:\n\n";
                text += Config.HaarCascadePath;
                MessageBox.Show(text, "Error", MessageBoxButtons.OK);
            }

            haarCascade = new CascadeClassifier(Config.HaarCascadePath);
            faceList.Clear();
            //nameList.Clear();
            //imageList.Clear();
            //labelList.Clear();
            string line;
            FaceData faceInstance = null;

            if (!Directory.Exists(Config.FacePhotosPath))
            {
                Directory.CreateDirectory(Config.FacePhotosPath);
            }

            if (!File.Exists(Config.FaceListTextFile))
            {
                string text = "Cannot find face data file:\n\n";
                text += Config.FaceListTextFile + "\n\n";
                text += "If this is your first time running the app, an empty file will be created for you.";
                DialogResult result = MessageBox.Show(text, "Warning", MessageBoxButtons.OKCancel);
                switch (result)
                {
                    case DialogResult.OK:
                        String dirName = Path.GetDirectoryName(Config.FaceListTextFile);
                        Directory.CreateDirectory(dirName);
                        File.Create(Config.FaceListTextFile).Close();
                        break;
                }
            }

            StreamReader reader = new StreamReader(Config.FaceListTextFile);
            int i = 0;
            while ((line = reader.ReadLine()) != null)
            {
                string[] lineParts = line.Split(':');
                faceInstance = new FaceData();
                faceInstance.FaceImage = new Image<Gray, byte>(Config.FacePhotosPath + lineParts[0] + Config.ImageFileExtension);
                faceInstance.PersonName = lineParts[1];
                faceList.Add(faceInstance);
            }
            foreach (var face in faceList)
            {
                imageList.Push(face.FaceImage.Mat);
                nameList.Add(face.PersonName);
                labelList.Push(new[] { i++ });
            }
            reader.Close();

            // Train recogniser
            if (imageList.Size > 0)
            {
                recognizer = new EigenFaceRecognizer(imageList.Size);
                recognizer.Train(imageList, labelList);
            }
        }

        private void ProcessFrame()
        {
            bgrFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>();

            if (bgrFrame != null)
            {
                try
                {
                    Image<Gray, byte> grayFrame = bgrFrame.Convert<Gray, byte>();

                    // 프레임에서 여러 얼굴을 감지
                    Rectangle[] faces = haarCascade.DetectMultiScale(grayFrame, 1.2, 10, new System.Drawing.Size(50, 50), new System.Drawing.Size(200, 200));

                    FaceName = "No face detected";
                    foreach (var face in faces)
                    {
                        bgrFrame.Draw(face, new Bgr(255, 255, 0), 2);
                        detectedFace = bgrFrame.Copy(face).Convert<Gray, byte>();
                        FaceRecognition();
                        break;
                    }
                    CameraCapture = bgrFrame.ToBitmap();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void FaceRecognition()
        {
            if (imageList.Size != 0)
            {
                //등록된 얼굴이 있어야 여기로 옴
                FaceRecognizer.PredictionResult result = recognizer.Predict(detectedFace.Resize(100, 100, Inter.Cubic));
                FaceName = nameList[result.Label];
                CameraCaptureFace = detectedFace.ToBitmap();
            }
            else
            {
                FaceName = "Please Add Face";
            }
        }
        #endregion

        #region Event
        private void Form1_Load(object sender, EventArgs e)
        {
            GetFacesList();
            videoCapture = new VideoCapture(Config.ActiveCameraIndex);
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ProcessFrame();
        }

        private void registerFaceButton_Click(object sender, EventArgs e)
        {
            if (detectedFace == null)
            {
                MessageBox.Show("No face detected");
                return;
            }
            //Save detected face
            detectedFace = detectedFace.Resize(100, 100, Inter.Cubic);
            detectedFace.Save(Config.FacePhotosPath + "face" + (faceList.Count + 1) + Config.ImageFileExtension);
            StreamWriter writer = new StreamWriter(Config.FaceListTextFile, true);
            string personName = Microsoft.VisualBasic.Interaction.InputBox("Your Name");
            writer.WriteLine(String.Format("face{0}:{1}", (faceList.Count + 1), personName));
            writer.Close();
            GetFacesList();
            MessageBox.Show("Successful.");
        }
        #endregion
    }
}