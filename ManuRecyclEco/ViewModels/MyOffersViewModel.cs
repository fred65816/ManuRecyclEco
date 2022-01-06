using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ManuRecyEco.ViewModels
{
    public class MyOffersViewModel : ObservableObject
    {

        private ApplicationDbContext db;

        private int CurrentUserId;

        // timer pour les messages
        private DispatcherTimer timer;

        private TimeSpan time;

        // classe condition
        public class Condition : ObservableObject
        {
            private int _index;
            public int Index
            {
                get { return _index; }
                set { OnPropertyChanged(ref _index, value); }
            }
            private string _description;
            public string Description
            {
                get { return _description; }
                set { OnPropertyChanged(ref _description, value); }
            }

            public override string ToString()
            {
                return Description;
            }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set { OnPropertyChanged(ref _currentUser, value); }
        }

        private MainWindowViewModel _mainWindowVM;
        public MainWindowViewModel MainWindowVM
        {
            get { return _mainWindowVM; }
            set { OnPropertyChanged(ref _mainWindowVM, value); }
        }

        // liste de livres en vente/echange du User
        private List<BookCopy> _userBookCopyList;
        public List<BookCopy> UserBookCopyList
        {
            get { return _userBookCopyList; }
            set 
            { 
                OnPropertyChanged(ref _userBookCopyList, value);

                IsNotTrading = true;
            }
        }

        // livre à modifier sélectionné
        private BookCopy _selectedBookCopy;
        public BookCopy SelectedBookCopy
        {
            get { return _selectedBookCopy; }
            set
            {
                MessageVisibility = Visibility.Hidden;
                OnPropertyChanged(ref _selectedBookCopy, value);

                //Updater l'affichage des champs à modifier selon le livre sélectionné
                SelectedCondition = ConditionList.Where(co => co.Index == SelectedBookCopy.Condition).Single();
                SelectedTransaction = TransactionList.Where(t => t == SelectedBookCopy.TransactionType).Single();
                StrPrice = SelectedBookCopy.Price.ToString() + "$";

                if(SelectedTransaction == AppStrings.TransactionEchange)
                {
                    SelectedBookCopy.Price = 0;
                    StrPrice = String.Empty;
                    IsNotTrading = false;
                }
                else
                {
                    StrPrice = SelectedBookCopy.Price.ToString() + "$";
                    IsNotTrading = true;
                }
                
                Book book = db.Books.Where(b => b.Id == SelectedBookCopy.BookId).Single();
                StrRefPrice = "" + book.ReferencePrice + "$";
                ImageSource = SelectedBookCopy.ImageCopy.BlobToImage();

                // si on a pas de sélection on set
                // le bouton supprimé à désactivé
                if (_userBookCopyList == null)
                {
                    DeleteButtonEnabled = false;
                }
                else
                {
                    DeleteButtonEnabled = true;
                }

                // hardcode de 93 images parce que c'est pas tous
                // les livres qui sont dans db.Books
                if(SelectedBookCopy.ImageCopyId <= 93)
                {
                    DeletePictureButtonEnabled = false;
                }
                else
                {
                    DeletePictureButtonEnabled = true;
                }
            }
        }

        // type de transaction choisi dans la liste déroulante
        private string _selectedTransaction;
        public string SelectedTransaction
        {
            get { return _selectedTransaction; }
            set 
            {
                MessageVisibility = Visibility.Hidden;

                OnPropertyChanged(ref _selectedTransaction, value);

                if (SelectedTransaction == AppStrings.TransactionEchange)
                {
                    SelectedBookCopy.Price = 0;
                    StrPrice = String.Empty;
                    IsNotTrading = false;
                }
                else
                {
                    StrPrice = SelectedBookCopy.Price.ToString() + "$";
                    IsNotTrading = true;
                }
            }
        }

        // liste des types de transactions de la liste déroulante
        private List<string> _transactionList;
        public List<string> TransactionList
        {
            get { return _transactionList; }
            set { OnPropertyChanged(ref _transactionList, value); }
        }

        // true si on fait un échange
        private bool _isNotTrading;
        public bool IsNotTrading
        {
            get { return _isNotTrading; }
            set { OnPropertyChanged(ref _isNotTrading, value); }
        }

        // condition choisie dans la liste déroulante
        private Condition _selectedCondition;
        public Condition SelectedCondition
        {
            get { return _selectedCondition; }
            set
            {
                MessageVisibility = Visibility.Hidden;
                OnPropertyChanged(ref _selectedCondition, value);
            }
        }

        // liste des conditions de la liste déroulante
        private List<Condition> _conditionList;
        public List<Condition> ConditionList
        {
            get { return _conditionList; }
            set { OnPropertyChanged(ref _conditionList, value); }
        }

        // prix sélectionné
        private string _strPrice;
        public string StrPrice
        {
            get { return _strPrice; }
            set
            {
                MessageVisibility = Visibility.Hidden;
                OnPropertyChanged(ref _strPrice, value);
            }
        }

        // prix de référence formatté
        private string _strRefPrice;
        public string StrRefPrice
        {
            get { return _strRefPrice; }
            set { OnPropertyChanged(ref _strRefPrice, value); }
        }

        private string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                MessageVisibility = Visibility.Hidden;

                OnPropertyChanged(ref _imagePath, value);

                if (_imagePath != String.Empty)
                {
                    ImageSource = new BitmapImage(new Uri(_imagePath));
                }
            }
        }
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set
            {
                OnPropertyChanged(ref _imageSource, value);
            }
        }

        // true s'il y a un cours selectionné
        private bool _deleteButtonEnabled;
        public bool DeleteButtonEnabled
        {
            get { return _deleteButtonEnabled; }
            set { OnPropertyChanged(ref _deleteButtonEnabled, value); }
        }

        // true si la photo n'est pas celle par défaut
        private bool _deletePictureButtonEnabled;
        public bool DeletePictureButtonEnabled
        {
            get { return _deletePictureButtonEnabled; }
            set { OnPropertyChanged(ref _deletePictureButtonEnabled, value); }
        }

        // contenu du message
        private string _messageContent;
        public string MessageContent
        {
            get { return _messageContent; }
            set { OnPropertyChanged(ref _messageContent, value); }
        }

        // pour masquer ou afficher le message
        private Visibility _messageVisibility;
        public Visibility MessageVisibility
        {
            get { return _messageVisibility; }
            set
            {
                if (value == Visibility.Visible)
                {
                    StartTimer();
                }
                else
                {
                    StopTimer();
                }
                OnPropertyChanged(ref _messageVisibility, value);
            }
        }

        public ICommand UpdateOfferCommand { get; private set; }
        public ICommand RemoveOfferCommand { get; private set; }
        public ICommand RemoveOfferPicture { get; private set; }

        public MyOffersViewModel(int CurrentUserId)
        {
            db = new ApplicationDbContext();

            this.CurrentUserId = CurrentUserId;

            // message caché
            MessageVisibility = Visibility.Hidden;
            MessageContent = String.Empty;

            // liste des livres mis en vente par le User
            UserBookCopyList = db.BookCopies.Where(bc => bc.UserId == CurrentUserId).ToList();

            // choix de transactions
            TransactionList = new List<string>() { AppStrings.TransactionVente,
                                                   AppStrings.TransactionEchange,
                                                   AppStrings.TransactionVenteEchange };

            // choix pour l'état du livre
            ConditionList = new List<Condition>() { new Condition { Index = 1, Description = "1 (très mauvais état)" },
                                                    new Condition { Index = 2, Description = "2" },
                                                    new Condition { Index = 3, Description = "3" },
                                                    new Condition { Index = 4, Description = "4" },
                                                    new Condition { Index = 5, Description = "5 (état moyen)" },
                                                    new Condition { Index = 6, Description = "6" },
                                                    new Condition { Index = 7, Description = "7" },
                                                    new Condition { Index = 8, Description = "8" },
                                                    new Condition { Index = 9, Description = "9" },
                                                    new Condition { Index = 10, Description = "10 (état neuf)" } };

            // vide au début jusqu'à ce
            // qu'on upload une image
            ImagePath = String.Empty;

            UpdateOfferCommand = new RelayCommand(UpdateOffer);
            RemoveOfferCommand = new RelayCommand(RemoveOffer);
            RemoveOfferPicture = new RelayCommand(RemovePicture);
        }

        // Sauvegarder modification sur l'offre
        private void UpdateOffer()
        {
            SelectedBookCopy.TransactionType = SelectedTransaction;
            SelectedBookCopy.Condition = SelectedCondition.Index;
            if(SelectedBookCopy.TransactionType != AppStrings.TransactionEchange)
            {
                // Pour un bug vraiment weird quand j'update une photo
                if (StrPrice.EndsWith('$'))
                {
                    SelectedBookCopy.Price = Convert.ToDouble(StrPrice.Remove(StrPrice.Length - 1));
                }
                else
                {
                    SelectedBookCopy.Price = Convert.ToDouble(StrPrice);
                }
                
                IsNotTrading = true;
            } 
            else
            {
                // effacer le contenu et mettre disabled le TextBox de prix
                SelectedBookCopy.Price = 0;
                IsNotTrading = false;
                StrPrice = String.Empty;
            }
            
            ImageCopy copy = new ImageCopy();

            // traitement de l'image
            if (ImagePath != String.Empty)
            {
                string extension = Path.GetExtension(ImagePath).ToLower();
                copy.ImageToBlob(ImagePath, extension);
                int id = db.ImageCopies.OrderByDescending(ic => ic.Id).Select(ic => ic.Id).First();
                id = id < 94 ? 94 : id;
                id++;
                copy.Id = id;
                db.Add(copy);
                db.SaveChanges();

                SelectedBookCopy.ImageCopy = copy;
                SelectedBookCopy.ImageCopyId = copy.Id;

                DeletePictureButtonEnabled = true;
                ImagePath = string.Empty;
            }

            db.BookCopies.Update(SelectedBookCopy);
            db.SaveChanges();
            MessageContent = AppStrings.ProfilUpdated;
            MessageVisibility = Visibility.Visible;

            UserBookCopyList = db.BookCopies.Where(bc => bc.UserId == CurrentUserId).ToList();
        }

        // Supprimer offre
        private void RemoveOffer()
        {
            db.BookCopies.Remove(SelectedBookCopy);
            db.SaveChanges();

            //Important pour éviter bug de SelectedBookCopy qui retourne null à l'affichage.
            SelectedBookCopy = UserBookCopyList.ElementAt(0);
            UserBookCopyList = db.BookCopies.Where(x => x.UserId == CurrentUserId).ToList();

            MessageContent = AppStrings.RemoveOffer;
            MessageVisibility = Visibility.Visible;
        }

        // Supprimer image de l'offre sélectionnée
        private void RemovePicture()
        {
            SelectedBookCopy.ImageCopy = db.Books.
                Where(b => b.Id == SelectedBookCopy.BookId).
                    Select(b => b.ImageCopy).Single();

            SelectedBookCopy.ImageCopyId = db.Books.
                Where(b => b.Id == SelectedBookCopy.BookId).
                    Select(b => b.ImageCopyId).Single();

            db.BookCopies.Update(SelectedBookCopy);
            db.SaveChanges();
            MessageContent = AppStrings.PictureRemoved;
            MessageVisibility = Visibility.Visible;

            ImageSource = SelectedBookCopy.ImageCopy.BlobToImage();

            DeletePictureButtonEnabled = false;
        }

        private void StartTimer()
        {
            // le message reste affiché 2 secondes
            time = TimeSpan.FromSeconds(2);

            // création du timer dans laquelle on passe une méthode anonyme qui check le countdown
            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                // si le countdown est à 0
                if (time == TimeSpan.Zero)
                {
                    // on cache le message et arrête le timer
                    MessageVisibility = Visibility.Hidden;
                    StopTimer();
                }

                // on enlève une seconde au countdown
                time = time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            timer.Start();
        }

        // arrête le timer
        private void StopTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }
    }
}