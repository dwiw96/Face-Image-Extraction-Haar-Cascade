# Face-Image-Extraction-Haar-Cascade
This program is to do face detection. Face is extracted with Haar Cascade using local binary patterns

## NOTE
  * I made this program for my internship at LIPI back in 2018, so there're a lot of things that I forget.
  * This code was made using Visual Studio 2010, but right now I don't have the other file such as library, etc..
    So, this is the only file that remaining.

## Image Capture
Face detection is running real time using laptop webcam, library emgu cv is used to access webcam on laptop.
This code is use to access webcam on laptop.
```
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
```
Result of that code is
<p align="center">
  <img height=60% width=60% src="https://user-images.githubusercontent.com/85001958/233644902-c0eb6ed1-0859-4027-8a94-8705b25450a3.png">
</p>

## Converting image to greyscale
This convertion running automatically, So when the program is running we can't se the result of greyscale.
So, in order show you the result I made separated program to convert image to greyscale (I'm not upload the code to repository)
code for convert to greyscale:
```
Image<Gray, byte>grayscale = Webcam.Convert<Gray, byte>();
```
Result:
<p align="center">
  <img width=60% height=60% src="https://user-images.githubusercontent.com/85001958/233647235-b66e5e33-99b7-4f2c-b4e7-476288a3040f.png">
</p>

## Haar Cascade
Image that already converted to greyscale will go to next process, that is detection the face.
method for detecting the face is haar cascade with LBP (local binary patterns).
Code:
```
public CascadeClassifier Face = new CascadeClassifier(Application.StartupPath + "\\Cascades\\haarcascade_frontalface_default.xml");
```
Result:
<p align="center">
  <img height=60% width=60% src="https://user-images.githubusercontent.com/85001958/233648734-335ea157-7153-43bc-9264-f0b5b9d48809.png"
</p>
