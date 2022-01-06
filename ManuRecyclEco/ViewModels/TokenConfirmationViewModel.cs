using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ManuRecyEco.ViewModels
{
    public class TokenConfirmationViewModel: ObservableObject
    {
        private ApplicationDbContext db;

        private User CurrentUser;

        // true si on vient d'une tentative de login
        // sur un user inactif
        private bool inactiveUserOnLogin;

        // true si on vient de la View email du password reset
        private bool passwordReset;

        // true si le token correspond à celui de la BD
        private bool tokenIsValid;

        private MainWindowViewModel _mainWindowVM;
        public MainWindowViewModel MainWindowVM
        {
            get { return _mainWindowVM; }
            set { OnPropertyChanged(ref _mainWindowVM, value); }
        }

        // string token aléatoire de 16 caractères
        private string _token;
        public string Token
        {
            get { return _token; }
            set { OnPropertyChanged(ref _token, value); }
        }

        // true si le bouton "Renvoyer courriel" est activé
        private bool _emailNotSent;
        public bool EmailNotSent
        {
            get { return _emailNotSent; }
            set { OnPropertyChanged(ref _emailNotSent, value); }
        }

        // string token aléatoire de 16 caractères
        private string _newEmail;
        public string NewEmail
        {
            get { return _newEmail; }
            set { OnPropertyChanged(ref _newEmail, value); }
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

        // Commande du bouton "Valider"
        public ICommand ValidateCommand { get; private set; }

        // Commande du bouton "Renvoyer courriel"
        public ICommand SendEmailCommand { get; private set; }

        // Commande du bouton "Quitter"
        public ICommand ExitCommand { get; private set; }

        private DispatcherTimer timer;

        private TimeSpan time;

        private string TokenGenerated;

        public TokenConfirmationViewModel(MainWindowViewModel MainWindowVM, int CurrentUserId, bool inactiveUserOnLogin, bool passwordReset)
        {
            // init BD
            db = new ApplicationDbContext();

            this.MainWindowVM = MainWindowVM;
            this.inactiveUserOnLogin = inactiveUserOnLogin;
            this.passwordReset = passwordReset;

            Token = string.Empty;
            NewEmail = string.Empty;
            TokenGenerated = string.Empty;

            // création des commande pour les boutons
            ValidateCommand = new RelayCommand(Validate);
            SendEmailCommand = new RelayCommand(SendEmail);
            ExitCommand = new RelayCommand(Exit);

            // on garde le bouton "Renvoyer courriel" activé
            EmailNotSent = true;

            this.CurrentUser = db.Users.Find(CurrentUserId);

            // message caché
            MessageVisibility = Visibility.Hidden;
            MessageContent = string.Empty;
        }

        public TokenConfirmationViewModel(MainWindowViewModel MainWindowVM, int CurrentUserId, string NewEmail)
        {
            // init BD
            db = new ApplicationDbContext();

            this.MainWindowVM = MainWindowVM;
            this.NewEmail = NewEmail;
            this.inactiveUserOnLogin = false;
            this.passwordReset = false;

            Token = string.Empty;
            TokenGenerated = string.Empty;

            // création des commande pour les boutons
            ValidateCommand = new RelayCommand(Validate);
            SendEmailCommand = new RelayCommand(SendEmail);
            ExitCommand = new RelayCommand(Exit);

            // on garde le bouton "Renvoyer courriel" activé
            EmailNotSent = true;

            this.CurrentUser = db.Users.Find(CurrentUserId);

            // message caché
            MessageVisibility = Visibility.Hidden;
            MessageContent = string.Empty;
        }

        // bouton "Valider"
        private void Validate()
        {
            tokenIsValid = true;

            // on va cherche le token le plus récent du user
            //String dbToken = db.GetLatestToken(user.Username);
            Token dbToken = db.Tokens.ToList().DefaultIfEmpty(null).Where(x => x.User.Equals(CurrentUser)).OrderByDescending(x => x.SentTime).First();

            // si inexistant ou invalide
            if (dbToken == null || !dbToken.RandomString.Equals(Token))
            {
                MessageContent = "Jeton invalide!";
                MessageVisibility = Visibility.Visible;
                tokenIsValid = false;
            }

            if (tokenIsValid)
            {
                // si on vient du profil on set le nouveau email
                if(NewEmail != string.Empty)
                {
                    CurrentUser.Email = NewEmail;
                }

                // set le user actif dans la BD
                CurrentUser.IsActive = true;
                db.Users.Update(CurrentUser);

                // supprime tous les tokens du user dans la BD
                //db.DeleteUserTokens(user.Username);
                db.Tokens.RemoveRange(CurrentUser.Tokens);
                db.SaveChanges();

                // si on vient du profil ou d'un user inactif au login
                // on va au profil
                if (inactiveUserOnLogin || NewEmail != string.Empty)
                {
                    MainWindowVM.CurrentView = new MainMenuViewModel(MainWindowVM, CurrentUser.Id);
                }
                else if(passwordReset)
                {
                    // si on vient du password reset
                    MainWindowVM.CurrentView = new PasswordResetViewModel(MainWindowVM, CurrentUser.Id);
                }
                else
                {
                    // si on vient de la création du compte
                    MainWindowVM.CurrentView = new LoginViewModel(MainWindowVM);
                }
            }
        }

        // bouton "Renvoyer courriel"
        private void SendEmail()
        {
            // jeton aléatoire de 16 caractères
            TokenGenerated = EmailUtil.GenerateToken();

            string email = NewEmail != String.Empty ? NewEmail : CurrentUser.Email;

            // on envoie le courriel
            EmailUtil.SendTokenEmail(email, TokenGenerated, new SendCompletedEventHandler(SendCompletedCallback));

            MessageContent = "Courriel envoyé à l'adresse " + email;
            MessageVisibility = Visibility.Visible;
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            string email = NewEmail != String.Empty ? NewEmail : CurrentUser.Email;

            MailMessage msg = (MailMessage)e.UserState;

            if (e.Cancelled)
            {
                MessageContent = AppStrings.TokenNotSent + email;
                MessageVisibility = Visibility.Visible;
            }
            if (e.Error != null)
            {
                MessageContent = AppStrings.TokenNotSent + email;
                MessageVisibility = Visibility.Visible;
            }
            else
            {
                if (msg != null)
                    msg.Dispose();

                // insertion du jeton dans la BD
                db.AddToken(CurrentUser, TokenGenerated);

                // on désactive le bouton 
                EmailNotSent = false;
            }
        }

        // bouton "Quitter"
        private void Exit()
        {
            // si on vient du profil
            // on retourne au profil
            if (NewEmail != String.Empty)
            {
                MainWindowVM.CurrentView = new MainMenuViewModel(MainWindowVM, CurrentUser.Id);
            }
            else
            {
                // sinon redirection vers la View de login
                MainWindowVM.CurrentView = new LoginViewModel(MainWindowVM);
            }
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
