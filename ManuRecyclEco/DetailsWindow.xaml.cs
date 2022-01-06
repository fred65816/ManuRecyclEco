using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using ManuRecyEco.Models;

namespace ManuRecyEco
{
    /// <summary>
    /// Interaction logic for DetailsWindow.xaml
    /// </summary>
    public partial class DetailsWindow : Window
    {
        private BookCopy _exemplaire;
        public BookCopy Exemplaire
        {
            get { return _exemplaire; }
            set { _exemplaire = value; }
        }

        private ImageSource _bookImage;
        public ImageSource BookImage
         {
            get { return _bookImage; }
            set { _bookImage = value; }
        }

        private string _prix;
        public string Prix
        {
            get { return _prix; }
            set { _prix = value; }
        }

        private string _prixReference;
        public string PrixReference
        {
            get { return _prixReference; }
            set { _prixReference = value; }
        }

        private string _condition;
        public string Condition 
        {
            get { return _condition; }
            set { _condition = value; }
        }

        public DetailsWindow(BookCopy exemplaire)
        {
            Exemplaire = exemplaire;
            BookImage = exemplaire.ImageCopy.BlobToImage();
            Prix = exemplaire.TransactionType == "Échange" ? "N/A" : exemplaire.Price.ToString() + "$";
            PrixReference = exemplaire.Book.ReferencePrice.ToString() + "$";
            Condition = exemplaire.Condition + "/10";
            InitializeComponent();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
