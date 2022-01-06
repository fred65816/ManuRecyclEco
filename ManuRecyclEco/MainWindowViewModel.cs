using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using ManuRecyEco.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ManuRecyEco
{
    public class MainWindowViewModel: ObservableObject
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { OnPropertyChanged(ref _currentView, value); }
        }

        private MainMenuViewModel _mainMenuVM;
        public MainMenuViewModel MainMenuVM
        {
            get { return _mainMenuVM; }
            set { OnPropertyChanged(ref _mainMenuVM, value); }
        }

        private LoginViewModel _loginVM;
        public LoginViewModel LoginVM
        {
            get { return _loginVM; }
            set { OnPropertyChanged(ref _loginVM, value); }
        }

        public MainWindowViewModel()
        {
            CurrentView = new LoginViewModel(this);
        }
    }
}
