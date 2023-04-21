using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum; //Jenis Font

using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Serialization;
using System.Drawing.Imaging;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Training_LBP
{
    public partial class Form1 : Form
    {
        #region Variables
        //Camera specific
        Capture capture;

        //Images for finding face
        Image<Bgr, Byte> Webcam;
        Image<Gray, byte> hasil = null;

        //For aquiring 10 images in a row
        List<Image<Gray, byte>> resultImages = new List<Image<Gray, byte>>();
        int num_faces_to_aquire = 10;

        //Saving Jpg
        EncoderParameters ENC_Parameters = new EncoderParameters(1);
        EncoderParameter ENC = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100);

        //Saving XAML Data file
        List<string> NamestoWrite = new List<string>();
        List<string> NamesforFile = new List<string>();
        XmlDocument docu = new XmlDocument();

        Image<Gray, byte> grayscale = null; //untuk hasil grayscale
        private bool captureInProgress;
        public CascadeClassifier Face = new CascadeClassifier(Application.StartupPath + "\\Cascades\\haarcascade_frontalface_default.xml");//untuk mendeteksi wajah (metode haar)
        //MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 0.5, 0.5); //jenis font
        int nolabel;
        //Classifier with default training location

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        /*public void initialise_capture()
        {
            capture = new Capture();
            capture.QueryFrame();
            //Initialize the FrameGraber event
            Application.Idle += new EventHandler(ProcessFrame);
        }*/

        public void ProcessFrame(object sender, EventArgs arg)
        {
            capture = new Capture();
            capture.QueryFrame();
            Webcam = capture.QuerySmallFrame(); //capture
            if (Webcam != null)
            {
                imageBox1.Image = Webcam; //tampilkan webcam ke imagebox1
                //Webcam.Save("C:\\Users\\irvan_effendi\\Pictures\\sample.jpg");
                grayscale = Webcam.Convert<Gray, byte>(); //convert ke grayscale
                Rectangle[] facesDetected = Face.DetectMultiScale(grayscale, 1.2, 10, new Size(50, 50), Size.Empty); //face detector
                //Action for each element detected
                for (int i = 0; i < facesDetected.Length; i++)// (Rectangle face_found in facesDetected)
                {
                    //This will focus in on the face from the haar results its not perfect but it will remove a majoriy
                    //of the background noise
                    facesDetected[i].X += (int)(facesDetected[i].Height * 0.15);
                    facesDetected[i].Y += (int)(facesDetected[i].Width * 0.22);
                    facesDetected[i].Height -= (int)(facesDetected[i].Height * 0.3);
                    facesDetected[i].Width -= (int)(facesDetected[i].Width * 0.35);

                    hasil = Webcam.Copy(facesDetected[i]).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    hasil._EqualizeHist();
                    Bitmap BmpInput = hasil.ToBitmap(); //convert emgu cv jadi bitmap
                    FaceImage.Image = BmpInput;
                    //draw the face detected in the 0th (gray) channel with blue color
                    Webcam.Draw(facesDetected[i], new Bgr(Color.Red), 2);
                }
            }
            
            //Show the faces procesed and recognized
            //pictureBox3.Image = Webcam.ToBitmap();
            //pictureBox2.Image = BmpInput;
        }

        double Bin2Dec(List<int> bin)
        {
            double d = 0;

            for (int i = 0; i < bin.Count; i++)
            {
                d += bin[i] * Math.Pow(2, i);
            }
            return d;
        }

        Bitmap LBP(Bitmap BmpInput, int R)
        {
            //mengambil sumber gambar LBP dari srcBmp dan window R
            Bitmap bmp = BmpInput;
            //1. Extract rows dan columns dari srcImage . sumber gambar harus grayscale
            int NumRow = BmpInput.Height;
            int numCol = BmpInput.Width;
            Bitmap lbp = new Bitmap(numCol, NumRow);
            Bitmap GRAY = new Bitmap(imageBox1.Width, imageBox1.Height);// GRAY adalah resultant matrix 
            double[,] MAT = new double[numCol, NumRow];
            double max = 0.0;
            //2. Loop through Pixels
            for (int i = 0; i < NumRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    //  Color c1=Color.FromArgb(0,0,0);
                    MAT[j, i] = 0;
                    //lbp.SetPixel(j, i,c1) ;


                    //define boundary condition, other wise say if you are looking at pixel (0,0), it does not have any suitable neighbors
                    if ((i > R) && (j > R) && (i < (NumRow - R)) && (j < (numCol - R)))
                    {
                        // we want to store binary values in a List
                        List<int> vals = new List<int>();
                        try
                        {
                            for (int i1 = i - R; i1 < (i + R); i1++)
                            {
                                for (int j1 = j - R; j1 < (j + R); j1++)
                                {
                                    int acPixel = BmpInput.GetPixel(j, i).R;
                                    int nbrPixel = BmpInput.GetPixel(j1, i1).R;
                                    // 3. This is the main Logic of LBP
                                    if (nbrPixel > acPixel)
                                    {
                                        vals.Add(1);

                                    }
                                    else
                                    {
                                        vals.Add(0);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                        //4. Once we have a list of 1's and 0's , convert the list to decimal
                        // Also for normalization purpose calculate Max value
                        double d1 = Bin2Dec(vals);
                        MAT[j, i] = d1;
                        if (d1 > max)
                        {
                            max = d1;
                        }
                    }
                }
            }
            //5. Normalize LBP matrix MAT an obtain LBP image lbp
            lbp = NormalizeLbpMatrix(MAT, lbp, max);
            return lbp;
        }

        Bitmap NormalizeLbpMatrix(double[,] Mat, Bitmap lbp, double max)
        {
            int NumRow = lbp.Height;
            int numCol = lbp.Width;
            for (int i = 0; i < NumRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    // see the Normalization process of dividing pixel by max value and multiplying with 255
                    double d = Mat[j, i] / max;
                    int v = (int)(d * 255);
                    Color c = Color.FromArgb(v, v, v);
                    lbp.SetPixel(j, i, c);
                }
            }
            return lbp;
        }

        //Saving The Data
        private bool save_training_data(Image face_data)
        {
            try
            {
                Random rand = new Random();
                bool file_create = true;
                string facename = "face_" + textBox1.Text + "_" + rand.Next().ToString() + ".jpg";
                while (file_create)
                {

                    if (!File.Exists(Application.StartupPath + "/TrainedFaces/" + facename))
                    {
                        file_create = false;
                    }
                    else
                    {
                        facename = "face_" + textBox1.Text + "_" + rand.Next().ToString() + ".jpg";
                    }
                }


                if (Directory.Exists(Application.StartupPath + "/TrainedFaces/"))
                {
                    face_data.Save(Application.StartupPath + "/TrainedFaces/" + facename, ImageFormat.Jpeg);
                }
                else
                {
                    Directory.CreateDirectory(Application.StartupPath + "/TrainedFaces/");
                    face_data.Save(Application.StartupPath + "/TrainedFaces/" + facename, ImageFormat.Jpeg);
                }
                if (File.Exists(Application.StartupPath + "/TrainedFaces/TrainedLabels.xml"))
                {
                    //File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", NAME_PERSON.Text + "\n\r");
                    bool loading = true;
                    while (loading)
                    {
                        try
                        {
                            docu.Load(Application.StartupPath + "/TrainedFaces/TrainedLabels.xml");
                            loading = false;
                        }
                        catch
                        {
                            docu = null;
                            docu = new XmlDocument();
                            Thread.Sleep(10);
                        }
                    }

                    //Get the root element
                    XmlElement root = docu.DocumentElement;

                    XmlElement face_D = docu.CreateElement("FACE");
                    XmlElement name_D = docu.CreateElement("NAME");
                    XmlElement file_D = docu.CreateElement("FILE");

                    //Add the values for each nodes
                    //name.Value = textBoxName.Text;
                    //age.InnerText = textBoxAge.Text;
                    //gender.InnerText = textBoxGender.Text;
                    name_D.InnerText = textBox1.Text;
                    file_D.InnerText = facename;

                    //Construct the Person element
                    //person.Attributes.Append(name);
                    face_D.AppendChild(name_D);
                    face_D.AppendChild(file_D);

                    //Add the New person element to the end of the root element
                    root.AppendChild(face_D);

                    //Save the document
                    docu.Save(Application.StartupPath + "/TrainedFaces/TrainedLabels.xml");
                    //XmlElement child_element = docu.CreateElement("FACE");
                    //docu.AppendChild(child_element);
                    //docu.Save("TrainedLabels.xml");
                }
                else
                {
                    FileStream FS_Face = File.OpenWrite(Application.StartupPath + "/TrainedFaces/TrainedLabels.xml");
                    using (XmlWriter writer = XmlWriter.Create(FS_Face))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Faces_For_Training");

                        writer.WriteStartElement("FACE");
                        writer.WriteElementString("NAME", textBox1.Text);
                        writer.WriteElementString("FILE", facename);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    FS_Face.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private void btnSS_Click(object sender, EventArgs e)
        {
            if (capture == null)
            {
                try
                {
                    capture = new Capture();
                    //capture = new Capture("http://admin:28653485@192.168.1.2/video.cgi?x.mjpeg");
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }
            if (capture != null)
            {
                if (captureInProgress)
                {
                    btnSS.Text = "Start";
                    Application.Idle -= ProcessFrame;
                }
                else
                {
                    btnSS.Text = "Stop";
                    Application.Idle += ProcessFrame;
                }
                captureInProgress = !captureInProgress;
            }
        }

        private void btnLBP_Click(object sender, EventArgs e)
        {
            LBPBox.Image = LBP((Bitmap)FaceImage.Image, int.Parse(LBPWindow.Text));
        }

        //Add the image to training data
        private void btnADD_Click(object sender, EventArgs e)
        {
            if (resultImages.Count == num_faces_to_aquire)
            {
                if (!save_training_data(FaceImage.Image)) MessageBox.Show("Error", "Error in saving file info. Training data not saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Application.Idle -= ProcessFrame;
                if (!save_training_data(FaceImage.Image)) MessageBox.Show("Error", "Error in saving file info. Training data not saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
                capture = new Capture();
                capture.QueryFrame();
                //Initialize the FrameGraber event
                Application.Idle += new EventHandler(ProcessFrame);
            }
        }
    }
}
