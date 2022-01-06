using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ManuRecyEco.ViewModels
{
    public class ProfileViewModel : ObservableObject
    {
        private ApplicationDbContext db;

        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set { OnPropertyChanged(ref _currentUser, value); }
        }

        // ville choisie dans la liste déroulante
        private City _selectedCity;
        public City SelectedCity
        {
            get { return _selectedCity; }
            set { OnPropertyChanged(ref _selectedCity, value); }
        }

        // liste des villes de la liste déroulante
        private List<City> _cityList;
        public List<City> CityList
        {
            get { return _cityList; }
            set { OnPropertyChanged(ref _cityList, value); }
        }

        private AcademicProgram _selectedUserAcademicProgram;
        public AcademicProgram SelectedUserAcademicProgram
        {
            get { return _selectedUserAcademicProgram; }
            set
            {
                OnPropertyChanged(ref _selectedUserAcademicProgram, value);
            }
        }

        private List<AcademicProgram> _userAcademicProgramList;
        public List<AcademicProgram> UserAcademicProgramList
        {
            get { return _userAcademicProgramList; }
            set { OnPropertyChanged(ref _userAcademicProgramList, value); }
        }

        // programme choisi dans la liste déroulante
        private AcademicProgram _selectedAcademicProgram;
        public AcademicProgram SelectedAcademicProgram
        {
            get { return _selectedAcademicProgram; }
            set
            {
                MessageVisibility = Visibility.Hidden;

                OnPropertyChanged(ref _selectedAcademicProgram, value);

                // si le nouveau choix est le choix par défault
                if (_selectedAcademicProgram.Equals(AcademicProgramList.ElementAt(0)))
                {
                    AcademicCourseList = db.GetCourses(AcademicProgramList);
                }
                else
                {
                    AcademicCourseList = db.GetCourses(SelectedAcademicProgram);
                }

                SelectedAcademicCourse = AcademicCourseList.ElementAt(0);
            }
        }

        // liste des programmes de la liste déroulante
        private List<AcademicProgram> _academicProgramList;
        public List<AcademicProgram> AcademicProgramList
        {
            get { return _academicProgramList; }
            set { OnPropertyChanged(ref _academicProgramList, value); }
        }

        // liste des cours de la liste déroulante
        private List<AcademicCourse> _academicCourseList;
        public List<AcademicCourse> AcademicCourseList
        {
            get { return _academicCourseList; }
            set
            {
                // si la ListBox est pas null, on enlève
                // de la liste de cours les cours du ListBox
                if (ListBoxCourseList != null)
                {
                    value = value.Except(ListBoxCourseList).
                        OrderBy(c => c.Acronym).ToList();
                }

                // quand la liste change (ici seulement quand on faire une query pour la réinitialiser),
                // on ajoute la valeur par défault à la liste
                value.Insert(0, new AcademicCourse { Id = 0, Acronym = "Veuillez ajouter un cours", Name = String.Empty });

                OnPropertyChanged(ref _academicCourseList, value);
            }
        }

        // nouveau code

        // cours sélectionné dans la liste déroulante
        private AcademicCourse _selectedAcademicCourse;
        public AcademicCourse SelectedAcademicCourse
        {
            get { return _selectedAcademicCourse; }
            set
            {
                OnPropertyChanged(ref _selectedAcademicCourse, value);

                // si c'est pas triggered à partir de la fonction delete et que le cours
                // est pas null et que c'est pas la valeur par défault
                if(!ComingFromDelete &&_selectedAcademicCourse != null &&
                    !_selectedAcademicCourse.Equals(AcademicCourseList.ElementAt(0)))
                {
                    // on ajoute le cours au Listbox
                    ListBoxCourseList = ListBoxCourseList.Append(_selectedAcademicCourse).
                        OrderBy(c => c.Acronym).ToList();

                    // on refresh la liste de cours avec le cours ajouté au ListBox en moins
                    if (SelectedAcademicProgram.Equals(AcademicProgramList.ElementAt(0)))
                    {
                        AcademicCourseList = db.GetCourses(AcademicProgramList);
                    }
                    else
                    {
                        AcademicCourseList = db.GetCourses(SelectedAcademicProgram);
                    }

                    SelectedListBoxCourse = null;

                    OnPropertyChanged(ref _selectedAcademicCourse, AcademicCourseList.ElementAt(0));
                }
            }
        }

        // Liste de cours du ListBox
        private List<AcademicCourse> _listBoxCourseList;
        public List<AcademicCourse> ListBoxCourseList
        {
            get { return _listBoxCourseList; }
            set { OnPropertyChanged(ref _listBoxCourseList, value); }
        }

        // cours sélectionné du ListBox
        private AcademicCourse _selectedListBoxCourse;
        public AcademicCourse SelectedListBoxCourse
        {
            get { return _selectedListBoxCourse; }
            set
            {
                OnPropertyChanged(ref _selectedListBoxCourse, value);

                // si on a pas de sélection on set
                // le bouton supprimé à désactivé
                if(_selectedListBoxCourse == null)
                {
                    DeleteButtonEnabled = false;
                }
                else
                {
                    DeleteButtonEnabled = true;
                }
            }
        }

        // true s'il y a au moins un cours dans le ListBox
        private bool _deleteButtonEnabled;
        public bool DeleteButtonEnabled
        {
            get { return _deleteButtonEnabled; }
            set { OnPropertyChanged(ref _deleteButtonEnabled, value); }
        }

        // programme choisi dans la liste déroulante pour les livres
        private AcademicProgram _bookSelectedAcademicProgram;
        public AcademicProgram BookSelectedAcademicProgram
        {
            get { return _bookSelectedAcademicProgram; }
            set
            {
                MessageVisibility = Visibility.Hidden;

                OnPropertyChanged(ref _bookSelectedAcademicProgram, value);

                // si le nouveau choix est le choix par défault
                if (_bookSelectedAcademicProgram.Equals(AcademicProgramList.ElementAt(0)))
                {
                    BookAcademicCourseList = db.GetCourses(AcademicProgramList);
                }
                else
                {
                    BookAcademicCourseList = db.GetCourses(BookSelectedAcademicProgram);
                }

                BookSelectedAcademicCourse = BookAcademicCourseList.ElementAt(0);
            }
        }

        // liste des cours de la liste déroulante pour livres
        private List<AcademicCourse> _bookAcademicCourseList;
        public List<AcademicCourse> BookAcademicCourseList
        {
            get { return _bookAcademicCourseList; }
            set
            {
                // quand la liste change (ici seulement quand on faire une query pour la réinitialiser),
                // on ajoute la valeur par défault à la liste
                value.Insert(0, new AcademicCourse { Id = 0, Acronym = "Tous les cours offerts", Name = String.Empty });

                OnPropertyChanged(ref _bookAcademicCourseList, value);
            }
        }

        // nouveau code

        // cours sélectionné dans la liste déroulante de livres
        private AcademicCourse _bookSelectedAcademicCourse;
        public AcademicCourse BookSelectedAcademicCourse
        {
            get { return _bookSelectedAcademicCourse; }
            set
            {
                OnPropertyChanged(ref _bookSelectedAcademicCourse, value);

                if (_bookSelectedAcademicCourse != null)
                {
                    // si le nouveau choix est le choix par défault
                    if (_bookSelectedAcademicCourse.Equals(BookAcademicCourseList.ElementAt(0)))
                    {
                        BookList = db.GetBooks(BookAcademicCourseList);
                    }
                    else
                    {
                        BookList = db.GetBooks(BookSelectedAcademicCourse);
                    }

                    SelectedBook = BookList.ElementAt(0);
                }
            }
        }

        // Liste de livres
        private List<Book> _bookList;
        public List<Book> BookList
        {
            get { return _bookList; }
            set
            {
                // si la ListBox est pas null, on enlève
                // de la liste de livres les livres du ListBox
                if (ListBoxBookList != null)
                {
                    value = value.Except(ListBoxBookList).
                        OrderBy(c => c.Title).ToList();
                }

                value.Insert(0, new Book { Id = 0, Title = "Veuillez ajouter un livre" });

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

                // si c'est pas triggered à partir de la fonction delete et que le livre
                // est pas null et que c'est pas la valeur par défault
                if (!ComingFromDelete && _selectedBook != null &&
                    !_selectedBook.Equals(BookList.ElementAt(0)))
                {
                    // on ajoute le livre au Listbox
                    ListBoxBookList = ListBoxBookList.Append(_selectedBook).
                        OrderBy(c => c.Title).ToList();

                    // on refresh la liste de livres avec le livre ajouté au ListBox en moins
                    if (_bookSelectedAcademicCourse.Equals(BookAcademicCourseList.ElementAt(0)))
                    {
                        BookList = db.GetBooks(BookAcademicCourseList);
                    }
                    else
                    {
                        BookList = db.GetBooks(BookSelectedAcademicCourse);
                    }

                    OnPropertyChanged(ref _selectedBook, BookList.ElementAt(0));

                    SelectedListBoxBook = null;
                }
            }
        }

        // Liste de livres du ListBox
        private List<Book> _listBoxBookList;
        public List<Book> ListBoxBookList
        {
            get { return _listBoxBookList; }
            set { OnPropertyChanged(ref _listBoxBookList, value); }
        }

        // livre sélectionné du ListBox
        private Book _selectedListBoxBook;
        public Book SelectedListBoxBook
        {
            get { return _selectedListBoxBook; }
            set
            {
                OnPropertyChanged(ref _selectedListBoxBook, value);

                // si on a pas de sélection on set
                // le bouton supprimé à désactivé
                if (_selectedListBoxBook == null)
                {
                    DeleteBookButtonEnabled = false;
                }
                else
                {
                    DeleteBookButtonEnabled = true;
                }
            }
        }

        // true s'il y a au moins un livre dans le ListBox
        private bool _deleteBookButtonEnabled;
        public bool DeleteBookButtonEnabled
        {
            get { return _deleteBookButtonEnabled; }
            set { OnPropertyChanged(ref _deleteBookButtonEnabled, value); }
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
            set { 
                OnPropertyChanged(ref _imageSource, value);
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { OnPropertyChanged(ref _email, value); }
        }

        private List<string> _styleList;
        public List<string> StyleList
        {
            get { return _styleList; }
            set { OnPropertyChanged(ref _styleList, value); }
        }

        private string _selectedStyle;
        public string SelectedStyle
        {
            get { return _selectedStyle; }
            set
            {
                OnPropertyChanged(ref _selectedStyle, value);

                if(_selectedStyle != null && !IsInit)
                {
                    CurrentUser.UiStyle = StyleList.IndexOf(_selectedStyle);
                    db.Update(CurrentUser);
                    db.SaveChanges();
                }
            }
        }


        private DispatcherTimer timer;

        private TimeSpan time;

        private bool ComingFromDelete;

        private string Token;

        public bool IsInit;

        // contenu du message
        private string _messageContent;
        public string MessageContent
        {
            get { return _messageContent; }
            set { OnPropertyChanged(ref _messageContent, value); }
        }

        private MainWindowViewModel _mainWindowVM;
        public MainWindowViewModel MainWindowVM
        {
            get { return _mainWindowVM; }
            set { OnPropertyChanged(ref _mainWindowVM, value); }
        }

        public ICommand ConfirmProfilUpdate { get; private set; }
        public ICommand DeleteSelectedCourse { get; private set; }
        public ICommand DeleteSelectedBook { get; private set; }

        public ProfileViewModel(MainWindowViewModel MainWindowVM, int CurrentUserId)
        {
            IsInit = true;

            db = new ApplicationDbContext();

            this.CurrentUser = db.Users.Find(CurrentUserId);

            this.MainWindowVM = MainWindowVM;

            ComingFromDelete = false;

            Token = string.Empty;

            // message caché
            MessageVisibility = Visibility.Hidden;
            MessageContent = String.Empty;

            // liste de styles
            StyleList = new List<string>()
            {
                "Défault",
                "Style 1",
                "Style 2",
                "Style 3"
            };

            SelectedStyle = StyleList.ElementAt(CurrentUser.UiStyle);

            // on set la ville
            CityList = db.Cities.OrderBy(n => n).ToList();

            SelectedCity = db.Cities.
                Where(c => c.Id == CurrentUser.CityId).Single();

            // on set l'avatar
            ImageSource = CurrentUser.ImageCopyId != null ?
                CurrentUser.ImageCopy.BlobToImage() :
                new BitmapImage(new Uri("pack://application:,,,/default-avatar.png"));

            // vide au début jusqu'à ce
            // qu'on upload une image
            ImagePath = String.Empty;

            // on set le email
            Email = CurrentUser.Email;

            // on set la ListBox au cours de la BD
            ListBoxCourseList = db.GetUserCourses(CurrentUserId);
            SelectedListBoxCourse = null;

            // on set la listbox aux livres de la BD
            ListBoxBookList = db.GetUserWantedBooks(CurrentUser);
            SelectedListBoxBook = null;

            // liste des programmes à l'UQAM pour le filtre des cours
            AcademicProgramList = db.GetUqamProgramListExceptDefault();

            AcademicProgramList.Insert(0, new AcademicProgram { Id = 0, Name = "Tous les programmes offerts" });

            UserAcademicProgramList = db.GetUqamProgramListExceptDefault();

            UserAcademicProgramList.Insert(0, new AcademicProgram { Id = 0, Name = "Aucun programme sélectionné" });

            SelectedUserAcademicProgram = UserAcademicProgramList.
                Where(p => p.Id == UserAcademicProgramList.
                    Where(p => p.Id == CurrentUser.AcademicProgramId).
                    Select(p => p.Id).FirstOrDefault()).Single();

            SelectedAcademicProgram = GetSelectedAcademicProgram();
            BookSelectedAcademicProgram = GetSelectedAcademicProgram();

            ConfirmProfilUpdate = new RelayCommand(ProfilUpdate);
            DeleteSelectedCourse = new RelayCommand(DeleteCourse);
            DeleteSelectedBook = new RelayCommand(DeleteBook);

            IsInit = false;
        }

        private AcademicProgram GetSelectedAcademicProgram()
        {
            return AcademicProgramList.
                Where(p => p.Id == AcademicProgramList.
                    Where(p => p.Id == CurrentUser.AcademicProgramId).
                    Select(p => p.Id).FirstOrDefault()).Single();
        }

        // crée et part le timer
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

        private void DeleteCourse()
        {
            ComingFromDelete = true;

            // on enlève le cours de la liste
            List<AcademicCourse> excludedCourse = new List<AcademicCourse> { SelectedListBoxCourse };
            ListBoxCourseList = ListBoxCourseList.Except(excludedCourse).OrderBy(c => c.Acronym).ToList();

            // on refresh la liste de cours pour rajouter le cours enlevé du listbox
            if (SelectedAcademicProgram.Equals(AcademicProgramList.ElementAt(0)))
            {
                AcademicCourseList = db.GetCourses(AcademicProgramList);
            }
            else
            {
                AcademicCourseList = db.GetCourses(SelectedAcademicProgram);
            }

            // on set le cours de la liste à la valeur par défault
            SelectedAcademicCourse = AcademicCourseList.ElementAt(0);

            SelectedListBoxCourse = null;
            ComingFromDelete = false;
        }

        private void DeleteBook()
        {
            ComingFromDelete = true;

            // on enlève le cours de la liste
            List<Book> excludedBook = new List<Book> { SelectedListBoxBook };
            ListBoxBookList = ListBoxBookList.Except(excludedBook).OrderBy(c => c.Title).ToList();

            // on refresh la liste de livres avec le livre ajouté au ListBox en moins
            if (_bookSelectedAcademicCourse.Equals(BookAcademicCourseList.ElementAt(0)))
            {
                BookList = db.GetBooks(BookAcademicCourseList);
            }
            else
            {
                BookList = db.GetBooks(BookSelectedAcademicCourse);
            }

            // on set le cours de la liste à la valeur par défault
            SelectedBook = BookList.ElementAt(0);

            SelectedListBoxBook = null;

            ComingFromDelete = false;
        }

        private void ProfilUpdate()
        {
            bool emailIsValid = true;
            bool emailHasChanged = false;

            // les cours des du ListBox
            List<AcademicCourse> courses = ListBoxCourseList;

            // on enlève les cours en double de la liste
            courses = courses.Distinct().ToList();

            // la liste de cours du user présentement dans la BD
            List<AcademicCourse> currentDbCourses = db.GetUserCourses(CurrentUser.Id);

            // la différence entre les cours des combobox et les cours de la BD
            // c'est à dire ceux à ajouter qui ne sont pas dans la BD            
            List<AcademicCourse> coursesToAdd = courses.Except(currentDbCourses).ToList();

            // la différence entre les cours de la BD et les cours des combobox
            // c'est à dire ceux à enlever qui sont déja dans la BD 
            List<AcademicCourse> coursesToRemove = currentDbCourses.Except(courses).ToList();

            // on ajoute tous les cours à ajouter
            foreach (AcademicCourse course in coursesToAdd)
            {
                db.Add(new AcademicCourseUser
                {
                    AcademicCourseId = course.Id,
                    AcademicCourse = course,
                    UserId = CurrentUser.Id,
                    User = CurrentUser
                });
            }

            // on enlève les cours à enlever
            foreach (AcademicCourse course in coursesToRemove)
            {
                AcademicCourseUser acu = db.AcademicCoursesUsers.
                                            Where(acu => acu.UserId == CurrentUser.Id &&
                                                  acu.AcademicCourseId == course.Id).Single();
                db.AcademicCoursesUsers.Remove(acu);
            }

            List<Book> books = ListBoxBookList;
            books = books.Distinct().ToList();
            List<Book> currentDbBooks = db.GetUserWantedBooks(CurrentUser);
            List<Book> booksToAdd = books.Except(currentDbBooks).ToList();
            List<Book> booksToRemove = currentDbBooks.Except(books).ToList();

            // on ajoute tous les livres à ajouter
            foreach (Book book in booksToAdd)
            {
                db.Add(new BookUser
                {
                    BookId = book.Id,
                    Book = book,
                    UserId = CurrentUser.Id,
                    User = CurrentUser
                });
            }

            // on enlève les livres à enlever
            foreach (Book book in booksToRemove)
            {
                BookUser bu = db.BooksUsers.
                                Where(bu => bu.UserId == CurrentUser.Id &&
                                bu.BookId == book.Id).Single();
                db.BooksUsers.Remove(bu);
            }

            ImageCopy copy = new ImageCopy();

            // traitement de l'image
            if (ImagePath != String.Empty)
            {
                string extension = Path.GetExtension(ImagePath).ToLower();
                copy.ImageToBlob(ImagePath, extension);
                db.Add(copy);

                // on supprime l'ancienne image
                if(CurrentUser.ImageCopy != null)
                {
                    db.Remove(CurrentUser.ImageCopy);
                }

                CurrentUser.ImageCopy = copy;
                CurrentUser.ImageCopyId = copy.Id;
            }

            // on save la ville
            CurrentUser.City = SelectedCity;
            CurrentUser.CityId = SelectedCity.Id;

            // on save le programme d'étude
            CurrentUser.AcademicProgramId = null;
            if (!SelectedUserAcademicProgram.Equals(UserAcademicProgramList.ElementAt(0)))
            {
                CurrentUser.AcademicProgramId = SelectedUserAcademicProgram.Id;
            }

            db.Users.Update(CurrentUser);

            SelectedAcademicProgram = GetSelectedAcademicProgram();
            BookSelectedAcademicProgram = GetSelectedAcademicProgram();

            // on enlève les espaces
            Email = Email.Trim();

            // si le email a changé
            if (CurrentUser.Email != Email)
            {
                emailHasChanged = true;

                if (!Regex.IsMatch(Email, AppStrings.EmailRegex, RegexOptions.IgnoreCase))
                {
                    // erreur de format
                    MessageContent = AppStrings.EmailFormat;
                    MessageVisibility = Visibility.Visible;
                    emailIsValid = false;
                }
                else if (db.Users.Where(x => x.Email == Email).Any())
                {
                    // le email existe déja dans la BD
                    MessageContent = AppStrings.EmailExists;
                    MessageVisibility = Visibility.Visible;
                    emailIsValid = false;
                }
            }

            // si on n'a pas d'erreur avec le format du email
            // ou qu'il n'a pas changé
            // on save tout sauf le nouveau email qui sera 
            // savé après le token validation
            if (emailIsValid)
            {
                db.SaveChanges();
                MessageContent = AppStrings.ProfilUpdated;
            }

            // si le email a changé et qu'il est valide
            if (emailHasChanged && emailIsValid)
            {
                // on génère le jeton
                Token = EmailUtil.GenerateToken();

                MessageContent = AppStrings.ProfilUpdatedEmail;

                // on envoie le email async
                EmailUtil.SendTokenEmail(Email, Token, new SendCompletedEventHandler(SendCompletedCallback));
            }

            MessageVisibility = Visibility.Visible;
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            MailMessage msg = (MailMessage)e.UserState;

            if (e.Cancelled)
            {
                MessageContent = AppStrings.TokenNotSent + Email;
                MessageVisibility = Visibility.Visible;
            }
            if (e.Error != null)
            {
                MessageContent = AppStrings.TokenNotSent + Email;
                MessageVisibility = Visibility.Visible;
            }
            else
            {
                if (msg != null)
                    msg.Dispose();

                // on sauve le jeton dans la BD
                db.AddToken(CurrentUser, Token);

                // redirection vers la View où on entre le token
                MainWindowVM.CurrentView = new TokenConfirmationViewModel(MainWindowVM, CurrentUser.Id, Email);
            }
        }
    }
}