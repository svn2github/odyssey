using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JediTest
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
          //  ApplySkin();
        }

        private void ApplySkin()
        {
            string Skin = @"C:\Temp\Luna.xaml";
            Uri uri = new Uri(Skin, UriKind.Absolute);
            ResourceDictionary skin = new ResourceDictionary();
            skin.Source = uri;
            explorerBar.Resources = skin;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Background = Brushes.LightSteelBlue;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Background = Brushes.Gold;
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            expander1.IsMinimized ^= true;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            expander2.IsMinimized ^= true;
        }

        private void Animate1Click(object sender, RoutedEventArgs e)
        {
            expander1.IsMinimized ^= true;
            expander2.IsExpanded ^= true;

        }
        private void Animate2Click(object sender, RoutedEventArgs e)
        {
            expander1.IsExpanded ^= true;
            expander2.IsExpanded ^= true;
        }
    }
}
