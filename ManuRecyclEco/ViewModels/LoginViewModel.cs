using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ManuRecyEco.ViewModels
{
    public class LoginViewModel: ObservableObject
    {
        private ApplicationDbContext db;

        private MainWindowViewModel _mainWindowVM;
        public MainWindowViewModel MainWindowVM
        {
            get { return _mainWindowVM; }
            set { OnPropertyChanged(ref _mainWindowVM, value); }
        }

        private MainMenuViewModel _mainMenuVM;
        public MainMenuViewModel MainMenuVM
        {
            get { return _mainMenuVM; }
            set { OnPropertyChanged(ref _mainMenuVM, value); }
        }

        private SearchViewModel _searchVM;
        public SearchViewModel SearchVM
        {
            get { return _searchVM; }
            set { OnPropertyChanged(ref _searchVM, value); }
        }

        private string _loginKey;
        public string LoginKey
        {
            get { return _loginKey; }
            set { OnPropertyChanged(ref _loginKey, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { OnPropertyChanged(ref _password, value); }
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

        public ICommand AuthentifyCommand { get; private set; }
        public ICommand CreateAccountCommand { get; private set; }
        public ICommand ForgotPasswordCommand { get; private set; }
        public ICommand GoToSearchCommand { get; private set; }

        // timer pour les messages
        private DispatcherTimer timer;

        private TimeSpan time;

        private User CurrentUser;
        private string Token;

        public LoginViewModel(MainWindowViewModel MainWindowVM)
        {
            db = new ApplicationDbContext();

            this.MainWindowVM = MainWindowVM;

            SearchVM = new SearchViewModel(MainWindowVM);
            MainMenuVM = new MainMenuViewModel(MainWindowVM);

            // important pour ne pas avoir de NullPointerException dans
            // Authentify() si jamais l'utilisateur ne rentre rien
            Password = string.Empty;
            LoginKey = string.Empty;
            Token = string.Empty;

            // commande du bouton "Authentification"
            AuthentifyCommand = new RelayCommand(Authentify, () => !String.IsNullOrEmpty(Password));

            // commande du bouton "Créer un compte"
            CreateAccountCommand = new RelayCommand(RedirectToCreateAccount);

            // commande de l'hyperlink "Mot de pass oublié?"
            ForgotPasswordCommand = new RelayCommand(RedirectToForgotPassword);

            GoToSearchCommand = new RelayCommand(GoToSearch);

            // message caché
            MessageVisibility = Visibility.Hidden;
            MessageContent = String.Empty;
        }

        private void Authentify()
        {
            // ici validation avec la base de donnée,
            // utiliser Password et Username pour valider
            // get le user
            CurrentUser = Regex.IsMatch(LoginKey, AppStrings.EmailRegex) ?
                db.Users.ToList().DefaultIfEmpty(null).Where(x => x != null && x.Email == LoginKey).SingleOrDefault() : 
                db.Users.ToList().DefaultIfEmpty(null).Where(x => x != null && x.Username == LoginKey).SingleOrDefault();


            if (CurrentUser == null || !CurrentUser.PasswordMatch(Password)) 
            {
                MessageContent = AppStrings.InvalidLoginInfo;
                MessageVisibility = Visibility.Visible;
                return;
            }

            if(CurrentUser != null && !CurrentUser.IsActive)
            {
                // jeton aléatoire de 16 caractères
                Token = EmailUtil.GenerateToken();

                // on envoie le courriel
                EmailUtil.SendTokenEmail(CurrentUser.Email, Token, new SendCompletedEventHandler(SendCompletedCallback));
            }
            else
            {
                // si valide, la view principale est maintenant le MainMenuView
                MainMenuVM.InitViewModel(CurrentUser.Id);
                MainWindowVM.CurrentView = MainMenuVM;
            }        
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            MailMessage msg = (MailMessage)e.UserState;

            if (e.Cancelled)
            {
                MessageContent = AppStrings.TokenNotSent + CurrentUser.Email;
                MessageVisibility = Visibility.Visible;
            }
            if (e.Error != null)
            {
                MessageContent = AppStrings.TokenNotSent + CurrentUser.Email;
                MessageVisibility = Visibility.Visible;
            }
            else
            {
                if (msg != null)
                    msg.Dispose();

                // on insère le jeton dans la BD
                db.AddToken(CurrentUser, Token);
            }

            // redirection vers la View où on entre le token
            MainWindowVM.CurrentView = new TokenConfirmationViewModel(MainWindowVM, CurrentUser.Id, true, false);
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

        private void RedirectToCreateAccount()
        {
            // la view principale est maintenant un nouveau CreateAccountView
            MainWindowVM.CurrentView = new CreateAccountViewModel(MainWindowVM);
        }

        private void RedirectToForgotPassword()
        {
            MainWindowVM.CurrentView = new EmailConfirmationViewModel(MainWindowVM);
        }

        private void GoToSearch()
        {
            MainWindowVM.CurrentView = SearchVM;
        }
      
    }
}
