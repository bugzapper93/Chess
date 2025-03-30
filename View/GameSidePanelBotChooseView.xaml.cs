using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Chess.View
{
    public partial class GameSidePanelBotChooseView : UserControl
    {
        private int minutes;
        public GameSidePanelBotChooseView()
        {
            InitializeComponent();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider != null && lblSlider != null)
            {
                lblSlider.Content = ((int)slider.Value).ToString();
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
    }
}
