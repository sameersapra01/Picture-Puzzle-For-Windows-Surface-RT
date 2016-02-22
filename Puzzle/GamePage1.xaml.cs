/**
 * File         :GamePage1.xaml.cs 
 * Project      :Puzzle
 * Date         :10-12-2014
 * Programmed By:Sameer sapra & Xiadong Meng
 * Description  : This is a windows application which is based on the concept of picture puzzle.User interact with the screen usnig touches which changes the images to the empty spot.
 * 
 * 
 * */

using Puzzle.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Animation;
using System.Threading;
using Windows.ApplicationModel.Core;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Puzzle
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GamePage1 : Page
    {

        bool won = false;
        BitmapImage bmp = new BitmapImage();
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        int[] nPos = new int[16];
        static Mutex mut = new Mutex();
        bool moveTile = false;
        Image[] MyImgages = new Image[16];
       
        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public GamePage1()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            for(int i=0;i<16;i++)
            {
                nPos[i] = i + 1;
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

  
        /// <summary>
        ///   private void choosePicture_Click(object sender, RoutedEventArgs e)
        ///   Description:  Event raised to choose an image from the computer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void choosePicture_Click(object sender, RoutedEventArgs e)
        {
            GetPicture();          
        }

   
        /// <summary>
        ///    private void captureImage_Click(object sender, RoutedEventArgs e)
        ///    Description:Event rasied to capture image from the camera.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void captureImage_Click(object sender, RoutedEventArgs e)
        {   
            CameraCapture();
        }


        /// <summary>
        ///  async private void CameraCapture()
        ///  Description:   Gets the captured image from the camera 
        /// </summary>
        async private void CameraCapture()
        {

            CameraCaptureUI cameraUI = new CameraCaptureUI();

            cameraUI.PhotoSettings.AllowCropping = false;
            cameraUI.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.MediumXga;

           StorageFile capturedMedia = await cameraUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (capturedMedia != null)
            {
                using (var streamCamera = await capturedMedia.OpenAsync(FileAccessMode.Read))
                {

                    BitmapImage bitmapImg = new BitmapImage();
                    bitmapImg.SetSource(streamCamera);

                    int width = bitmapImg.PixelWidth;
                    int height = bitmapImg.PixelHeight;

                    WriteableBitmap writableBitmap = new WriteableBitmap(width, height);

                    using (var stream = await capturedMedia.OpenAsync(FileAccessMode.Read))
                    {
                        writableBitmap.SetSource(stream);
                        showClippedImages(bitmapImg);
                        randomAllPictures();
                    }
                }
            }
        }

        /// <summary>
        ///  async private void GetPicture()
        ///  Description: gets the picture form the filepicker
        /// </summary>
        async private void GetPicture()
        {
            // Load an image
            try
            {
                FileOpenPicker openPicker = new FileOpenPicker();

                //openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;

                // Filter to include a sample subset of file types.
                openPicker.FileTypeFilter.Clear();             
                openPicker.FileTypeFilter.Add(".png");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".jpg");

                // Open the file picker.
                StorageFile file = await openPicker.PickSingleFileAsync();

                // file is null if user cancels the file picker.
                if (file != null)
                {
                    // Open a stream for the selected file.
                  IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                    // Set the image source to the selected bitmap.
                    BitmapImage bitmapImage =new   BitmapImage();

                    bitmapImage.SetSource(fileStream);
                    bitmapImage.DecodePixelHeight = 10000;

                    showClippedImages(bitmapImage);
                    randomAllPictures();

                }
            }
            catch (Exception e)
            {
             
            }
        }


        /// <summary>
        ///   public void randomAllPictures()
        ///   Description:  Randomizes the positions of the cropped images in the grid.
        /// </summary>
        public void randomAllPictures()
        {
            moveTile = true;
            Random randomPosition = new Random();          
            for (int i = 0; i < 50; i++)
            {
                int rNum = randomPosition.Next(1, 17);
                if(rNum==16)
                {
                    image14_Tapped(null, null);
                   
                }
                if (rNum == 15)
                {
                    image13_Tapped(null, null);
               
                }
                if (rNum == 14)
                {
                    image12_Tapped(null, null);
                }
                if (rNum == 13)
                {
                    image11_Tapped(null, null);
                }
                if (rNum == 12)
                {
                    image10_Tapped(null, null);
                }
                if (rNum == 11)
                {
                    image09_Tapped(null, null);
                }
                if (rNum == 10)
                {
                    image07_Tapped(null, null);
                }
                if (rNum == 9)
                {
                    image08_Tapped(null, null);
                }
                if (rNum == 8)
                {
                    image05_Tapped(null, null);
                }
                if (rNum == 7)
                {
                    image06_Tapped(null, null);
                }
                if (rNum == 6)
                {
                    image04_Tapped(null, null);
                }
                if (rNum == 5)
                {
                    image03_Tapped(null, null);
                }
                if (rNum == 4)
                {
                    image02_Tapped(null, null);
                }
                if (rNum == 3)
                {
                    image01_Tapped(null, null);
                }
                if (rNum == 3)
                {
                    image00_Tapped(null, null);
                }
            
            }
            moveTile = false;
        }


        /// <summary>
        ///   private void showClippedImages(BitmapImage img)
        ///   Description: Shows the clipped images in the grid.
        /// </summary>
        /// <param name="img"></param>
        private void showClippedImages(BitmapImage img)
        {              
            MyImgages[0] = image00;
            MyImgages[1] = image01;
            MyImgages[2] = image02;
            MyImgages[3] = image03;
            MyImgages[4] = image04;
            MyImgages[5] = image05;
            MyImgages[6] = image06;
            MyImgages[7] = image07;
            MyImgages[8] = image08;
            MyImgages[9] = image09;
            MyImgages[10] = image10;
            MyImgages[11] = image11;
            MyImgages[12] = image12;
            MyImgages[13] = image13;
            MyImgages[14] = image14;


            for (int i = 0; i < 15;i++ )
            {
                MyImgages[i].Source = img;
                MyImgages[i].Stretch = Stretch.Fill;
            }
              
            //the image on the left side
            myImage.Source = img;

            image00 = Crop(image00, 0, 0);
            image01 = Crop(image01, 1, 0);
            image02 = Crop(image02, 2, 0);
            image03 = Crop(image03, 3, 0);
            image04 = Crop(image04, 0, 1);
            image05 = Crop(image05, 1, 1);
            image06 = Crop(image06, 2, 1);
            image07 = Crop(image07, 3, 1);
            image08 = Crop(image08, 0, 2);
            image09 = Crop(image09, 1, 2);
            image10 = Crop(image10, 2, 2);
            image11 = Crop(image11, 3, 2);
            image12 = Crop(image12, 0, 3);
            image13 = Crop(image13, 1, 3);
            image14 = Crop(image14, 2, 3);

            myImage.Visibility = Visibility.Visible;
            for(int i =0;i<15;i++)
            {
                MyImgages[i].Visibility = Visibility.Visible;
            }
         
        }

        /// <summary>
        ///  void checkWinningCondition()
        ///  Descritpion: Checks the winning consdition of the game in the thread.
        /// </summary>
        void checkWinningCondition()
        {
            IAsyncAction asynAction = Windows.System.Threading.ThreadPool.RunAsync(
           (workitem) =>
           {
   
                    bool bresult = true;         
                    while (bresult)
                    {
                        int i=0;
                        for ( i = 0; i < 16; i++)
                        {
                            if (nPos[i] == i + 1)
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (i == 16)
                        {
                            bresult = false;
                        }
                      
                    }
                    if (!bresult)
                    {
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
                        {
                        winMessage.Visibility = Visibility.Visible;
                        playAgainButton.Visibility = Visibility.Visible;
                        playButton.IsEnabled = false;
                        choosePicture.IsEnabled = false;
                        captureImage.IsEnabled = false;
                        moveTile = false;
                        won = true;
                    
                        }));
                    }

           });              
        }
            
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tmp">original image to be cropped</param>
        /// <param name="x">X-position</param>
        /// <param name="y">Y-position</param>
        /// <returns></returns>
        public Image Crop(Image tmp, int x, int y)
        {
            Image img = tmp;

        
            img.Clip = new RectangleGeometry();
            img.Clip.Rect = new Rect(148 * x, 111 * y, 148, 111);

            return img;
        }

  
        /// <summary>
        ///    private void image14_Tapped(object asender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="asender"></param>
        /// <param name="e"></param>
        private void image14_Tapped(object asender, TappedRoutedEventArgs e)
        {           
            ImageTapped(14, ref image14);           
        }


        /// <summary>
        ///   private void image13_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image13_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(13, ref image13);           

        }

        /// <summary>
        ///   private void image12_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image12_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(12, ref image12);
    
        }


        /// <summary>
        /// private void image11_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image11_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(11, ref image11);
           
        }

        /// <summary>
        ///   private void image10_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image10_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(10, ref image10);
           
        }

        /// <summary>
        ///   private void image09_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void image09_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(9, ref image09);
           
        }

        /// <summary>
        ///   private void image08_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void image08_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(8, ref image08);
           
        }

        /// <summary>
        ///  private void image07_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image07_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(7, ref image07);
           
        }

        /// <summary>
        ///     private void image06_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void image06_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(6, ref image06);
           
        }


        /// <summary>
        ///  private void image05_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void image05_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(5, ref image05);
           
        }

        /// <summary>
        ///   private void image04_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image04_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(4, ref image04);
        }

        /// <summary>
        ///    private void image02_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image02_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(2, ref image02);
           
        }

        /// <summary>
        ///  private void image03_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image03_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(3, ref image03);
           
        }


        /// <summary>
        ///   private void image01_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image01_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(1, ref image01);
          
        }

        /// <summary>
        ///   private void image00_Tapped(object sender, TappedRoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image00_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageTapped(0, ref image00);                               
        }

        /// <summary>
        ///   private void ImageTapped(int index, ref Image image)
        ///   Description:  Function is called to change the position of the tile.
        /// </summary>
        /// <param name="index">point of the image</param>
        /// <param name="image">which image to move</param>
        private void ImageTapped(int index, ref Image image)
        {
            if (moveTile)
            {
                mut.WaitOne();

                if (nPos[index] + 1 == nPos[15] && nPos[15] != 1 && nPos[15] != 5 && nPos[15] != 9 && nPos[15] != 13)
                {
                    image.Margin = new Thickness(image.Margin.Left + 148, image.Margin.Top, 0, 0);
                    nPos[index] = nPos[index] + 1;
                    nPos[15] = nPos[15] - 1;

                }
                else if (nPos[index] - 1 == nPos[15] && nPos[15] != 4 && nPos[15] != 8 && nPos[15] != 12 && nPos[15] != 16)
                {
                    image.Margin = new Thickness(image.Margin.Left - 148, image.Margin.Top, 0, 0);
                    nPos[index] = nPos[index] - 1;
                    nPos[15] = nPos[15] + 1;

                }

                else if (nPos[index] + 4 == nPos[15])
                {
                    image.Margin = new Thickness(image.Margin.Left, image.Margin.Top + 111, 0, 0);
                    nPos[index] = nPos[index] + 4;
                    nPos[15] = nPos[15] - 4;

                }
                else if (nPos[index] - 4 == nPos[15])
                {
                    image.Margin = new Thickness(image.Margin.Left, image.Margin.Top - 111, 0, 0);
                    nPos[index] = nPos[index] - 4;
                    nPos[15] = nPos[15] + 4;

                }
                mut.ReleaseMutex();
            }
        }

        

        /// <summary>
        ///  private void playButton_Click(object sender, RoutedEventArgs e)
        /// Description:Game starts.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            myImage.Visibility = Visibility.Collapsed;
            moveTile = true;
            if (!won)
            {
                checkWinningCondition();
            }
        }

        /// <summary>
        /// private void playAgainButton_Click(object sender, RoutedEventArgs e)
        /// Description: To let the suer to play the game after winning it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playAgainButton_Click(object sender, RoutedEventArgs e)
        {
            winMessage.Visibility = Visibility.Collapsed;
            playAgainButton.Visibility = Visibility.Collapsed;
            for(int i=0;i<15;i++)
            {
                MyImgages[i].Visibility = Visibility.Collapsed;
            }
            playButton.IsEnabled = true;
            choosePicture.IsEnabled = true;
            captureImage.IsEnabled = true;
        }
    }
}
