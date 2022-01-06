using ManuRecyEco.Utility;
using Microsoft.Win32;
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

namespace ManuRecyEco.Views
{
    /// <summary>
    /// Interaction logic for ProfileView.xaml
    /// </summary>
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        private void ImageUpload_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "PNG|*.png|JPEG|*.jpg";
            if (dialog.ShowDialog() == true)
            {
                if (this.DataContext != null)
                {
                    ((dynamic)this.DataContext).ImagePath = dialog.FileName;
                }
            }
        }

        private void Change_Style(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbStyle = sender as ComboBox;

            if (cmbStyle.SelectedItem != null && !((dynamic)this.DataContext).IsInit)
            {
                ResourceDictionary Styles = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.StylePath, UriKind.Relative));
                
                Application.Current.Resources.MergedDictionaries.Clear();

                if (cmbStyle.SelectedIndex == 0)
                {
                    ResourceDictionary theme = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.DefaultThemePath, UriKind.Relative));
                    Application.Current.Resources.MergedDictionaries.Add(theme);
                }
                else if (cmbStyle.SelectedIndex == 1)
                {
                    ResourceDictionary theme = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.Theme1Path, UriKind.Relative));
                    Application.Current.Resources.MergedDictionaries.Add(theme);
                }
                else if (cmbStyle.SelectedIndex == 2)
                {
                    ResourceDictionary theme = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.Theme2Path, UriKind.Relative));
                    Application.Current.Resources.MergedDictionaries.Add(theme);
                }
                else if (cmbStyle.SelectedIndex == 3)
                {
                    ResourceDictionary theme = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.Theme3Path, UriKind.Relative));
                    Application.Current.Resources.MergedDictionaries.Add(theme);
                }

                Application.Current.Resources.MergedDictionaries.Add(Styles);
            }
        }
    }
}
