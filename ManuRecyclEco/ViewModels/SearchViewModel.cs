using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ManuRecyEco.ViewModels
{
    public class SearchViewModel : ObservableObject
    {
        // liste de programmes
        private List<AcademicProgram> _programList;
        public List<AcademicProgram> ProgramList
        {
            get { return _programList; }
            set { OnPropertyChanged(ref _programList, value); }
        }

        // programme sélectionné
        private AcademicProgram _selectedProgram;
        public AcademicProgram SelectedProgram
        {
            get { return _selectedProgram; }
            set
            {
                OnPropertyChanged(ref _selectedProgram, value);

                IsUpdatingSelectedProgram = true;

                // si le nouveau choix est le choix par défault
                if (_selectedProgram.Equals(ProgramList.ElementAt(0)))
                {
                    CourseList = db.GetCourses(ProgramList);

                    if (!InGetExemplaires)
                    {
                        Exemplaires = db.GetProgramListExemplaires(ProgramList);
                        FilterExemplaires();
                    }
                }
                else
                {
                    CourseList = db.GetCourses(SelectedProgram);

                    if (!InGetExemplaires)
                    {
                        Exemplaires = db.GetProgramExemplaires(SelectedProgram);
                        FilterExemplaires();
                    }
                }

                SelectedCourse = CourseList.ElementAt(0);

                IsUpdatingSelectedProgram = false;
            }
        }

        // liste de cours
        private List<AcademicCourse> _courseList;
        public List<AcademicCourse> CourseList
        {
            get { return _courseList; }
            set
            {
                value.Insert(0, new AcademicCourse { Id = 0, Acronym = "Tous les cours", Name = string.Empty });

                OnPropertyChanged(ref _courseList, value);
            }
        }

        // cours sélectionné
        private AcademicCourse _selectedCourse;
        public AcademicCourse SelectedCourse
        {
            get { return _selectedCourse; }
            set
            {
                OnPropertyChanged(ref _selectedCourse, value);

                if (_selectedCourse != null)
                {
                    IsUpdatingSelectedCourse = true;

                    // si le nouveau choix est le choix par défault
                    if (_selectedCourse.Equals(CourseList.ElementAt(0)))
                    {
                        BookList = db.GetBooks(CourseList);

                        if(!IsUpdatingSelectedProgram && !InGetExemplaires)
                        {
                            Exemplaires = db.GetCourseListExemplaires(CourseList);
                            FilterExemplaires();
                        }
                    }
                    else
                    {
                        BookList = db.GetBooks(SelectedCourse);

                        if (!IsUpdatingSelectedProgram && !InGetExemplaires)
                        {
                            Exemplaires = db.GetCourseExemplaires(SelectedCourse);
                            FilterExemplaires();
                        }
                    }

                    SelectedBook = BookList.ElementAt(0);

                    IsUpdatingSelectedCourse = false;
                }
            }
        }

        // liste de livres
        private List<Book> _bookList;
        public List<Book> BookList
        {
            get { return _bookList; }
            set
            {
                value.Insert(0, new Book { Id = 0, Title = "Tous les livres" });

                OnPropertyChanged(ref _bookList, value);
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

                if(_selectedBook != null)
                {
                    if (!IsUpdatingSelectedProgram && !IsUpdatingSelectedCourse && !InGetExemplaires)
                    {
                        if (_selectedBook.Equals(BookList.ElementAt(0)))
                        {
                            Exemplaires = db.GetBookListExemplaires(BookList);
                        }
                        else
                        {
                            Exemplaires = db.GetBookExemplaires(SelectedBook);
                        }

                        FilterExemplaires();
                    }
                }
            }
        }

        // texte recherché
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                value = value.Trim();

                OnPropertyChanged(ref _searchText, value);

                if (_searchText.Length > 2 || _searchText == string.Empty)
                {
                    GetExemplaires();
                }
            }
        }

        // liste d'états
        private List<string> _conditionList;
        public List<string> ConditionList
        {
            get { return _conditionList; }
            set { OnPropertyChanged(ref _conditionList, value); }
        }

        // état sélectionné
        private string _selectedCondition;
        public string SelectedCondition
        {
            get { return _selectedCondition; }
            set
            {
                OnPropertyChanged(ref _selectedCondition, value);

                if(SelectedConditionOperator != null && !SelectedConditionOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    GetExemplaires();
                }
            }
        }

        // liste d'états
        private List<string> _publisherList;
        public List<string> PublisherList
        {
            get { return _publisherList; }
            set { OnPropertyChanged(ref _publisherList, value); }
        }

        // état sélectionné
        private string _selectedPublisher;
        public string SelectedPublisher
        {
            get { return _selectedPublisher; }
            set
            {
                OnPropertyChanged(ref _selectedPublisher, value);

                if (_selectedPublisher != null)
                {
                    GetExemplaires();
                }
            }
        }

        // types de transactions
        private List<string> _transactionList;
        public List<string> TransactionList
        {
            get { return _transactionList; }
            set { OnPropertyChanged(ref _transactionList, value); }
        }

        // transaction sélectionné
        private string _selectedTransaction;
        public string SelectedTransaction
        {
            get { return _selectedTransaction; }
            set
            {
                OnPropertyChanged(ref _selectedTransaction, value);

                GetExemplaires();
            }
        }

        // utilisateurs
        private List<User> _userList;
        public List<User> UserList
        {
            get { return _userList; }
            set { OnPropertyChanged(ref _userList, value); }
        }

        // transaction sélectionné
        private User _selectedUser;
        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                OnPropertyChanged(ref _selectedUser, value);

                GetExemplaires();
            }
        }

        // opérateurs
        private List<string> _operatorsList;
        public List<string> OperatorsList
        {
            get { return _operatorsList; }
            set { OnPropertyChanged(ref _operatorsList, value); }
        }

        // opérateur année sélectionné
        private string _selectedYearOperator;
        public string SelectedYearOperator
        {
            get { return _selectedYearOperator; }
            set
            {
                OnPropertyChanged(ref _selectedYearOperator, value);
                
                if(_selectedYearOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    StrYear = string.Empty;
                    GetExemplaires();
                    return;
                }

                int number = 0;
                if(ValidateTextBox(StrYear, ref number, 0, 2022))
                {
                    GetExemplaires();
                }
            }
        }

        // opérateur état sélectionné
        private string _selectedConditionOperator;
        public string SelectedConditionOperator
        {
            get { return _selectedConditionOperator; }
            set
            {
                OnPropertyChanged(ref _selectedConditionOperator, value);

                if (_selectedConditionOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    SelectedCondition = ConditionList.ElementAt(0);
                    GetExemplaires();
                    return;
                }

                if (!SelectedCondition.Equals(ConditionList.ElementAt(0)))
                {
                    GetExemplaires();
                }
            }
        }

        // opérateur prix sélectionné
        private string _selectedPriceOperator;
        public string SelectedPriceOperator
        {
            get { return _selectedPriceOperator; }
            set
            {
                OnPropertyChanged(ref _selectedPriceOperator, value);

                if (_selectedPriceOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    StrPrice = string.Empty;
                    GetExemplaires();
                    return;
                }

                int number = 0;
                if (ValidateTextBox(StrPrice, ref number, 0, 999))
                {
                    GetExemplaires();
                }
            }
        }

        // opérateur prix référence sélectionné
        private string _selectedRefPriceOperator;
        public string SelectedRefPriceOperator
        {
            get { return _selectedRefPriceOperator; }
            set
            {
                OnPropertyChanged(ref _selectedRefPriceOperator, value);

                if (_selectedRefPriceOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    StrRefPrice = string.Empty;
                    GetExemplaires();
                    return;
                }

                int number = 0;
                if (ValidateTextBox(StrRefPrice, ref number, 0, 999))
                {
                    GetExemplaires();
                }
            }
        }

        // opérateur nombre pages sélectionné
        private string _selectedNbPagesOperator;
        public string SelectedNbPagesOperator
        {
            get { return _selectedNbPagesOperator; }
            set
            {
                OnPropertyChanged(ref _selectedNbPagesOperator, value);

                if (_selectedNbPagesOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    StrNbPages = string.Empty;
                    GetExemplaires();
                    return;
                }

                int number = 0;
                if (ValidateTextBox(StrNbPages, ref number, 0, 9999))
                {
                    GetExemplaires();
                }
            }
        }

        // prix entré
        private string _strPrice;
        public string StrPrice
        {
            get { return _strPrice; }
            set
            {
                value = value.Trim();

                OnPropertyChanged(ref _strPrice, value);

                if (!SelectedPriceOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    int number = 0;
                    if ((_strPrice.Length > 1 && ValidateTextBox(_strPrice, ref number, 0, 999)) ||
                        _strPrice == string.Empty)
                    {
                        GetExemplaires();
                    }
                }
            }
        }

        // prix de référence entré
        private string _strRefPrice;
        public string StrRefPrice
        {
            get { return _strRefPrice; }
            set
            {
                value = value.Trim();

                OnPropertyChanged(ref _strRefPrice, value);

                if (!SelectedRefPriceOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    int number = 0;
                    if ((_strRefPrice.Length > 1 && ValidateTextBox(_strRefPrice, ref number, 0, 999)) ||
                        _strRefPrice == string.Empty)
                    {
                        GetExemplaires();
                    }
                }
            }
        }

        // nombre de pages entré
        private string _strNbPages;
        public string StrNbPages
        {
            get { return _strNbPages; }
            set
            {
                value = value.Trim();

                OnPropertyChanged(ref _strNbPages, value);

                if (!SelectedNbPagesOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    int number = 0;
                    if ((_strNbPages.Length > 1 && ValidateTextBox(_strNbPages, ref number, 0, 9999)) ||
                        _strNbPages == string.Empty)
                    {
                        GetExemplaires();
                    }
                }
            }
        }

        // année entrée
        private string _strYear;
        public string StrYear
        {
            get { return _strYear; }
            set
            {
                value = value.Trim();

                OnPropertyChanged(ref _strYear, value);

                if (!SelectedYearOperator.Equals(OperatorsList.ElementAt(0)))
                {
                    int number = 0;
                    if ((_strYear.Length == 4 && ValidateTextBox(_strYear, ref number, 0, 2022)) ||
                        _strYear == string.Empty)
                    {
                        GetExemplaires();
                    }
                }
            }
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

                    if (IsVisitor)
                    {
                        VisitorButtonVisibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    StopTimer();

                    if (IsVisitor)
                    {
                        VisitorButtonVisibility = Visibility.Visible;
                    }
                }

                OnPropertyChanged(ref _messageVisibility, value);
            }
        }

        // pour le bouton visiteur
        private Visibility _visitorButtonVisibility;
        public Visibility VisitorButtonVisibility
        {
            get { return _visitorButtonVisibility; }
            set {  OnPropertyChanged(ref _visitorButtonVisibility, value); }
        }

        // pour le message d'aucun résultat de recherche
        private Visibility _noResultMessageVisibility;
        public Visibility NoResultMessageVisibility
        {
            get { return _noResultMessageVisibility; }
            set { OnPropertyChanged(ref _noResultMessageVisibility, value); }
        }

        private string _nbResult;
        public string NbResult
        {
            get { return _nbResult; }
            set { OnPropertyChanged(ref _nbResult, value); }
        }

        // contenu du message
        private string _messageContent;
        public string MessageContent
        {
            get { return _messageContent; }
            set { OnPropertyChanged(ref _messageContent, value); }
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

        private MainWindowViewModel _mainWindowVM;
        public MainWindowViewModel MainWindowVM
        {
            get { return _mainWindowVM; }
            set { OnPropertyChanged(ref _mainWindowVM, value); }
        }

        public ICommand PreviousPageCommand { get; set; }
        public ICommand NextPageCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ResetFieldsCommand { get; set; }
        public ICommand GoToLoginCommand { get; set; }

        private const int BOOKS_PER_PAGE = 10;

        private ApplicationDbContext db;

        private int CurrentUserId;

        private int CurrentIndex;

        private int CurrentPage;

        private int MaxPage;

        private List<BookCopy> Exemplaires;

        private bool IsUpdatingSelectedProgram;

        private bool IsUpdatingSelectedCourse;
        private bool InGetExemplaires;
        private bool IsFiltering;

        private DispatcherTimer timer;

        private TimeSpan time;

        private bool IsVisitor;

        private DetailsWindow detailsWindow;

        // constructeur visiteur
        public SearchViewModel(MainWindowViewModel MainWindowVM)
        {
            IsVisitor = true;

            this.MainWindowVM = MainWindowVM;

            db = new ApplicationDbContext();

            VisitorButtonVisibility = Visibility.Visible;

            // liste utilisateurs
            UserList = new List<User>() { new User { 
                Id = 0, 
                Username = "Non-disponible", 
                FirstName = string.Empty, 
                LastName = string.Empty } };

            InitViewModel();
        }

        // constructeur utilisateur
        public SearchViewModel(int CurrentUserId)
        {
            IsVisitor = false;

            this.CurrentUserId = CurrentUserId;

            db = new ApplicationDbContext();

            VisitorButtonVisibility = Visibility.Collapsed;

            // liste utilisateurs
            UserList = db.Users.OrderBy(u => u.Id).ToList();

            UserList.Insert(0, new User { 
                Id = 0, Username = "Tous les utilisateurs",
                FirstName = string.Empty,
                LastName = string.Empty
            });

            InitViewModel();
        }

        private void InitViewModel()
        {
            IsUpdatingSelectedProgram = false;
            IsUpdatingSelectedCourse = false;
            InGetExemplaires = false;
            IsFiltering = false;

            // liste des programmes à l'UQAM
            ProgramList = db.GetUqamProgramListExceptDefault();

            ProgramList.Insert(0, new AcademicProgram { Id = 0, Name = "Tous les programmes" });
            SelectedProgram = ProgramList.ElementAt(0);

            // choix de transactions
            TransactionList = new List<string>() { "Toutes les transactions",
                                                   AppStrings.TransactionVente,
                                                   AppStrings.TransactionEchange };

            // choix pour l'état du livre
            ConditionList = new List<string>() { "N/A", "1", "2", "3", "4", "5",
                                                 "6", "7", "8", "9", "10" };

            // choix d'opérateurs
            OperatorsList = new List<string>() { AppStrings.NoOperator,
                                                 AppStrings.SmallerThan,
                                                 AppStrings.GreaterThan,
                                                 AppStrings.EqualTo };

            // liste éditeurs
            PublisherList = db.Books.Select(b => b.Publisher.Trim()).
                Distinct().OrderBy(x => x).ToList();

            PublisherList.Insert(0, "Tous les éditeurs");

            InitFields();

            // message caché
            MessageVisibility = Visibility.Hidden;
            MessageContent = String.Empty;

            PreviousPageCommand = new RelayCommand(PreviousPage);
            NextPageCommand = new RelayCommand(NextPage);
            SearchCommand = new RelayCommand(Search);
            ResetFieldsCommand = new RelayCommand(ResetFields);
            GoToLoginCommand = new RelayCommand(GoToLogin);
        }

        private void InitFields()
        {
            // initialisation des éléments sélectionnés avec les valeurs par défault
            SelectedUser = UserList.ElementAt(0);
            SelectedPublisher = PublisherList.ElementAt(0);
            SelectedCondition = ConditionList.ElementAt(0);
            SelectedTransaction = TransactionList.ElementAt(0);
            SelectedConditionOperator = OperatorsList.ElementAt(0);
            SelectedYearOperator = OperatorsList.ElementAt(0);
            SelectedPriceOperator = OperatorsList.ElementAt(0);
            SelectedRefPriceOperator = OperatorsList.ElementAt(0);
            SelectedNbPagesOperator = OperatorsList.ElementAt(0);
            StrPrice = string.Empty;
            StrRefPrice = string.Empty;
            StrYear = string.Empty;
            StrNbPages = string.Empty;
            SearchText = string.Empty;
        }

        private void ResetFields()
        {
            InitFields();

            SelectedProgram = ProgramList.ElementAt(0);

            MessageVisibility = Visibility.Visible;
            MessageContent = "Tous les champs ont été réinitialisés";
        }

        private void GoToLogin()
        {
            if(IsVisitor)
            {
                MainWindowVM.CurrentView = new LoginViewModel(MainWindowVM);
            }
        }

        private bool ValidateTextBox(string field, ref int number, int min, int max)
        {
            if (field.Trim() != string.Empty && int.TryParse(field.Trim(), out number) && number >= min && number <= max)
            {
                return true;
            }
            return false;
        }

        private void Search()
        {
            GetExemplaires();
        }

        private void FilterExemplaires()
        {
            IsFiltering = true;

            List<Book> books = Exemplaires.
                        Join(db.Books,
                            e => e.BookId,
                            b => b.Id,
                            (e, b) => b).Distinct().ToList();

            // champ texte
            if (SearchText != null && SearchText.Trim().Length >= 3)
            {
                string search = SearchText.ToLower();

                books = books.Where(b => b.Title.ToLower().Contains(search) ||
                            b.Author.ToLower().Contains(search)).ToList();
            }
            else if (SearchText != null && SearchText.Trim().Length < 3 && SearchText.Trim() != string.Empty)
            {
                if (MessageVisibility == Visibility.Hidden)
                {
                    MessageVisibility = Visibility.Visible;
                    MessageContent = "Le texte entrée a moins de 3 caractères, la recherche texte est donc ignorée";
                }
            }

            // éditeur
            if (SelectedPublisher != null && !SelectedPublisher.Equals(PublisherList.ElementAt(0)))
            {
                books = books.Where(b => b.Publisher == SelectedPublisher).ToList();
            }

            // année
            if (SelectedYearOperator != null && SelectedYearOperator != AppStrings.NoOperator)
            {
                int number = 0;
                if (ValidateTextBox(StrYear, ref number, 0, 2022))
                {
                    if (SelectedYearOperator == AppStrings.GreaterThan)
                    {
                        books = books.Where(b => b.Year > number).ToList();
                    }
                    else if (SelectedYearOperator == AppStrings.SmallerThan)
                    {
                        books = books.Where(b => b.Year < number).ToList();
                    }
                    else if (SelectedYearOperator == AppStrings.EqualTo)
                    {
                        books = books.Where(b => b.Year == number).ToList();
                    }
                }
                else if(StrYear != string.Empty)
                {
                    if (MessageVisibility == Visibility.Hidden)
                    {
                        MessageVisibility = Visibility.Visible;
                        MessageContent = "L'année entrée doit être un entier entre 0 et 2022";
                    }
                }
            }

            // prix de référence
            if (SelectedRefPriceOperator != null && SelectedRefPriceOperator != AppStrings.NoOperator)
            {
                int number = 0;
                if (ValidateTextBox(StrRefPrice, ref number, 0, 999))
                {
                    if (SelectedRefPriceOperator == AppStrings.GreaterThan)
                    {
                        books = books.Where(b => b.ReferencePrice > number).ToList();
                    }
                    else if (SelectedRefPriceOperator == AppStrings.SmallerThan)
                    {
                        books = books.Where(b => b.ReferencePrice < number).ToList();
                    }
                    else if (SelectedRefPriceOperator == AppStrings.EqualTo)
                    {
                        books = books.Where(b => (int)b.ReferencePrice == number).ToList();
                    }
                }
                else if (StrRefPrice != string.Empty)
                {
                    if (MessageVisibility == Visibility.Hidden)
                    {
                        MessageVisibility = Visibility.Visible;
                        MessageContent = "Le prix de référence doit être une entier entre 0 et 999";
                    }
                }
            }

            // nombre de pages
            if (SelectedNbPagesOperator != null && SelectedNbPagesOperator != AppStrings.NoOperator)
            {
                int number = 0;
                if (ValidateTextBox(StrNbPages, ref number, 0, 9999))
                {
                    if (SelectedNbPagesOperator == AppStrings.GreaterThan)
                    {
                        books = books.Where(b => b.NumPages > number).ToList();
                    }
                    else if (SelectedNbPagesOperator == AppStrings.SmallerThan)
                    {
                        books = books.Where(b => b.NumPages < number).ToList();
                    }
                    else if (SelectedNbPagesOperator == AppStrings.EqualTo)
                    {
                        books = books.Where(b => b.NumPages == number).ToList();
                    }
                }
                else if (StrNbPages != string.Empty)
                {
                    if (MessageVisibility == Visibility.Hidden)
                    {
                        MessageVisibility = Visibility.Visible;
                        MessageContent = "Le nombre de pages doit être une entier entre 0 et 9999";
                    }
                }

            }

            Exemplaires = books.Join(db.BookCopies,
                        b => b.Id,
                        bc => bc.BookId,
                        (b, bc) => bc).Distinct().ToList();

            // vente ou échange
            if (TransactionList != null && SelectedTransaction != null && 
                !SelectedTransaction.Equals(TransactionList.ElementAt(0)))
            {
                if (SelectedTransaction == AppStrings.TransactionVente)
                {
                    Exemplaires = Exemplaires.
                        Where(e => e.TransactionType == AppStrings.TransactionVente ||
                            e.TransactionType == AppStrings.TransactionVenteEchange).ToList();
                }
                else if (SelectedTransaction == AppStrings.TransactionEchange)
                {
                    Exemplaires = Exemplaires.
                        Where(e => e.TransactionType == AppStrings.TransactionEchange ||
                            e.TransactionType == AppStrings.TransactionVenteEchange).ToList();
                }
            }

            // état du livre
            if (ConditionList != null && SelectedCondition != null && 
                !SelectedCondition.Equals(ConditionList.ElementAt(0)) &&
                SelectedConditionOperator != AppStrings.NoOperator)
            {
                int condition = int.Parse(SelectedCondition);

                if (SelectedConditionOperator == AppStrings.GreaterThan)
                {
                    Exemplaires = Exemplaires.
                        Where(e => e.Condition > condition).ToList();
                }
                else if (SelectedConditionOperator == AppStrings.SmallerThan)
                {
                    Exemplaires = Exemplaires.
                        Where(e => e.Condition < condition).ToList();
                }
                else if (SelectedConditionOperator == AppStrings.EqualTo)
                {
                    Exemplaires = Exemplaires.
                        Where(e => e.Condition == condition).ToList();
                }
            }

            // prix de vente
            if (SelectedPriceOperator!= null && SelectedPriceOperator != AppStrings.NoOperator)
            {
                int number = 0;
                if (ValidateTextBox(StrPrice, ref number, 0, 999))
                {
                    if (SelectedPriceOperator == AppStrings.GreaterThan)
                    {
                        Exemplaires = Exemplaires.
                            Where(e => e.Price > number).ToList();
                    }
                    else if (SelectedPriceOperator == AppStrings.SmallerThan)
                    {
                        Exemplaires = Exemplaires.
                            Where(e => e.Price < number).ToList();
                    }
                    else if (SelectedPriceOperator == AppStrings.EqualTo)
                    {
                        Exemplaires = Exemplaires.
                            Where(e => (int)e.Price == number).ToList();
                    }
                }
                else if (StrPrice != string.Empty)
                {
                    if (MessageVisibility == Visibility.Hidden)
                    {
                        MessageVisibility = Visibility.Visible;
                        MessageContent = "Le prix de vente doit être une entier entre 0 et 999";
                    }
                }
            }

            // username
            if(SelectedUser != null && UserList != null && !SelectedUser.Equals(UserList.ElementAt(0)))
            {
                Exemplaires = Exemplaires.
                            Where(e => e.UserId == SelectedUser.Id).ToList();
            }

            if(Exemplaires.Count == 0)
            {
                NoResultMessageVisibility = Visibility.Visible;
            }
            else
            {
                NoResultMessageVisibility = Visibility.Collapsed;
            }

            OrderExemplaires();

            IsFiltering = false;

            SetPagination();
        }

        private void OrderExemplaires()
        {
            Exemplaires = Exemplaires.
                OrderBy(bc => bc.Price).
                OrderByDescending(bc => bc.Condition).
                OrderBy(bc => bc.Book.Title).ToList();
        }

        private void GetExemplaires()
        {
            db = new ApplicationDbContext();

            InGetExemplaires = true;

            if (!IsFiltering)
            {
                if (SelectedBook != null && !SelectedBook.Equals(BookList.ElementAt(0)))
                {
                    Exemplaires = db.GetBookExemplaires(SelectedBook);
                }
                else if (SelectedCourse != null && !SelectedCourse.Equals(CourseList.ElementAt(0)))
                {
                    Exemplaires = db.GetCourseExemplaires(SelectedCourse);
                }
                else if (!SelectedProgram.Equals(ProgramList.ElementAt(0)))
                {
                    Exemplaires = db.GetProgramExemplaires(SelectedProgram);
                }
                else if (SelectedBook != null && SelectedBook.Equals(BookList.ElementAt(0)) &&
                    !SelectedCourse.Equals(CourseList.ElementAt(0)))
                {
                    Exemplaires = db.GetBookListExemplaires(BookList);
                }
                else if (SelectedCourse != null && SelectedCourse.Equals(CourseList.ElementAt(0)) &&
                    !SelectedProgram.Equals(ProgramList.ElementAt(0)))
                {
                    Exemplaires = db.GetCourseListExemplaires(CourseList);
                }
                else if (SelectedProgram.Equals(ProgramList.ElementAt(0)))
                {
                    Exemplaires = db.GetProgramListExemplaires(ProgramList);
                }

                FilterExemplaires();

                InGetExemplaires = false;
            }
        }

        // ouvre la page de détail quand on clique sur details d'un exemplaire 
        private void GoToDetailPage(BookCopy exemplaire)
        {
            if (!IsVisitor)
            {
                if (Application.Current.Windows.OfType<DetailsWindow>().Any())
                {
                    detailsWindow.Close();
                }

                detailsWindow = new DetailsWindow(exemplaire);
                detailsWindow.Show();
            }
            else
            {
                if (MessageVisibility == Visibility.Hidden)
                {
                    MessageContent = "Cette fonction est réservée aux membres";
                    MessageVisibility = Visibility.Visible;
                }
            }
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

            if (CurrentPage == 1)
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

        private void StartTimer()
        {
            // le message reste affiché 2 secondes
            time = TimeSpan.FromSeconds(3);

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
