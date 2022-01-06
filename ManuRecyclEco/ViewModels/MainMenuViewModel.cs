using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ManuRecyEco.ViewModels
{
    public class MainMenuViewModel: ObservableObject
    {
        private MainWindowViewModel _mainWindowVM;
        public MainWindowViewModel MainWindowVM
        {
            get { return _mainWindowVM; }
            set { OnPropertyChanged(ref _mainWindowVM, value); }
        }

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { OnPropertyChanged(ref _currentView, value); }
        }

        private bool _myOffersEnabled;
        public bool MyOffersEnabled
        {
            get { return _myOffersEnabled; }
            set { OnPropertyChanged(ref _myOffersEnabled, value); }
        }

        public ICommand ProfileCommand { get; private set; }
        public ICommand BooksCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand PublishCommand { get; private set; }
        public ICommand MyOffersCommand { get; private set; }
        public ICommand MessagesCommand { get; private set; }
        public ICommand LogOutCommand { get; private set; }

        private ApplicationDbContext db;

        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set { OnPropertyChanged(ref _currentUser, value); }
        }

        public MainMenuViewModel(MainWindowViewModel MainWindowVM, int CurrentUserId)
        {
            this.MainWindowVM = MainWindowVM;

            db = new ApplicationDbContext();

            InitViewModel(CurrentUserId);
        }

        public MainMenuViewModel(MainWindowViewModel MainWindowVM)
        {
            this.MainWindowVM = MainWindowVM;

            db = new ApplicationDbContext();
        }

        public void InitViewModel(int CurrentUserId)
        {
            CurrentUser = db.Users.Find(CurrentUserId);

            db.SetUserLastLogin(CurrentUser);

            ProfileCommand = new RelayCommand(Profile);
            BooksCommand = new RelayCommand(Books);
            SearchCommand = new RelayCommand(Search);
            PublishCommand = new RelayCommand(Publish);
            MyOffersCommand = new RelayCommand(MyOffers);
            MessagesCommand = new RelayCommand(Messages);
            LogOutCommand = new RelayCommand(LogOut);

            // on commence toujours dans le profil
            CurrentView = new ProfileViewModel(MainWindowVM, CurrentUser.Id);

            // si le user a au moins une offre,
            // on met le bouton "Mes offres" enabled
            MyOffersEnabled = false;
            if (db.GetUserBookCopyCount(CurrentUser) > 0)
            {
                MyOffersEnabled = true;
            }
        }

        private void Profile()
        {
            if(!(CurrentView is ProfileViewModel))
            {
                CurrentView = new ProfileViewModel(MainWindowVM, CurrentUser.Id);
            }
        }

        private void Books()
        {
            if (!(CurrentView is BookListingViewModel))
            {
                CurrentView = new BookListingViewModel(this, CurrentUser.Id);
            }
        }

        private void Search()
        {
            if (!(CurrentView is SearchViewModel))
            {
                CurrentView = new SearchViewModel(CurrentUser.Id);
            }
        }

        private void Publish()
        {
            if (!(CurrentView is PublishBookViewModel))
            {
                CurrentView = new PublishBookViewModel(CurrentUser.Id);
            }
        }

        private void MyOffers()
        {
            if (!(CurrentView is MyOffersViewModel))
            {
                CurrentView = new MyOffersViewModel(CurrentUser.Id);
            }
        }

        private void Messages()
        {

        }

        private void LogOut()
        {
            MainWindowVM.CurrentView = new LoginViewModel(MainWindowVM);
        }
    }
}
