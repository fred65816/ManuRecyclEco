using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ManuRecyEco.ViewModels
{
    public class PasswordResetViewModel: ObservableObject
    {
        private ApplicationDbContext db;

        private MainWindowViewModel _mainWindowVM;
        public MainWindowViewModel MainWindowVM
        {
            get { return _mainWindowVM; }
            set { OnPropertyChanged(ref _mainWindowVM, value); }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set { OnPropertyChanged(ref _currentUser, value); }
        }

        // les deux champs password
        // ne peuvent pas être bindés dans un PasswordBox
        public string Password;
        public string Password2;

        // commande du bouton "créer le compte"
        public ICommand UpdatePasswordCommand { get; private set; }

        // Commande du bouton "retour"
        public ICommand ExitCommand { get; private set; }

        // booléen pour la validation
        private bool canUpdatePassword;

        public PasswordResetViewModel(MainWindowViewModel MainWindowVM, int CurrentUserId)
        {
            db = new ApplicationDbContext();

            this.MainWindowVM = MainWindowVM;
            this.CurrentUser = db.Users.Find(CurrentUserId);

            Password = String.Empty;
            Password2 = String.Empty;

            // commande du bouton "Mise à jour"
            UpdatePasswordCommand = new RelayCommand(UpdatePassword);

            // Commande du bouton "retour"
            ExitCommand = new RelayCommand(Exit);
        }

        // bouton "retour"
        private void Exit()
        {
            // on set la vue de la fenêtre à une nouvelle vue de login
            MainWindowVM.CurrentView = new LoginViewModel(MainWindowVM);
        }

        // bouton "créer le compte"
        private void UpdatePassword()
        {
            canUpdatePassword = true;

            // validation
            if (!Regex.IsMatch(Password, AppStrings.PasswordRegex))
            {
                DisplayValidationErrorMessage(AppStrings.PasswordFormat);
            }
            else if (!Password.Equals(Password2))
            {
                DisplayValidationErrorMessage(AppStrings.PasswordMatch);
            }

            // si tout est valide
            if (canUpdatePassword)
            {
                MessageBox.Show("Mot de passe mis à jour", AppStrings.AppName, MessageBoxButton.OK, MessageBoxImage.Information);

                // on crée le salt et le hash qui sera mis dans la BD
                CurrentUser.CreatePasswordHashSalt(Password);

                // update du user à la BD
                db.Users.Update(CurrentUser);
                db.SaveChanges();

                // redirection vers la View d'authentification
                MainWindowVM.CurrentView = new LoginViewModel(MainWindowVM);
            }
        }

        private void DisplayValidationErrorMessage(string message)
        {
            MessageBox.Show(message, AppStrings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            canUpdatePassword = false;
        }
    }
}
