using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using ManuRecyEco.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ManuRecyEco.ViewModels
{
    public class EmailConfirmationViewModel : ObservableObject
    {
        private ApplicationDbContext db;

        private MainWindowViewModel _mainWindowVM;
        public MainWindowViewModel MainWindowVM
        {
            get { return _mainWindowVM; }
            set { OnPropertyChanged(ref _mainWindowVM, value); }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { OnPropertyChanged(ref _email, value); }
        }

        private User CurrentUser;
        private string Token;

        // bouton "retour"
        private void Exit()
        {
            // on set la vue de la fenêtre à une nouvelle vue de login
            MainWindowVM.CurrentView = new LoginViewModel(MainWindowVM);
        }

        private bool _emailNotSent;
        public bool EmailNotSent
        {
            get { return _emailNotSent; }
            set { OnPropertyChanged(ref _emailNotSent, value); }
        }

        // bouton "Envoyer courriel"
        private void SendEmail()
        {
            // Pour exemple avec user 2 que j'ai cree dans ma db
            CurrentUser = db.Users.ToList().DefaultIfEmpty(null).Where(x => x != null && x.Email == Email).SingleOrDefault();

            if (CurrentUser == null)
            {
                MessageBox.Show(AppStrings.EmailMatch, AppStrings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Token = EmailUtil.GenerateToken();

                // on envoie le courriel
                EmailUtil.SendTokenEmail(CurrentUser.Email, Token, new SendCompletedEventHandler(SendCompletedCallback));
            }
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            MailMessage msg = (MailMessage)e.UserState;

            if (e.Cancelled)
            {
                MessageBox.Show(AppStrings.TokenNotSent + CurrentUser.Email, AppStrings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (e.Error != null)
            {
                MessageBox.Show(AppStrings.TokenNotSent + CurrentUser.Email, AppStrings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (msg != null)
                    msg.Dispose();

                // on insère le jeton dans la BD
                db.AddToken(CurrentUser, Token);

                EmailNotSent = false;

                MessageBox.Show("Courriel envoyé à l'adresse " + CurrentUser.Email + ".", AppStrings.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // redirection vers la View où on entre le token
            MainWindowVM.CurrentView = new TokenConfirmationViewModel(MainWindowVM, CurrentUser.Id, false, true);
        }

        // Commande du bouton "Renvoyer courriel"
        public ICommand SendEmailCommand { get; private set; }

        // Commande du bouton "Quitter"
        public ICommand ExitCommand { get; private set; }

        public EmailConfirmationViewModel(MainWindowViewModel MainWindowVM)
        {
            db = new ApplicationDbContext();

            this.MainWindowVM = MainWindowVM;

            // Commande envoyer courriel
            SendEmailCommand = new RelayCommand(SendEmail);

            // Commande du bouton "retour"
            ExitCommand = new RelayCommand(Exit);
        }
    }
}

