using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gifmo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Default Settings
        /// </summary>
        private const int DefaultWidth = 360;
        private const int DefaultHeight = 180;
        private const int DefaultDuration = 5;
        private const double DefaultFuzz = 1.6;
        private const int DefaultDelay = 5;
        private const int DefaultLoops = 0;
        private const string DefaultVideoStartPostion = "00:00";
        private const string DefaultOutputName = "gifmo.gif";
        private const string DefaultTempFileName = "temp.gif";

        /// <summary>
        /// Path to the 3rd Party Components on your system
        /// </summary>
        private string MPlayerPath = @"c:\gifmo\mplayer";
        private string ImageMajikPath = @"c:\gifmo\convert";
        private string GifsiclePath = @"c:\gifmo\gifsicle";

        /// <summary>
        /// Pathing for temporary folder
        /// </summary>
        private string TempFolderRelativePath = "/gifmo/temp";
        private string TempFolderFullPath = @"c:\gifmo\temp\";
        private string TempFolderImageList = "/gifmo/temp/*.png";
        private string TempFileName;
        private string TempFileNamePath;

        /// <summary>
        /// Pathing for the output folder where finsihed gif is placed
        /// </summary>
        private string OutputFolderRelativePath = "/gifmo/completed/";
        private string OutputFolderFullPath = @"c:\gifmo\completed\";
        private string OutputFileRelativePath = string.Empty;
        private string OutputFileFullPath = string.Empty;
        private string OutputFileName = string.Empty;
        private string OutputFilePath = string.Empty;
        private int OutputWidth;
        private int OutputHeight;
        //TODO: Make these inputs
        private int OutputDelay = 0;
        private int OutputLoop = 0;
        private double OutputFuzz = 1.6;


        //Video Settings
        private string VideoStartPosition;
        private int VideoDuration;
        private string VideoSource = string.Empty;


        public MainWindow()
        {
            InitializeComponent();

            InitForm();

            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd")
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            btnCaptureImages.IsEnabled = false;
            btnGenerate.IsEnabled = false;
            btnPrepImages.IsEnabled = false;
        }

        private void InitForm()
        {
            //set default Form values
            tbWidth.Text = DefaultWidth.ToString();
            tbHeight.Text = DefaultHeight.ToString();
            //tbOutputDelay.Text = DefaultDelay.ToString();
            //tbOutputFuzz.Text = DefaultFuzz.ToString();
        }


        private void tbSelectFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "Document"; // Default file name
            //dlg.DefaultExt = ".txt"; // Default file extension
            //dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                tbVideoSource.Text = dlg.FileName;

                btnCaptureImages.IsEnabled = true;
                btnPrepImages.IsEnabled = false;
                btnGenerate.IsEnabled = false;
            }
        }

        private void btnCaptureImages_Click(object sender, RoutedEventArgs e)
        {
            lblOutput.Content = "Capturing Images from Video";
            if (UnbindForm())
            {
                //Delete any existing files in the Temp folder
                ClearTempFolder();

                //Use MPlayer to create collection of PNGs from video source
                ExtractImagesFromVideo();

                BindTempImages();

                btnCaptureImages.IsEnabled = true;
                btnPrepImages.IsEnabled = true;
                btnGenerate.IsEnabled = true;
            }
            lblOutput.Content = "Image Capturing Completed.";
        }

        private void BindTempImages()
        {
            string[] fp = Directory.GetFiles(TempFolderFullPath);

            lbImages.DataContext = fp;
            



        }



        private void btnPrepImages_Click(object sender, RoutedEventArgs e)
        {
            UnbindForm();

            //Reduce and Reverse Files before transform to animation
            ProcessFiles();
        }


        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            lblOutput.Content = "Working.... wait.";

            //make sure we have valid form inputs
            if (UnbindForm())
            {
                //Take the list of images and convert them to an Animated Gif
                ConvertImages();

                CompressGif();

            }
            else
            {
                MessageBox.Show("Invalid Form Data");
            }

            lblOutput.Content = "Animation Creation Completed";

        }


        private void ProcessFiles()
        {
            if (cbReduce.IsChecked.Value)
            {
                //If selected, Reduce will remove every other image in the list to make it smaller
                Reduce();
            }

            if (cbReverse.IsChecked.Value)
            {
                //Reverse the loop
                ReverseLoop();
            }
        }

        private bool UnbindForm()
        {
            try
            {
                
                //Get Temp and Output File/Folder Options
                OutputFileName = !string.IsNullOrWhiteSpace(tbGifName.Text) ? tbGifName.Text : Guid.NewGuid().ToString() + ".gif";
                if (!OutputFileName.EndsWith(".gif"))
                {
                    OutputFileName += ".gif";
                }
                OutputFilePath = OutputFolderFullPath + OutputFileName;
                OutputWidth = !string.IsNullOrWhiteSpace(tbWidth.Text) ? int.Parse(tbWidth.Text) : DefaultWidth;
                OutputHeight = !string.IsNullOrWhiteSpace(tbHeight.Text) ? int.Parse(tbHeight.Text) : DefaultHeight;
                //OutputFuzz = !string.IsNullOrWhiteSpace(tbOutputFuzz.Text) ? double.Parse(tbOutputFuzz.Text) : DefaultFuzz;
                //OutputDelay = !string.IsNullOrWhiteSpace(tbOutputDelay.Text) ? int.Parse(tbOutputDelay.Text) : DefaultDelay; 


                TempFileName = DefaultTempFileName;
                if (!TempFileName.EndsWith(".gif"))
                {
                    TempFileName += ".gif";
                }
                TempFileNamePath = TempFolderFullPath + TempFileName;
                                
                //Get Video Options
                VideoStartPosition = !string.IsNullOrWhiteSpace(tbStart.Text) ? tbStart.Text : DefaultVideoStartPostion;
                VideoDuration = !string.IsNullOrWhiteSpace(tbDuration.Text) ? int.Parse(tbDuration.Text) : DefaultDuration;
                VideoSource = !string.IsNullOrWhiteSpace(tbVideoSource.Text) ? tbVideoSource.Text : string.Empty;


                if (string.IsNullOrWhiteSpace(VideoSource))
                {
                    MessageBox.Show("No Video Source Found");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return false;
            }
        }


        /// <summary>
        /// Use MPlayer.exe to grab list of images from video
        /// </summary>
        /// <returns></returns>
        private bool ExtractImagesFromVideo()
        {
            try
            {
                string cmdMPlayer = @"/C {0} -ao null -vo png:outdir={1} -ss {2} -endpos {3} {4}";
                string cmd = string.Format(cmdMPlayer, MPlayerPath, TempFolderRelativePath, VideoStartPosition, VideoDuration, VideoSource);
                return ExecuteCMD(cmd);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return false;
            }
        }


        /// <summary>
        /// Use ImageMajik Convert to take list of files and convert them into animated gif
        /// </summary>
        /// <returns></returns>
        private void ConvertImages()
        {
            try
            {
                string resize = string.Format(" -resize {0}x{1} ", OutputWidth, OutputHeight);
                string fuzz = string.Format(" -fuzz {0}% ", OutputFuzz);
                string delay = string.Format(" -delay {0} ", OutputDelay);
                string loop = string.Format(" -loop {0} ", 0); //force all infinite loops
                string cmdImageMajik = @"/C {0} {1} {2} {3} {4} +repage {5} -layers OptimizePlus -layers OptimizeTransparency {6}";
                string cmd = string.Format(cmdImageMajik, ImageMajikPath, resize, fuzz, delay, loop, TempFolderImageList, TempFileNamePath);
                ExecuteCMD(cmd);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

        }


        /// <summary>
        /// Useses Gifsicle to compress the animated gif
        /// </summary>
        /// <returns></returns>
        private bool CompressGif()
        {
            try
            {
                string cmdGifsicle = @"/C {0} -O3 --colors 256 {1} > {2}";
                string cmd = string.Format(cmdGifsicle, GifsiclePath, TempFileNamePath, OutputFilePath);
                return ExecuteCMD(cmd);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return false;
            }
        }

        /// <summary>
        /// Delete every other file
        /// </summary>
        /// <returns></returns>
        private bool Reduce()
        {
            try
            {
                string[] fp = Directory.GetFiles(TempFolderFullPath);

                for (var i = 0; i < fp.Length; i++)
                {
                    if (i % 2 != 0)
                    {
                        File.Delete(fp[i]);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return false;
            }
            finally
            {
            }

        }

        private bool ReverseLoop()
        {
            try
            {
                string[] fp = Directory.GetFiles(TempFolderFullPath);

                Array.Reverse(fp);

                for (var i = 0; i < fp.Length; i++)
                {
                    if (i != 0 && ((i+1) != fp.Length))
                    {
                        File.Copy(fp[i], TempFolderFullPath + "z_" + string.Format("{0:d6}", i) + ".png");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return false;
            }
            finally
            {
            }
        }

        //remove all the files in the temp folder
        private bool ClearTempFolder()
        {
            try
            {
                string[] fp = Directory.GetFiles(TempFolderFullPath);

                for (var i = 0; i < fp.Length; i++)
                {
                    File.Delete(fp[i]);
                }

                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return false;
            }
            finally
            {
            }
        }

        private void HandleException(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        private bool ExecuteCMD(string cmd)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = cmd;
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                if (process != null)
                {
                    process.Close();
                    process.Dispose();
                }
            }

            return true;
        }


        

        //private const string cmdMPlayer = @"/C C:\gifmo\mplayer -ao null -vo png:outdir=/tmp -ss 00:18 -endpos 5 C:\Users\admin\Desktop\Video\test.mp4";
        //private const string cmdImageMajik = @"/C C:\gifmo\convert -resize 320x180 +repage -fuzz 1.6% -delay 5 -loop 0 /tmp/*.png -layers OptimizePlus -layers OptimizeTransparency /tmp/Almost.gif";
        //private const string cmdGifsicle = @"/C c:\gifmo\gifsicle -O3 --colors 256 /tmp/Almost.gif > /tmp/Done.gif";




        //private void btnResizeImages_Click(object sender, RoutedEventArgs e)
        //{

        //    UnbindForm();

        //    string[] fp = Directory.GetFiles(TempFolderFullPath);

        //    for (var i = 0; i < fp.Length; i++)
        //    {
        //        var img = System.Drawing.Image.FromFile(fp[i]);

        //        Bitmap bm = new Bitmap(OutputWidth, OutputHeight);

        //        var gr = Graphics.FromImage(bm);
        //        gr.DrawImage(img, 0, 0, OutputWidth, OutputHeight);

        //        bm.Save(fp[i], System.Drawing.Imaging.ImageFormat.Png);


        //        //File.Delete(fp[i]);
        //    }
        //    //OutputWidth, OutputHeight
        //}
    }
}
