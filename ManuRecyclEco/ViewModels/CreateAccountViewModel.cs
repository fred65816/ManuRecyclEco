using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ManuRecyEco.ViewModels
{
    public class CreateAccountViewModel: ObservableObject
    {
        private ApplicationDbContext db;

        private User CurrentUser;

        private MainWindowViewModel _mainWindowVM;
        public MainWindowViewModel MainWindowVM
        {
            get { return _mainWindowVM; }
            set { OnPropertyChanged(ref _mainWindowVM, value); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { OnPropertyChanged(ref _username, value); }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { OnPropertyChanged(ref _email, value); }
        }

        // prénom
        private String _firstName;
        public String FirstName
        {
            get { return _firstName; }
            set { OnPropertyChanged(ref _firstName, value); }
        }

        // nom
        private String _lastName;
        public String LastName
        {
            get { return _lastName; }
            set { OnPropertyChanged(ref _lastName, value); }
        }

        // ville choisie dans la liste déroulante
        private string _selectedCity;
        public string SelectedCity
        {
            get { return _selectedCity; }
            set { OnPropertyChanged(ref _selectedCity, value); }
        }

        // true si le bouton "Créer compte" est activé
        private bool _buttonCreateEnabled;
        public bool ButtonCreateEnabled
        {
            get { return _buttonCreateEnabled; }
            set { OnPropertyChanged(ref _buttonCreateEnabled, value); }
        }

        // liste des villes de la liste déroulante
        private List<string> _cityList;
        public List<string> CityList
        {
            get { return _cityList; }
            set { OnPropertyChanged(ref _cityList, value); }
        }

        // les deux champs password
        // ne peuvent pas être bindés dans un PasswordBox
        public string Password;
        public string Password2;

        // commande du bouton "créer le compte"
        public ICommand CreateAccountCommand { get; private set; }

        // Commande du bouton "retour"
        public ICommand ExitCommand { get; private set; }

        // booléen pour la validation
        private bool canCreateAccount;

        private string Token;

        public CreateAccountViewModel(MainWindowViewModel MainWindowVM)
        {
            db = new ApplicationDbContext();

            CurrentUser = null;

            this.MainWindowVM = MainWindowVM;

            Username = String.Empty;
            Email = String.Empty;
            Password = String.Empty;
            Password2 = String.Empty;
            FirstName = String.Empty;
            LastName = String.Empty;
            Token = string.Empty;

            // commande du bouton "créer le compte"
            CreateAccountCommand = new RelayCommand(CreateAccount);

            // Commande du bouton "retour"
            ExitCommand = new RelayCommand(Exit);

            // liste des villes
            CityList = db.cityNames;

            if (CityList.Count > 0){
                // ville par défaut dans le ComboBox
                List<string> list = CityList.Where(x => x.Equals("Montréal")).ToList();
                SelectedCity = list.Count > 0 ? list[0] : CityList[0];
            }

            // on active le bouton "Créer compte"
            ButtonCreateEnabled = true;
        }

        // bouton "retour"
        private void Exit()
        {
            // on set la vue de la fenêtre à une nouvelle vue de login
            MainWindowVM.CurrentView = new LoginViewModel(MainWindowVM);
        }

        // bouton "créer le compte"
        private void CreateAccount()
        {
            canCreateAccount = true;

            // enlever les espaces au début/fin des strings
            Username = Username.Trim();
            Email = Email.Trim();
            FirstName = FirstName.Trim();
            LastName = LastName.Trim();

            if (Username.Length < 2)
            {
                DisplayValidationErrorMessage(AppStrings.UsernameFormat);
            }
            else if (db.Users.Where(x => x.Username == Username).Any())
            {
                DisplayValidationErrorMessage("Ce nom d'utilisateur est lié à un compte existant");
            }
            else if (!Regex.IsMatch(Email, AppStrings.EmailRegex, RegexOptions.IgnoreCase))
            {
                DisplayValidationErrorMessage(AppStrings.EmailFormat);
            }
            else if (db.Users.Where(x => x.Email == Email).Any())
            {
                DisplayValidationErrorMessage(AppStrings.EmailExists);
            }
            else if (!Regex.IsMatch(Password, AppStrings.PasswordRegex))
            {
                DisplayValidationErrorMessage(AppStrings.PasswordFormat);
            }
            else if (!Password.Equals(Password2))
            {
                DisplayValidationErrorMessage(AppStrings.PasswordMatch);
            }

            // si tout est valide
            if (canCreateAccount)
            {
                // on désactive le bouton "Créer compte"
                ButtonCreateEnabled = false;

                // si on a des string vides on met null dans la BD
                FirstName = FirstName.Equals(String.Empty) ? null : FirstName;
                LastName = LastName.Equals(String.Empty) ? null : LastName;

                City city = db.Cities.Where(x => x.Name == SelectedCity).Single();

                // user inactif
                CurrentUser = new User(Username, Email, FirstName, LastName, city, null, false);

                // on crée le salt et le hash qui sera mis dans la BD
                CurrentUser.CreatePasswordHashSalt(Password);

                // ajout du user à la BD
                db.Users.Add(CurrentUser);
                db.SaveChanges();

                CurrentUser = db.Users.Find(CurrentUser.Id);

                // jeton aléatoire de 16 caractères
                Token = EmailUtil.GenerateToken();

                // si le courriel est envoyé
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
            }

            // redirection vers la View où on entre le token
            MainWindowVM.CurrentView = new TokenConfirmationViewModel(MainWindowVM, CurrentUser.Id, false, false);
        }

        private void DisplayValidationErrorMessage(string message)
        {
            MessageBox.Show(message, AppStrings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            canCreateAccount = false;
        }
    }
}
