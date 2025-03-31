using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Chess.Objects;
using Chess.Tools;

namespace Chess.View
{
    public partial class GameSidePanelBotChooseView : UserControl
    {
        private int minutes;
        public int chosenColor;
        public string chosenName;
        public GameSidePanelBotChooseView()
        {
            InitializeComponent();

            var GMs = Constants.Grandmasters.Keys.ToList();
            bots.ItemsSource = GMs;

            string resource = Pieces.ResourceNames['K'];
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.EndInit();

                whiteImg.Source = bitmap;
            }

            resource = Pieces.ResourceNames['k'];
            assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.EndInit();

                blackImg.Source = bitmap;
            }

            chosenColor = Pieces.White;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(slider != null && lblSlider != null)
            {
                int newValue = (int)slider.Value;
                lblSlider.Content = newValue.ToString();
            }
        }
        public TimeSpan GetSelectedTime()
        {
            return TimeSpan.FromMinutes(minutes);
        }

        private void TenMinutesRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if(TenMinutesRadioButton.IsChecked == true)
            {
                minutes = 10;
            }
        }

        private void FiveMinutesRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (FiveMinutesRadioButton.IsChecked == true)
            {
                minutes = 5;
            }
        }
        public int SelectedDepth
        {
            get
            {
                if (slider != null)
                {
                    return (int)slider.Value;
                }
                return 5; 
            }
        }

        private void SwapColorBlack(object sender, RoutedEventArgs e)
        {
            if (chosenColor == Pieces.White)
                chosenColor = Pieces.Black;
        }

        private void SwapColorWhite(object sender, RoutedEventArgs e)
        {
            if (chosenColor == Pieces.Black)
                chosenColor = Pieces.White;
        }
    }
}
