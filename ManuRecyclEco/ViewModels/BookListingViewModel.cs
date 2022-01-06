using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ManuRecyEco.ViewModels
{
    public class BookListingViewModel: ObservableObject
    {
        private const int BOOKS_PER_PAGE = 10;

        private ApplicationDbContext db;

        private int CurrentUserId;

        private int CurrentIndex;

        private int CurrentPage;

        private int MaxPage;

        private List<BookCopy> Exemplaires;

        // liste de cours
        private List<AcademicCourse> _courseList;
        public List<AcademicCourse> CourseList
        {
            get { return _courseList; }
            set { OnPropertyChanged(ref _courseList, value); }
        }

        // cours sélectionné
        private AcademicCourse _selectedCourse;
        public AcademicCourse SelectedCourse
        {
            get { return _selectedCourse; }
            set
            {
                OnPropertyChanged(ref _selectedCourse, value);

                IsUpdatingCourse = true;

                // si on a au moins 1 cours, donc au moins 1 exemplaire à lister
                if (CourseList.Count > 1)
                {
                    // si "Tous mes cours" est la nouvelle sélection
                    if (value.Equals(CourseList.ElementAt(0)))
                    {
                        // la liste de tous les livres
                        BookList = db.GetCourseListBooksWithBookCopies(CourseList, CurrentUserId);

                        // tous les exemplaires
                        Exemplaires = db.GetCourseListBooksCopies(CourseList, CurrentUserId);
                    }
                    else
                    {
                        // la liste des livres d'un cours
                        BookList = db.GetCourseBooksWithBookCopies(SelectedCourse, CurrentUserId);

                        // exemplaires de tous les livres du cours sélectionné
                        Exemplaires = db.GetCourseBooksCopies(SelectedCourse, CurrentUserId);
                    }

                    SetPagination();
                }

                IsUpdatingCourse = false;
            }
        }

        // liste de livres
        private List<Book> _bookList;
        public List<Book> BookList
        {
            get { return _bookList; }
            set
            {
                value.Insert(0, new Book { Id = 0, Title = "Tous mes livres" });

                OnPropertyChanged(ref _bookList, value);

                SelectedBook = _bookList.ElementAt(0);
            }
        }

        // livre sélectionné
        private Book _selectedBook;
        public Book SelectedBook
        {
            get { return _selectedBook; }
            set
            {
                OnPropertyChanged(ref _selectedBook, value);

                // si on ne vient pas du set de SelectedCourse et qu'on
                // a au moins un examplaire au total
                if(!IsUpdatingCourse && CourseList.Count > 1)
                {
                    // si "Tous mes livres" est la nouvelle sélection
                    if (value.Equals(BookList.ElementAt(0)))
                    {
                        if (SelectedCourse.Equals(CourseList.ElementAt(0)))
                        {
                            // tous les exemplaires
                            Exemplaires = db.GetCourseListBooksCopies(CourseList, CurrentUserId);
                        }
                        else
                        {
                            // exemplaires de tous les livres du cours sélectionné
                            Exemplaires = db.GetCourseBooksCopies(SelectedCourse, CurrentUserId);
                        }
                    }
                    else
                    {
                        // les exemplaires du livre sélectionné, excluant ceux mis en vente par le user
                        Exemplaires = db.GetBookCopies(SelectedBook, CurrentUserId);
                    }

                    SetPagination();
                }
            }
        }

        private string _nbResult;
        public string NbResult
        {
            get { return _nbResult; }
            set { OnPropertyChanged(ref _nbResult, value); }
        }

        private bool _previousPageEnabled;
        public bool PreviousPageEnabled
        {
            get { return _previousPageEnabled; }
            set { OnPropertyChanged(ref _previousPageEnabled, value); }
        }

        private bool _nextPageEnabled;
        public bool NextPageEnabled
        {
            get { return _nextPageEnabled; }
            set { OnPropertyChanged(ref _nextPageEnabled, value); }
        }

        private MainMenuViewModel _mainMenuVM;
        public MainMenuViewModel MainMenuVM
        {
            get { return _mainMenuVM; }
            set { OnPropertyChanged(ref _mainMenuVM, value); }
        }

        public ICommand PreviousPageCommand { get; set; }
        public ICommand NextPageCommand { get; set; }

        private bool IsUpdatingCourse;

        private DetailsWindow detailsWindow;

        public BookListingViewModel(MainMenuViewModel MainMenuVM, int CurrentUserId)
        {
            this.CurrentUserId = CurrentUserId;
            this.MainMenuVM = MainMenuVM;

            db = new ApplicationDbContext();

            PreviousPageCommand = new RelayCommand(PreviousPage);
            NextPageCommand = new RelayCommand(NextPage);

            // liste de cours de l'étudiant avec au moins un exemplaire
            // non-publié par lui-même
            CourseList = db.GetUserCoursesWithBookCopies(CurrentUserId);

            // valeur par défault
            CourseList.Insert(0, new AcademicCourse { Id = 0, Acronym = "Tous mes cours", Name = String.Empty });
            SelectedCourse = CourseList.ElementAt(0);

            // si il n'y a aucun exemplaire disponible
            // pour le user, sa liste de cours est vide
            if (CourseList.Count == 1)
            {
                // init pour get la valeur par défaut
                BookList = new List<Book>();
                return;
            }

            IsUpdatingCourse = false;
        }

        // ouvre la page de détail quand on clique sur details d'un exemplaire 
        private void GoToDetailPage(BookCopy exemplaire)
        {
            if (Application.Current.Windows.OfType<DetailsWindow>().Any())
            {
                detailsWindow.Close();
            }

            detailsWindow = new DetailsWindow(exemplaire);
            detailsWindow.Show();
        }

        // affiche la liste de livres et gère les pages
        private void SetPagination()
        {
            CurrentIndex = 0;
            CurrentPage = 1;
            MaxPage = Exemplaires.Count % BOOKS_PER_PAGE == 0 ? Exemplaires.Count / BOOKS_PER_PAGE : (Exemplaires.Count / BOOKS_PER_PAGE) + 1;

            // rempli les 8 BookHolders
            setProperties();

            NextPageEnabled = false;
            PreviousPageEnabled = false;

            if (MaxPage > CurrentPage)
            {
                NextPageEnabled = true;
            }
        }

        private void PreviousPage()
        {
            CurrentPage--;

            CurrentIndex = (CurrentPage - 1) * BOOKS_PER_PAGE;

            setProperties();

            if(CurrentPage == 1)
            {
                PreviousPageEnabled = false;
            }

            CurrentIndex = (CurrentPage - 1) * BOOKS_PER_PAGE;

            NextPageEnabled = true;
        }

        private void NextPage()
        {
            CurrentPage++;

            CurrentIndex = (CurrentPage - 1) * BOOKS_PER_PAGE;

            setProperties();

            PreviousPageEnabled = true;

            if (CurrentPage == MaxPage)
            {
                NextPageEnabled = false;
            }

            CurrentIndex = (CurrentPage - 1) * BOOKS_PER_PAGE;
        }

        private ImageSource GetSource()
        {
            return Exemplaires[CurrentIndex].ImageCopyId != null ? Exemplaires[CurrentIndex].ImageCopy.BlobToImage() : null;
        }

        private String GetTitle()
        {
            return Exemplaires[CurrentIndex].Book.Title;
        }

        private String GetCondition()
        {
            return AppStrings.ConditionLivreA + Exemplaires[CurrentIndex].Condition.ToString() + AppStrings.ConditionLivreB;
        }

        private String GetPrice()
        {
            double price = Exemplaires[CurrentIndex].Price;
            string strPrice = price == 0 ? AppStrings.NonAppl : price.ToString("F") + "$";
            return AppStrings.PrixLivre + strPrice;
        }

        private String GetTransaction()
        {
            return Exemplaires[CurrentIndex].TransactionType;
        }

        private String GetHyperlink()
        {
            CurrentIndex++;
            return AppStrings.HyperLinkAcheter;
        }

        private bool OverLimit()
        {
            return CurrentIndex == Exemplaires.Count;
        }

        private string GetCopyId()
        {
            return (CurrentIndex + 1).ToString();
        }

        private void setProperties()
        {
            NbResult = Exemplaires.Count > 1 ?
                Exemplaires.Count.ToString() + " résultats de recherche, page " + CurrentPage + " de " + MaxPage :
                Exemplaires.Count.ToString() + " résultat de recherche, page " + CurrentPage + " de " + MaxPage;

            // on vide les 8 BookHolders
            BookImage1 = null; BookTitle1 = String.Empty; BookCondition1 = String.Empty; BookPrice1 = String.Empty; TransactionType1 = String.Empty; HyperLinkMessage1 = String.Empty; DetailPageCommand1 = null; Visibility1 = Visibility.Hidden; CopyId1 = string.Empty;
            BookImage2 = null; BookTitle2 = String.Empty; BookCondition2 = String.Empty; BookPrice2 = String.Empty; TransactionType2 = String.Empty; HyperLinkMessage2 = String.Empty; DetailPageCommand2 = null; Visibility2 = Visibility.Hidden; CopyId2 = string.Empty;
            BookImage3 = null; BookTitle3 = String.Empty; BookCondition3 = String.Empty; BookPrice3 = String.Empty; TransactionType3 = String.Empty; HyperLinkMessage3 = String.Empty; DetailPageCommand3 = null; Visibility3 = Visibility.Hidden; CopyId3 = string.Empty;
            BookImage4 = null; BookTitle4 = String.Empty; BookCondition4 = String.Empty; BookPrice4 = String.Empty; TransactionType4 = String.Empty; HyperLinkMessage4 = String.Empty; DetailPageCommand4 = null; Visibility4 = Visibility.Hidden; CopyId4 = string.Empty;
            BookImage5 = null; BookTitle5 = String.Empty; BookCondition5 = String.Empty; BookPrice5 = String.Empty; TransactionType5 = String.Empty; HyperLinkMessage5 = String.Empty; DetailPageCommand5 = null; Visibility5 = Visibility.Hidden; CopyId5 = string.Empty;
            BookImage6 = null; BookTitle6 = String.Empty; BookCondition6 = String.Empty; BookPrice6 = String.Empty; TransactionType6 = String.Empty; HyperLinkMessage6 = String.Empty; DetailPageCommand6 = null; Visibility6 = Visibility.Hidden; CopyId6 = string.Empty;
            BookImage7 = null; BookTitle7 = String.Empty; BookCondition7 = String.Empty; BookPrice7 = String.Empty; TransactionType7 = String.Empty; HyperLinkMessage7 = String.Empty; DetailPageCommand7 = null; Visibility7 = Visibility.Hidden; CopyId7 = string.Empty;
            BookImage8 = null; BookTitle8 = String.Empty; BookCondition8 = String.Empty; BookPrice8 = String.Empty; TransactionType8 = String.Empty; HyperLinkMessage8 = String.Empty; DetailPageCommand8 = null; Visibility8 = Visibility.Hidden; CopyId8 = string.Empty;
            BookImage9 = null; BookTitle9 = String.Empty; BookCondition9 = String.Empty; BookPrice9 = String.Empty; TransactionType9 = String.Empty; HyperLinkMessage9 = String.Empty; DetailPageCommand9 = null; Visibility9 = Visibility.Hidden; CopyId9 = string.Empty;
            BookImage10 = null; BookTitle10 = String.Empty; BookCondition10 = String.Empty; BookPrice10 = String.Empty; TransactionType10 = String.Empty; HyperLinkMessage10 = String.Empty; DetailPageCommand10 = null; Visibility10 = Visibility.Hidden; CopyId10 = string.Empty;

            if (OverLimit()) return;
            CopyId1 = GetCopyId();
            BookImage1 = GetSource(); BookTitle1 = GetTitle(); BookCondition1 = GetCondition();
            BookPrice1 = GetPrice(); TransactionType1 = GetTransaction(); HyperLinkMessage1 = GetHyperlink();
            DetailPageCommand1 = new RelayCommand(SetClickedExemplaire1); Visibility1 = Visibility.Visible;

            if (OverLimit()) return;
            CopyId2 = GetCopyId();
            BookImage2 = GetSource(); BookTitle2 = GetTitle(); BookCondition2 = GetCondition();
            BookPrice2 = GetPrice(); TransactionType2 = GetTransaction(); HyperLinkMessage2 = GetHyperlink();
            DetailPageCommand2 = new RelayCommand(SetClickedExemplaire2); Visibility2 = Visibility.Visible;

            if (OverLimit()) return;
            CopyId3 = GetCopyId();
            BookImage3 = GetSource(); BookTitle3 = GetTitle(); BookCondition3 = GetCondition();
            BookPrice3 = GetPrice(); TransactionType3 = GetTransaction(); HyperLinkMessage3 = GetHyperlink();
            DetailPageCommand3 = new RelayCommand(SetClickedExemplaire3); Visibility3 = Visibility.Visible;

            if (OverLimit()) return;
            CopyId4 = GetCopyId();
            BookImage4 = GetSource(); BookTitle4 = GetTitle(); BookCondition4 = GetCondition();
            BookPrice4 = GetPrice(); TransactionType4 = GetTransaction(); HyperLinkMessage4 = GetHyperlink();
            DetailPageCommand4 = new RelayCommand(SetClickedExemplaire4); Visibility4 = Visibility.Visible;

            if (OverLimit()) return;
            CopyId5 = GetCopyId();
            BookImage5 = GetSource(); BookTitle5 = GetTitle(); BookCondition5 = GetCondition();
            BookPrice5 = GetPrice(); TransactionType5 = GetTransaction(); HyperLinkMessage5 = GetHyperlink();
            DetailPageCommand5 = new RelayCommand(SetClickedExemplaire5); Visibility5 = Visibility.Visible;

            if (OverLimit()) return;
            CopyId6 = GetCopyId();
            BookImage6 = GetSource(); BookTitle6 = GetTitle(); BookCondition6 = GetCondition();
            BookPrice6 = GetPrice(); TransactionType6 = GetTransaction(); HyperLinkMessage6 = GetHyperlink();
            DetailPageCommand6 = new RelayCommand(SetClickedExemplaire6); Visibility6 = Visibility.Visible;

            if (OverLimit()) return;
            CopyId7 = GetCopyId();
            BookImage7 = GetSource(); BookTitle7 = GetTitle(); BookCondition7 = GetCondition();
            BookPrice7 = GetPrice(); TransactionType7 = GetTransaction(); HyperLinkMessage7 = GetHyperlink();
            DetailPageCommand7 = new RelayCommand(SetClickedExemplaire7); Visibility7 = Visibility.Visible;

            if (OverLimit()) return;
            CopyId8 = GetCopyId();
            BookImage8 = GetSource(); BookTitle8 = GetTitle(); BookCondition8 = GetCondition();
            BookPrice8 = GetPrice(); TransactionType8 = GetTransaction(); HyperLinkMessage8 = GetHyperlink();
            DetailPageCommand8 = new RelayCommand(SetClickedExemplaire8); Visibility8 = Visibility.Visible;

            if (OverLimit()) return;
            CopyId9 = GetCopyId();
            BookImage9 = GetSource(); BookTitle9 = GetTitle(); BookCondition9 = GetCondition();
            BookPrice9 = GetPrice(); TransactionType9 = GetTransaction(); HyperLinkMessage9 = GetHyperlink();
            DetailPageCommand9 = new RelayCommand(SetClickedExemplaire9); Visibility9 = Visibility.Visible;

            if (OverLimit()) return;
            CopyId10 = GetCopyId();
            BookImage10 = GetSource(); BookTitle10 = GetTitle(); BookCondition10 = GetCondition();
            BookPrice10 = GetPrice(); TransactionType10 = GetTransaction(); HyperLinkMessage10 = GetHyperlink();
            DetailPageCommand10 = new RelayCommand(SetClickedExemplaire10); Visibility10 = Visibility.Visible;
        }

        private void SetClickedExemplaire1() { GoToDetailPage(Exemplaires[0 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }
        private void SetClickedExemplaire2() { GoToDetailPage(Exemplaires[1 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }
        private void SetClickedExemplaire3() { GoToDetailPage(Exemplaires[2 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }
        private void SetClickedExemplaire4() { GoToDetailPage(Exemplaires[3 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }
        private void SetClickedExemplaire5() { GoToDetailPage(Exemplaires[4 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }
        private void SetClickedExemplaire6() { GoToDetailPage(Exemplaires[5 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }
        private void SetClickedExemplaire7() { GoToDetailPage(Exemplaires[6 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }
        private void SetClickedExemplaire8() { GoToDetailPage(Exemplaires[7 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }
        private void SetClickedExemplaire9() { GoToDetailPage(Exemplaires[8 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }
        private void SetClickedExemplaire10() { GoToDetailPage(Exemplaires[9 + (CurrentPage - 1) * BOOKS_PER_PAGE]); }

        #region BookHolder Properties

        private ImageSource _bookImage1;
        public ImageSource BookImage1
        {
            get { return _bookImage1; }
            set { OnPropertyChanged(ref _bookImage1, value); }
        }
        private ImageSource _bookImage2;
        public ImageSource BookImage2
        {
            get { return _bookImage2; }
            set { OnPropertyChanged(ref _bookImage2, value); }
        }
        private ImageSource _bookImage3;
        public ImageSource BookImage3
        {
            get { return _bookImage3; }
            set { OnPropertyChanged(ref _bookImage3, value); }
        }
        private ImageSource _bookImage4;
        public ImageSource BookImage4
        {
            get { return _bookImage4; }
            set { OnPropertyChanged(ref _bookImage4, value); }
        }
        private ImageSource _bookImage5;
        public ImageSource BookImage5
        {
            get { return _bookImage5; }
            set { OnPropertyChanged(ref _bookImage5, value); }
        }
        private ImageSource _bookImage6;
        public ImageSource BookImage6
        {
            get { return _bookImage6; }
            set { OnPropertyChanged(ref _bookImage6, value); }
        }
        private ImageSource _bookImage7;
        public ImageSource BookImage7
        {
            get { return _bookImage7; }
            set { OnPropertyChanged(ref _bookImage7, value); }
        }
        private ImageSource _bookImage8;
        public ImageSource BookImage8
        {
            get { return _bookImage8; }
            set { OnPropertyChanged(ref _bookImage8, value); }
        }
        private ImageSource _bookImage9;
        public ImageSource BookImage9
        {
            get { return _bookImage9; }
            set { OnPropertyChanged(ref _bookImage9, value); }
        }
        private ImageSource _bookImage10;
        public ImageSource BookImage10
        {
            get { return _bookImage10; }
            set { OnPropertyChanged(ref _bookImage10, value); }
        }
        private String _bookTitle1;
        public String BookTitle1
        {
            get { return _bookTitle1; }
            set { OnPropertyChanged(ref _bookTitle1, value); }
        }
        private String _bookTitle2;
        public String BookTitle2
        {
            get { return _bookTitle2; }
            set { OnPropertyChanged(ref _bookTitle2, value); }
        }
        private String _bookTitle3;
        public String BookTitle3
        {
            get { return _bookTitle3; }
            set { OnPropertyChanged(ref _bookTitle3, value); }
        }
        private String _bookTitle4;
        public String BookTitle4
        {
            get { return _bookTitle4; }
            set { OnPropertyChanged(ref _bookTitle4, value); }
        }
        private String _bookTitle5;
        public String BookTitle5
        {
            get { return _bookTitle5; }
            set { OnPropertyChanged(ref _bookTitle5, value); }
        }
        private String _bookTitle6;
        public String BookTitle6
        {
            get { return _bookTitle6; }
            set { OnPropertyChanged(ref _bookTitle6, value); }
        }
        private String _bookTitle7;
        public String BookTitle7
        {
            get { return _bookTitle7; }
            set { OnPropertyChanged(ref _bookTitle7, value); }
        }
        private String _bookTitle8;
        public String BookTitle8
        {
            get { return _bookTitle8; }
            set { OnPropertyChanged(ref _bookTitle8, value); }
        }
        private String _bookTitle9;
        public String BookTitle9
        {
            get { return _bookTitle9; }
            set { OnPropertyChanged(ref _bookTitle9, value); }
        }
        private String _bookTitle10;
        public String BookTitle10
        {
            get { return _bookTitle10; }
            set { OnPropertyChanged(ref _bookTitle10, value); }
        }
        private String _bookCondition1;
        public String BookCondition1
        {
            get { return _bookCondition1; }
            set { OnPropertyChanged(ref _bookCondition1, value); }
        }
        private String _bookCondition2;
        public String BookCondition2
        {
            get { return _bookCondition2; }
            set { OnPropertyChanged(ref _bookCondition2, value); }
        }
        private String _bookCondition3;
        public String BookCondition3
        {
            get { return _bookCondition3; }
            set { OnPropertyChanged(ref _bookCondition3, value); }
        }
        private String _bookCondition4;
        public String BookCondition4
        {
            get { return _bookCondition4; }
            set { OnPropertyChanged(ref _bookCondition4, value); }
        }
        private String _bookCondition5;
        public String BookCondition5
        {
            get { return _bookCondition5; }
            set { OnPropertyChanged(ref _bookCondition5, value); }
        }
        private String _bookCondition6;
        public String BookCondition6
        {
            get { return _bookCondition6; }
            set { OnPropertyChanged(ref _bookCondition6, value); }
        }
        private String _bookCondition7;
        public String BookCondition7
        {
            get { return _bookCondition7; }
            set { OnPropertyChanged(ref _bookCondition7, value); }
        }
        private String _bookCondition8;
        public String BookCondition8
        {
            get { return _bookCondition8; }
            set { OnPropertyChanged(ref _bookCondition8, value); }
        }
        private String _bookCondition9;
        public String BookCondition9
        {
            get { return _bookCondition9; }
            set { OnPropertyChanged(ref _bookCondition9, value); }
        }
        private String _bookCondition10;
        public String BookCondition10
        {
            get { return _bookCondition10; }
            set { OnPropertyChanged(ref _bookCondition10, value); }
        }
        private String _bookPrice1;
        public String BookPrice1
        {
            get { return _bookPrice1; }
            set { OnPropertyChanged(ref _bookPrice1, value); }
        }
        private String _bookPrice2;
        public String BookPrice2
        {
            get { return _bookPrice2; }
            set { OnPropertyChanged(ref _bookPrice2, value); }
        }
        private String _bookPrice3;
        public String BookPrice3
        {
            get { return _bookPrice3; }
            set { OnPropertyChanged(ref _bookPrice3, value); }
        }
        private String _bookPrice4;
        public String BookPrice4
        {
            get { return _bookPrice4; }
            set { OnPropertyChanged(ref _bookPrice4, value); }
        }
        private String _bookPrice5;
        public String BookPrice5
        {
            get { return _bookPrice5; }
            set { OnPropertyChanged(ref _bookPrice5, value); }
        }
        private String _bookPrice6;
        public String BookPrice6
        {
            get { return _bookPrice6; }
            set { OnPropertyChanged(ref _bookPrice6, value); }
        }
        private String _bookPrice7;
        public String BookPrice7
        {
            get { return _bookPrice7; }
            set { OnPropertyChanged(ref _bookPrice7, value); }
        }
        private String _bookPrice8;
        public String BookPrice8
        {
            get { return _bookPrice8; }
            set { OnPropertyChanged(ref _bookPrice8, value); }
        }
        private String _bookPrice9;
        public String BookPrice9
        {
            get { return _bookPrice9; }
            set { OnPropertyChanged(ref _bookPrice9, value); }
        }
        private String _bookPrice10;
        public String BookPrice10
        {
            get { return _bookPrice10; }
            set { OnPropertyChanged(ref _bookPrice10, value); }
        }
        private String _transactionType1;
        public String TransactionType1
        {
            get { return _transactionType1; }
            set { OnPropertyChanged(ref _transactionType1, value); }
        }
        private String _transactionType2;
        public String TransactionType2
        {
            get { return _transactionType2; }
            set { OnPropertyChanged(ref _transactionType2, value); }
        }
        private String _transactionType3;
        public String TransactionType3
        {
            get { return _transactionType3; }
            set { OnPropertyChanged(ref _transactionType3, value); }
        }
        private String _transactionType4;
        public String TransactionType4
        {
            get { return _transactionType4; }
            set { OnPropertyChanged(ref _transactionType4, value); }
        }
        private String _transactionType5;
        public String TransactionType5
        {
            get { return _transactionType5; }
            set { OnPropertyChanged(ref _transactionType5, value); }
        }
        private String _transactionType6;
        public String TransactionType6
        {
            get { return _transactionType6; }
            set { OnPropertyChanged(ref _transactionType6, value); }
        }
        private String _transactionType7;
        public String TransactionType7
        {
            get { return _transactionType7; }
            set { OnPropertyChanged(ref _transactionType7, value); }
        }
        private String _transactionType8;
        public String TransactionType8
        {
            get { return _transactionType8; }
            set { OnPropertyChanged(ref _transactionType8, value); }
        }
        private String _transactionType9;
        public String TransactionType9
        {
            get { return _transactionType9; }
            set { OnPropertyChanged(ref _transactionType9, value); }
        }
        private String _transactionType10;
        public String TransactionType10
        {
            get { return _transactionType10; }
            set { OnPropertyChanged(ref _transactionType10, value); }
        }
        private String _hyperLinkMessage1;
        public String HyperLinkMessage1
        {
            get { return _hyperLinkMessage1; }
            set { OnPropertyChanged(ref _hyperLinkMessage1, value); }
        }
        private String _hyperLinkMessage2;
        public String HyperLinkMessage2
        {
            get { return _hyperLinkMessage2; }
            set { OnPropertyChanged(ref _hyperLinkMessage2, value); }
        }
        private String _hyperLinkMessage3;
        public String HyperLinkMessage3
        {
            get { return _hyperLinkMessage3; }
            set { OnPropertyChanged(ref _hyperLinkMessage3, value); }
        }
        private String _hyperLinkMessage4;
        public String HyperLinkMessage4
        {
            get { return _hyperLinkMessage4; }
            set { OnPropertyChanged(ref _hyperLinkMessage4, value); }
        }
        private String _hyperLinkMessage5;
        public String HyperLinkMessage5
        {
            get { return _hyperLinkMessage5; }
            set { OnPropertyChanged(ref _hyperLinkMessage5, value); }
        }
        private String _hyperLinkMessage6;
        public String HyperLinkMessage6
        {
            get { return _hyperLinkMessage6; }
            set { OnPropertyChanged(ref _hyperLinkMessage6, value); }
        }
        private String _hyperLinkMessage7;
        public String HyperLinkMessage7
        {
            get { return _hyperLinkMessage7; }
            set { OnPropertyChanged(ref _hyperLinkMessage7, value); }
        }
        private String _hyperLinkMessage8;
        public String HyperLinkMessage8
        {
            get { return _hyperLinkMessage8; }
            set { OnPropertyChanged(ref _hyperLinkMessage8, value); }
        }
        private String _hyperLinkMessage9;
        public String HyperLinkMessage9
        {
            get { return _hyperLinkMessage9; }
            set { OnPropertyChanged(ref _hyperLinkMessage9, value); }
        }
        private String _hyperLinkMessage10;
        public String HyperLinkMessage10
        {
            get { return _hyperLinkMessage10; }
            set { OnPropertyChanged(ref _hyperLinkMessage10, value); }
        }
        private ICommand _detailPageCommand1;
        public ICommand DetailPageCommand1
        {
            get { return _detailPageCommand1; }
            set { OnPropertyChanged(ref _detailPageCommand1, value); }
        }
        private ICommand _detailPageCommand2;
        public ICommand DetailPageCommand2
        {
            get { return _detailPageCommand2; }
            set { OnPropertyChanged(ref _detailPageCommand2, value); }
        }
        private ICommand _detailPageCommand3;
        public ICommand DetailPageCommand3
        {
            get { return _detailPageCommand3; }
            set { OnPropertyChanged(ref _detailPageCommand3, value); }
        }
        private ICommand _detailPageCommand4;
        public ICommand DetailPageCommand4
        {
            get { return _detailPageCommand4; }
            set { OnPropertyChanged(ref _detailPageCommand4, value); }
        }
        private ICommand _detailPageCommand5;
        public ICommand DetailPageCommand5
        {
            get { return _detailPageCommand5; }
            set { OnPropertyChanged(ref _detailPageCommand5, value); }
        }
        private ICommand _detailPageCommand6;
        public ICommand DetailPageCommand6
        {
            get { return _detailPageCommand6; }
            set { OnPropertyChanged(ref _detailPageCommand6, value); }
        }
        private ICommand _detailPageCommand7;
        public ICommand DetailPageCommand7
        {
            get { return _detailPageCommand7; }
            set { OnPropertyChanged(ref _detailPageCommand7, value); }
        }
        private ICommand _detailPageCommand8;
        public ICommand DetailPageCommand8
        {
            get { return _detailPageCommand8; }
            set { OnPropertyChanged(ref _detailPageCommand8, value); }
        }
        private ICommand _detailPageCommand9;
        public ICommand DetailPageCommand9
        {
            get { return _detailPageCommand9; }
            set { OnPropertyChanged(ref _detailPageCommand9, value); }
        }
        private ICommand _detailPageCommand10;
        public ICommand DetailPageCommand10
        {
            get { return _detailPageCommand10; }
            set { OnPropertyChanged(ref _detailPageCommand10, value); }
        }
        private Visibility _visibility1;
        public Visibility Visibility1
        {
            get { return _visibility1; }
            set { OnPropertyChanged(ref _visibility1, value); }
        }
        private Visibility _visibility2;
        public Visibility Visibility2
        {
            get { return _visibility2; }
            set { OnPropertyChanged(ref _visibility2, value); }
        }
        private Visibility _visibility3;
        public Visibility Visibility3
        {
            get { return _visibility3; }
            set { OnPropertyChanged(ref _visibility3, value); }
        }
        private Visibility _visibility4;
        public Visibility Visibility4
        {
            get { return _visibility4; }
            set { OnPropertyChanged(ref _visibility4, value); }
        }
        private Visibility _visibility5;
        public Visibility Visibility5
        {
            get { return _visibility5; }
            set { OnPropertyChanged(ref _visibility5, value); }
        }
        private Visibility _visibility6;
        public Visibility Visibility6
        {
            get { return _visibility6; }
            set { OnPropertyChanged(ref _visibility6, value); }
        }
        private Visibility _visibility7;
        public Visibility Visibility7
        {
            get { return _visibility7; }
            set { OnPropertyChanged(ref _visibility7, value); }
        }
        private Visibility _visibility8;
        public Visibility Visibility8
        {
            get { return _visibility8; }
            set { OnPropertyChanged(ref _visibility8, value); }
        }
        private Visibility _visibility9;
        public Visibility Visibility9
        {
            get { return _visibility9; }
            set { OnPropertyChanged(ref _visibility9, value); }
        }
        private Visibility _visibility10;
        public Visibility Visibility10
        {
            get { return _visibility10; }
            set { OnPropertyChanged(ref _visibility10, value); }
        }
        private string _copyId1;
        public string CopyId1
        {
            get { return _copyId1; }
            set { OnPropertyChanged(ref _copyId1, value); }
        }
        private string _copyId2;
        public string CopyId2
        {
            get { return _copyId2; }
            set { OnPropertyChanged(ref _copyId2, value); }
        }
        private string _copyId3;
        public string CopyId3
        {
            get { return _copyId3; }
            set { OnPropertyChanged(ref _copyId3, value); }
        }
        private string _copyId4;
        public string CopyId4
        {
            get { return _copyId4; }
            set { OnPropertyChanged(ref _copyId4, value); }
        }
        private string _copyId5;
        public string CopyId5
        {
            get { return _copyId5; }
            set { OnPropertyChanged(ref _copyId5, value); }
        }
        private string _copyId6;
        public string CopyId6
        {
            get { return _copyId6; }
            set { OnPropertyChanged(ref _copyId6, value); }
        }
        private string _copyId7;
        public string CopyId7
        {
            get { return _copyId7; }
            set { OnPropertyChanged(ref _copyId7, value); }
        }
        private string _copyId8;
        public string CopyId8
        {
            get { return _copyId8; }
            set { OnPropertyChanged(ref _copyId8, value); }
        }
        private string _copyId9;
        public string CopyId9
        {
            get { return _copyId9; }
            set { OnPropertyChanged(ref _copyId9, value); }
        }
        private string _copyId10;
        public string CopyId10
        {
            get { return _copyId10; }
            set { OnPropertyChanged(ref _copyId10, value); }
        }

#endregion
    }
}
