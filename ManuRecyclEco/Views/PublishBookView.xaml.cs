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
using Microsoft.Win32;

namespace ManuRecyEco.Views
{
    /// <summary>
    /// Interaction logic for PublishBookView.xaml
    /// </summary>
    public partial class PublishBookView : UserControl
    {
        public PublishBookView()
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
    }
}
