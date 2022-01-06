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
using System.Windows.Threading;

namespace ManuRecyEco.ViewModels
{
    public class PublishBookViewModel: ObservableObject
    {
        public class Condition: ObservableObject
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

        private ApplicationDbContext db;

        private string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                MessageVisibility = Visibility.Hidden;

                OnPropertyChanged(ref _imagePath, value);
            }
        }

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

        private bool IsUpdatingSelectedAcademicProgram;

        // programme choisi dans la liste déroulante
        private AcademicProgram _selectedAcademicProgram;
        public AcademicProgram SelectedAcademicProgram
        {
            get { return _selectedAcademicProgram; }
            set
            {
                MessageVisibility = Visibility.Hidden;

                OnPropertyChanged(ref _selectedAcademicProgram, value);

                IsUpdatingSelectedAcademicProgram = true;

                // si le nouveau choix est le choix par défault
                if (_selectedAcademicProgram.Equals(AcademicProgramList.ElementAt(0)))
                {
                    AcademicCourseList = db.GetCourses(AcademicProgramList);
                }
                else
                {
                    AcademicCourseList = db.GetCourses(SelectedAcademicProgram);
                }

                // peu importe le choix on set la liste de livres
                // à tous les cours puisqu'on change de programme
                BookList = db.GetBooks(AcademicCourseList);

                IsUpdatingSelectedAcademicProgram = false;
            }
        }

        // liste des programmes de la liste déroulante
        private List<AcademicProgram> _academicProgramList;
        public List<AcademicProgram> AcademicProgramList
        {
            get { return _academicProgramList; }
            set { OnPropertyChanged(ref _academicProgramList, value); }
        }

        // cours choisi dans la liste déroulante
        private AcademicCourse _selectedAcademicCourse;
        public AcademicCourse SelectedAcademicCourse
        {
            get { return _selectedAcademicCourse; }
            set
            {
                MessageVisibility = Visibility.Hidden;
                OnPropertyChanged(ref _selectedAcademicCourse, value);

                // on fait des queries seulement si on a manuellement fait
                // un choix dans la liste et non à cause du update de programme
                // parce que dans ce cas on se charge déja des livres
                if(!IsUpdatingSelectedAcademicProgram)
                {
                    // si le nouveau choix est le choix par défault
                    if (_selectedAcademicCourse.Equals(AcademicCourseList.ElementAt(0)))
                    {
                        BookList = db.GetBooks(AcademicCourseList);
                    }
                    else
                    {
                        BookList = db.GetBooks(SelectedAcademicCourse);
                    }
                }
            }
        }

        // liste des cours de la liste déroulante
        private List<AcademicCourse> _academicCourseList;
        public List<AcademicCourse> AcademicCourseList
        {
            get { return _academicCourseList; }
            set
            {
                // quand la liste change (ici seulement quand on faire une query pour la réinitialiser),
                // on ajoute la valeur par défault à la liste
                value.Insert(0, new AcademicCourse { Id = 0, Acronym = "Cours pour filtrer la liste de livres", Name = String.Empty });

                OnPropertyChanged(ref _academicCourseList, value);

                // on set le livre sélectionné à la valeur par défault
                SelectedAcademicCourse = _academicCourseList.ElementAt(0);
            }
        }

        // livre choisi dans la liste déroulante
        private Book _selectedBook;
        public Book SelectedBook
        {
            get { return _selectedBook; }
            set
            {
                MessageVisibility = Visibility.Hidden;
                OnPropertyChanged(ref _selectedBook, value);

                if (_selectedBook != null)
                {
                    // si le nouveau choix est le choix par défault
                    if (_selectedBook.Equals(BookList.ElementAt(0)))
                    {
                        StrRefPrice = String.Empty;
                        MiscInfos = String.Empty;
                    }
                    else
                    {
                        StrRefPrice = _selectedBook.ReferencePrice <= 0 ?
                            AppStrings.NonAppl :
                            _selectedBook.ReferencePrice.ToString("F") + "$";

                        MiscInfos = _selectedBook.Publisher + ", " +
                            _selectedBook.Year + ", " +
                            _selectedBook.NumPages + " pages";
                    }
                }
            }
        }

        // liste des livres de la liste déroulante
        private List<Book> _bookList;
        public List<Book> BookList
        {
            get { return _bookList; }
            set
            {
                // quand la liste change (ici seulement quand on faire une query pour la réinitialiser),
                // on ajoute la valeur par défault à la liste
                value.Insert(0, new Book { Id = 0, Title = "Veuillez choisir un livre", ISBN = String.Empty, Author = String.Empty });

                OnPropertyChanged(ref _bookList, value);

                // on set le livre sélectionné à la valeur par défault
                SelectedBook = _bookList.ElementAt(0);

                // si la nouvelle liste contient 
                // seulement le choix par défault
                if (_bookList.Count == 1)
                {
                    MessageContent = "Il n'y a pas de livre pour ce cours";
                    MessageVisibility = Visibility.Visible;
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

                // effacer le contenu et mettre disabled le TextBox de prix
                if (value == AppStrings.TransactionEchange)
                {
                    IsNotTrading = false;
                    StrPrice = String.Empty;
                }
                else
                {
                    IsNotTrading = true;
                }

                OnPropertyChanged(ref _selectedTransaction, value); 
            }
        }

        // liste des types de transactions de la liste déroulante
        private List<string> _transactionList;
        public List<string> TransactionList
        {
            get { return _transactionList; }
            set { OnPropertyChanged(ref _transactionList, value); }
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

        // true si on fait un échange
        private bool _isNotTrading;
        public bool IsNotTrading
        {
            get { return _isNotTrading; }
            set { OnPropertyChanged(ref _isNotTrading, value); }
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

        // contenu du message
        private string _messageContent;
        public string MessageContent
        {
            get { return _messageContent; }
            set { OnPropertyChanged(ref _messageContent, value); }
        }

        // éditeur, année et nb pages
        private string _miscInfos;
        public string MiscInfos
        {
            get { return _miscInfos; }
            set { OnPropertyChanged(ref _miscInfos, value); }
        }

        // prix de référence formatté
        private string _strRefPrice;
        public string StrRefPrice
        {
            get { return _strRefPrice; }
            set { OnPropertyChanged(ref _strRefPrice, value); }
        }

        public ICommand PublishOfferCommand { get; private set; }

        private bool IsValid;

        private int CurrentUserId;

        // timer pour les messages
        private DispatcherTimer timer;

        private TimeSpan time;

        public PublishBookViewModel(int CurrentUserId)
        {
            this.CurrentUserId = CurrentUserId;

            db = new ApplicationDbContext();

            // liste des programmes à l'UQAM
            AcademicProgramList = db.GetUqamProgramListExceptDefault();

            AcademicProgramList.Insert(0, new AcademicProgram { Id = 0, Name = "Programme pour filtrer la liste de livres" });
            SelectedAcademicProgram = AcademicProgramList.ElementAt(0);

            // choix de transactions
            TransactionList = new List<string>() { "Veuillez choisir une transaction",
                                                   AppStrings.TransactionVente,
                                                   AppStrings.TransactionEchange,
                                                   AppStrings.TransactionVenteEchange };

            // choix pour l'état du livre
            ConditionList = new List<Condition>() { new Condition { Index = 0, Description = "Veuillez choisir l'état du livre" },
                                                    new Condition { Index = 1, Description = "1 (très mauvais état)" },
                                                    new Condition { Index = 2, Description = "2" },
                                                    new Condition { Index = 3, Description = "3" },
                                                    new Condition { Index = 4, Description = "4" },
                                                    new Condition { Index = 5, Description = "5 (état moyen)" },
                                                    new Condition { Index = 6, Description = "6" },
                                                    new Condition { Index = 7, Description = "7" },
                                                    new Condition { Index = 8, Description = "8" },
                                                    new Condition { Index = 9, Description = "9" },
                                                    new Condition { Index = 10, Description = "10 (état neuf)" } };

            // initialisation des éléments sélectionnés avec les valeurs par défault
            SelectedCondition = ConditionList.ElementAt(0);
            SelectedTransaction = TransactionList.ElementAt(0);
            StrPrice = String.Empty;
            ImagePath = String.Empty;

            // message caché
            MessageVisibility = Visibility.Hidden;
            MessageContent = String.Empty;

            PublishOfferCommand = new RelayCommand(PublishOffer);

            IsUpdatingSelectedAcademicProgram = false;
        }

        private void PublishOffer()
        {
            IsValid = true;
            double price = 0;

            if(SelectedBook.Equals(BookList.ElementAt(0)))
            {
                DisplayValidationErrorMessage("Vous devez choisir un livre.");
            }
            else if (SelectedTransaction == TransactionList.ElementAt(0))
            {
                DisplayValidationErrorMessage("Vous devez choisir un type de transaction.");
            }
            else if (SelectedCondition.Equals(ConditionList.ElementAt(0)))
            {
                DisplayValidationErrorMessage("Vous devez choisir l'état du livre.");
            }
            else if (IsNotTrading && !double.TryParse(StrPrice.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out price))
            {
                DisplayValidationErrorMessage("Le prix entré n'est pas valide.");
            }
            else if(IsNotTrading && !(price > 0))
            {
                DisplayValidationErrorMessage("Le prix entré doit être supérieur à 0.");
            }
            else if (ImagePath != String.Empty && (Path.GetExtension(ImagePath).ToUpper() != ".PNG" && Path.GetExtension(ImagePath).ToUpper() != ".JPG"))
            {
                DisplayValidationErrorMessage("Le format d'image doit être PNG ou JPG.");
                ImagePath = String.Empty;
            }

            if (IsValid)
            {
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
                }

                // ajout de l'exemplaire
                db.BookCopies.Add(new BookCopy
                {
                    UserId = CurrentUserId,
                    BookId = SelectedBook.Id,
                    Condition = SelectedCondition.Index,
                    TransactionType = SelectedTransaction,
                    Price = Math.Round(price, 2),
                    ImageCopyId = copy.IsUsed ? copy.Id : SelectedBook.ImageCopyId
                });
                db.SaveChanges();

                // confirmation et reset des champs
                SelectedAcademicProgram = AcademicProgramList.ElementAt(0);
                SelectedAcademicCourse = AcademicCourseList.ElementAt(0);
                SelectedBook = BookList.ElementAt(0);
                SelectedCondition = ConditionList.ElementAt(0);
                SelectedTransaction = TransactionList.ElementAt(0);
                StrPrice = String.Empty;
                ImagePath = String.Empty;

                MessageContent = "Votre offre a été publiée!";
                MessageVisibility = Visibility.Visible;
            }
        }

        private void DisplayValidationErrorMessage(string message)
        {
            MessageContent = message;
            MessageVisibility = Visibility.Visible;
            IsValid = false;
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
