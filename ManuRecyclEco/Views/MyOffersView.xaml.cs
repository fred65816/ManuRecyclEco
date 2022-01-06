using System;
using Microsoft.Win32;
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
    /// Interaction logic for MyOffersView.xaml
    /// </summary>
    public partial class MyOffersView : UserControl
    {
        public MyOffersView()
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

            Button_Click(sender, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var listBoxItem = (ListBoxItem)lstBx.ItemContainerGenerator.ContainerFromItem(lstBx.SelectedItem);
            listBoxItem.Focus();
        }
    }
}
