using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ManuRecyEco.Views
{
    /// <summary>
    /// Interaction logic for MainMenuView.xaml
    /// </summary>
    public partial class MainMenuView : UserControl
    {
        public MainMenuView()
        {
            InitializeComponent();

            ToggleButtonProfile.IsChecked = true;

            if (this.DataContext != null && 
                ((dynamic)this.DataContext).CurrentUser != null &&
                ((dynamic)this.DataContext).CurrentUser.UiStyle != 0)
            {
                ResourceDictionary Styles = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.StylePath, UriKind.Relative));
                
                Application.Current.Resources.MergedDictionaries.Clear();

                if (((dynamic)this.DataContext).CurrentUser.UiStyle == 1)
                {
                    ResourceDictionary theme = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.Theme1Path, UriKind.Relative));
                    Application.Current.Resources.MergedDictionaries.Add(theme);
                }
                else if (((dynamic)this.DataContext).CurrentUser.UiStyle == 2)
                {
                    ResourceDictionary theme = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.Theme2Path, UriKind.Relative));
                    Application.Current.Resources.MergedDictionaries.Add(theme);
                }
                else if (((dynamic)this.DataContext).CurrentUser.UiStyle == 3)
                {
                    ResourceDictionary theme = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.Theme3Path, UriKind.Relative));
                    Application.Current.Resources.MergedDictionaries.Add(theme);
                }

                Application.Current.Resources.MergedDictionaries.Add(Styles);
            }
        }

        private void DisableAllEnableOne(object sender)
        {
            ToggleButton btn = (ToggleButton)sender;
            if ((bool)btn.IsChecked)
            {
                DisableAll();
                btn.IsChecked = true;
            }
            else
            {
                DisableAll();
                btn.IsChecked = true;
            }
        }

        private void DisableAll()
        {
            ToggleButtonProfile.IsChecked = false;
            ToggleButtonBooks.IsChecked = false;
            ToggleButtonMessages.IsChecked = false;
            ToggleButtonSearch.IsChecked = false;
            ToggleButtonPublish.IsChecked = false;
            ToggleButtonOffers.IsChecked = false;
            ToggleButtonLogOut.IsChecked = false;
        }

        private void ToggleButtonProfile_Click(object sender, RoutedEventArgs e)
        {
            DisableAllEnableOne(sender);
        }

        private void ToggleButtonBooks_Click(object sender, RoutedEventArgs e)
        {
            DisableAllEnableOne(sender);
        }

        private void ToggleButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            DisableAllEnableOne(sender);
        }

        private void ToggleButtonPublish_Click(object sender, RoutedEventArgs e)
        {
            DisableAllEnableOne(sender);
        }

        private void ToggleButtonOffers_Click(object sender, RoutedEventArgs e)
        {
            DisableAllEnableOne(sender);
        }

        private void ToggleButtonMessages_Click(object sender, RoutedEventArgs e)
        {
            DisableAllEnableOne(sender);
        }

        private void ToggleButtonLogOut_Click(object sender, RoutedEventArgs e)
        {
            if (((dynamic)this.DataContext).CurrentUser.UiStyle != 0)
            {
                ResourceDictionary defaultTheme = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.DefaultThemePath, UriKind.Relative));
                ResourceDictionary Styles = (ResourceDictionary)Application.LoadComponent(new Uri(AppStrings.StylePath, UriKind.Relative));

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(defaultTheme);
                Application.Current.Resources.MergedDictionaries.Add(Styles);
            }

            DisableAllEnableOne(sender);
        }
    }
}
