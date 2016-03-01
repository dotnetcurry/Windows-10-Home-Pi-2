using HomePi.Class;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Spi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HomePi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        enum CurrentPage
        {
            Weather,
            NextBus,
            MusicPlayer,
            Power
        };

        string tempC = string.Empty;
        DispatcherTimer appTimer, touchTimer;
        const string CalibrationFilename = "TSC2046";
        Windows.Foundation.Point lastPosition = new Windows.Foundation.Point(double.NaN, double.NaN);
        int nextBus = 0, nowPlaying = 0;
        CurrentPage currenPage;
        List<SoundCloudTrack> likes = new List<SoundCloudTrack>();

        const Int32 DCPIN = 22;
        const Int32 RESETPIN = 27;

        ILI9341Display display1 = new ILI9341Display(ILI9341Display.BLACK, DCPIN, RESETPIN);

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Init();
            base.OnNavigatedTo(e);
        }

        private void initTimers()
        {
            appTimer = new DispatcherTimer();
            appTimer.Tick += AppTimer_Tick;
            appTimer.Interval = TimeSpan.FromMinutes(1);
            appTimer.Start();

            touchTimer = new DispatcherTimer();
            touchTimer.Tick += TouchTimer_Tick;
            touchTimer.Interval = TimeSpan.FromMilliseconds(50);
            touchTimer.Start();

        }

        private void TouchTimer_Tick(object sender, object e)
        {
            TSC2046.CheckTouch();

            int x = TSC2046.getTouchX();
            int y = TSC2046.getTouchY();
            int p = TSC2046.getPressure();

            if (p > 5)
            {
                CheckAction(TSC2046.getDispX(), TSC2046.getDispY());
            }
        }

        private async void AppTimer_Tick(object sender, object e)
        {
            //Run only if the current page is Next Bus
            switch (currenPage)
            {
                case CurrentPage.NextBus:
                    //Erase screen
                    ILI9341.fillRect(display1, 0, 0, 240, 70, 0xC616);
                    nextBus -= 1;
                    if (nextBus <= 2)
                    {
                        nextBus = await Utilities.GetNextBus();
                    }

                    ILI9341.setCursor(display1, 30, 15);
                    string s = nextBus.ToString() + " min";
                    ILI9341.write(display1, s.ToCharArray(), 6, 0xDB69);
                    break;
            }
        }


        private void CheckAction(int x, int y)
        {
            Windows.Foundation.Point touchPoint = new Windows.Foundation.Point(x, y);
            Rect rect;
            bool isControlpoint;

            switch (currenPage)
            {
                case CurrentPage.Power:
                    rect = new Rect(64, 53, 108, 110); //Power button is within this rectangle area
                    isControlpoint = rect.Contains(touchPoint);
                    if (isControlpoint)
                    {
                        ShutDown();
                    }
                    break;
            }

            //Menu controls
            rect = new Rect(275, 20, 40, 40); //Next bus Icon is within this rectangle area
            isControlpoint = rect.Contains(touchPoint);
            if (isControlpoint)
            {
                GetNextBus();
            }

            rect = new Rect(275, 72, 40, 40); //Music player Icon is within this rectangle area
            isControlpoint = rect.Contains(touchPoint);
            if (isControlpoint)
            {
                PlayMusic();
            }
            rect = new Rect(275, 125, 40, 40); //Weather Icon is within this rectangle area
            isControlpoint = rect.Contains(touchPoint);
            if (isControlpoint)
            {
                GetWeather();
            }

            rect = new Rect(275, 179, 40, 40);
            isControlpoint = rect.Contains(touchPoint); //Power button is within this rectangle area
            if (isControlpoint)
            {
                ShowPowerPage();
            }

        }

        private void ShutDown()
        {

            ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0.5));
        }

        private async void CalibrateTouch()
        {
            //5 point calibration
            CAL_POINT[] touchPoints = new CAL_POINT[5];
            touchPoints[0] = new CAL_POINT();
            touchPoints[1] = new CAL_POINT();
            touchPoints[2] = new CAL_POINT();
            touchPoints[3] = new CAL_POINT();
            touchPoints[4] = new CAL_POINT();

            CAL_POINT[] screenPoints = new CAL_POINT[5];
            screenPoints[0] = new CAL_POINT();
            screenPoints[1] = new CAL_POINT();
            screenPoints[2] = new CAL_POINT();
            screenPoints[3] = new CAL_POINT();
            screenPoints[4] = new CAL_POINT();

            ILI9341.fillRect(display1, 0, 0, 240, 320, 0x0000);

            ILI9341.LineDrawH(display1, 30, 30, 1, 0xFFFF);
            ILI9341.LineDrawV(display1, 30, 30, 1, 0xFFFF);
            while (TSC2046.pressure < 5) { TSC2046.CheckTouch(); }//wait for pen pressure
            screenPoints[0].x = 30;
            screenPoints[0].y = 30;
            touchPoints[0].x = TSC2046.tp_x;
            touchPoints[0].y = TSC2046.tp_y;
            while (TSC2046.pressure > 1) { TSC2046.CheckTouch(); } // wait for release of pen


            ILI9341.LineDrawH(display1, 30, 300, 1, 0xFFFF);
            ILI9341.LineDrawV(display1, 30, 300, 1, 0xFFFF);
            while (TSC2046.pressure < 5) { TSC2046.CheckTouch(); }//wait for pen pressure
            screenPoints[1].x = 300;
            screenPoints[1].y = 30;
            touchPoints[1].x = TSC2046.tp_x;
            touchPoints[1].y = TSC2046.tp_y;
            while (TSC2046.pressure > 1) { TSC2046.CheckTouch(); }// wait for release of pen

            ILI9341.LineDrawH(display1, 120, 160, 1, 0xFFFF);
            ILI9341.LineDrawV(display1, 120, 160, 1, 0xFFFF);
            while (TSC2046.pressure < 5) { TSC2046.CheckTouch(); }//wait for pen pressure
            screenPoints[2].x = 160;
            screenPoints[2].y = 120;
            touchPoints[2].x = TSC2046.tp_x;
            touchPoints[2].y = TSC2046.tp_y;
            while (TSC2046.pressure > 1) { TSC2046.CheckTouch(); }// wait for release of pen


            ILI9341.LineDrawH(display1, 210, 30, 1, 0xFFFF);
            ILI9341.LineDrawV(display1, 210, 30, 1, 0xFFFF);
            while (TSC2046.pressure < 5) { TSC2046.CheckTouch(); }//wait for pen pressure
            screenPoints[3].x = 30;
            screenPoints[3].y = 210;
            touchPoints[3].x = TSC2046.tp_x;
            touchPoints[3].y = TSC2046.tp_y;
            while (TSC2046.pressure > 1) { TSC2046.CheckTouch(); }// wait for release of pen


            ILI9341.LineDrawH(display1, 210, 300, 1, 0xFFFF);
            ILI9341.LineDrawV(display1, 210, 300, 1, 0xFFFF);
            while (TSC2046.pressure < 5) { TSC2046.CheckTouch(); }//wait for pen pressure
            screenPoints[4].x = 300;
            screenPoints[4].y = 210;
            touchPoints[4].x = TSC2046.tp_x;
            touchPoints[4].y = TSC2046.tp_y;
            while (TSC2046.pressure > 1) { TSC2046.CheckTouch(); }// wait for release of pen

            TSC2046.setCalibration(screenPoints, touchPoints);
            if (await TSC2046.CalibrationMatrix.SaveCalData(CalibrationFilename))
            {
                //Success
            }
            else
            {
                //Handle error
            }
            ILI9341.Flush(display1);
        }

        private async void GetWeather()
        {
            currenPage = CurrentPage.Weather;
            //paint Menu
            await ILI9341.LoadBitmap(display1, 0, 0, 240, 320, "ms-appx:///assets/Home.png");

            //Write loading
            ILI9341.setCursor(display1, 60, 120);
            ILI9341.write(display1, "Checking...".ToCharArray(), 2, 0x86D2);

            //Get Weather
            string responseText = await Utilities.GetjsonStream("http://api.worldweatheronline.com/free/v2/weather.ashx?q=Vienna, Austria&format=JSON&extra=&num_of_days=2&date=&fx=&cc=&includelocation=&show_comments=&callback=&key=4b3c8aaa970207d3e4a5c7fb6d514");
            LocalWeather localWeather = JsonConvert.DeserializeObject<LocalWeather>(responseText);

            tempC = string.Empty;
            tempC = localWeather.data.current_Condition[0].temp_C.ToString();

            //Clear
            ILI9341.fillRect(display1, 0, 0, 240, 150, 0xFFFF);

            //Get weather icon
            XElement iconXml = XElement.Load("wwoConditionCodes.xml");
            var iconditions = iconXml.Elements("condition");
            var iconCodeElement = iconditions.Where(i => i.Element("code").Value == localWeather.data.current_Condition[0].weatherCode).FirstOrDefault();

            string iconFile = string.Empty;
            if (DateTime.Now.Hour < 18)
            {
                iconFile = iconCodeElement.Element("day_icon").Value;
            }
            else
            {
                iconFile = iconCodeElement.Element("night_icon").Value;
            }

            iconFile = "ms-appx:///assets/Weather Icons/" + iconFile.Trim() + ".png";

            //Display Icon
            await ILI9341.LoadBitmap(display1, 60, 20, 120, 120, iconFile);

            if (Convert.ToInt16(tempC) > 9)
            {
                ILI9341.setCursor(display1, 80, 150);
            }
            else
            {
                ILI9341.setCursor(display1, 100, 150);
            }
            ILI9341.write(display1, tempC.ToCharArray(), 8, 0xE2C9);

            //Second row can display 20 characters
            //Calculate spacing

            int count = localWeather.data.current_Condition[0].weatherDesc[0].value.ToString().Length;

            string weatherDesc = localWeather.data.current_Condition[0].weatherDesc[0].value.ToString();
            UInt16 spacing = 12;
            if (weatherDesc.Length > 18)
            {
                weatherDesc = weatherDesc.Substring(0, 15) + "...";
            }
            else
            {
                int counter = ((18 - weatherDesc.Length) / 2) + 1;
                spacing += (UInt16)(spacing * counter);
            }

            ILI9341.setCursor(display1, spacing, 230);
            ILI9341.write(display1, weatherDesc.ToCharArray(), 2, 0xE2C9);

        }

        private async void PlayMusic()
        {
            currenPage = CurrentPage.MusicPlayer;
            //paint Menu
            await ILI9341.LoadBitmap(display1, 0, 0, 240, 320, "ms-appx:///assets/Music_Home.png");
            likes = await Utilities.GetLikes();
            if (likes.Count > 0)
            {
                LoadTrack(likes[nowPlaying]);
            }
        }

        private async void GetNextBus()
        {
            currenPage = CurrentPage.NextBus;
            await ILI9341.LoadBitmap(display1, 0, 0, 240, 320, "ms-appx:///assets/Bus_Home.png");
            nextBus = await Utilities.GetNextBus();
            ILI9341.setCursor(display1, 30, 15);
            string s = nextBus.ToString() + " min";
            ILI9341.write(display1, s.ToCharArray(), 6, 0xDB69);
        }

        private async void ShowPowerPage()
        {
            currenPage = CurrentPage.Power;
            await ILI9341.LoadBitmap(display1, 0, 0, 240, 320, "ms-appx:///assets/Power_Home.png");
        }


        private async void Init()
        {
            await ILI9341.InitILI9341DisplaySPI(display1, 0, 50000000, SpiMode.Mode0, "SPI0", "");
            await TSC2046.InitTSC2046SPI();
            initTimers();

            if (!await TSC2046.CalibrationMatrix.LoadCalData(CalibrationFilename))
            {
                CalibrateTouch();
            }

            //Paint background
            ILI9341.fillRect(display1, 0, 0, 240, 320, 0xFFFF);
            ILI9341.Flush(display1);

            GetNextBus();

        }

        private void mPlayer_MediaEnded(System.Object sender, RoutedEventArgs e)
        {
            nowPlaying += 1;
            if (nowPlaying > likes.Count) //Reset to first track
                nowPlaying = 0;
            LoadTrack(likes[nowPlaying]);
        }

        private void LoadTrack(SoundCloudTrack currentTrack)
        {
            //Stop player, set new stream uri and play track
            mPlayer.Stop();
            Uri streamUri = new Uri(currentTrack.stream_url + "?client_id=YOUR_CLIENT_ID_HERE");
            mPlayer.Source = streamUri;
            mPlayer.Play();
        }

    }

}

