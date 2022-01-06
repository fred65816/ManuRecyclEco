using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Utility
{
    // pour les strings de messages utilisées plus d'une fois dans les ViewModels
    // ou pour les regex de validation utilisé plus d'une fois
    public static class AppStrings
    {
        // pour les MessageBox.Show()
        public const string AppName = "ManuRecyclEco";

        // 8 caractères minimum, au moins une majuscule, minuscule et un chiffre
        public const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,100}$";

        public const string InvalidLoginInfo = "Informations incorrectes. Veuillez réessayer.";

        public const string EmailMatch = "L'adresse courriel ne correspond à aucun compte ManuRecyEco.";

        public const string UsernameFormat = "Le nom d'utilisateur est invalide";

        public const string EmailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

        public const string EmailFormat = "Le format du courriel est invalide";

        public const string EmailExists = "Cette adresse courriel est liée à un compte existant";

        public const string PasswordFormat = "Le format du mot de passe est invalide. Le mot de passe doit contenir 8 caractères minimum, une majuscule, une minuscule et un chiffre";

        public const string PasswordMatch = "Les deux mots de passe ne sont pas identiques";

        public const string TokenNotSent = "Échec de l'envoie du courriel contenant le jeton aléatoire à l'adresse ";

        public const string HyperLinkAcheter = "Détails";

        public const string ProfilUpdated = "Modifications enregistrées avec succès";

        public const string RemoveOffer = "L'offre a été supprimé avec succès";

        public const string ProfilUpdatedEmail = "Modifications enregistrées, redirection sous peu..";

        public const string PictureRemoved = "La photo a été supprimé et celle par défaut a été restaurée";

        // pour l'affichage de livres
        public const string TransactionEchange = "Échange";
        public const string TransactionVente = "Vente";
        public const string TransactionVenteEchange = "Vente ou échange";
        public const string PrixLivre = "Prix: ";
        public const string ConditionLivreA = "Condition: ";
        public const string ConditionLivreB = "/10";
        public const string NonAppl = "N/A";

        // pour les opérateurs dans Recherche
        public const string SmallerThan = "Plus petit que";
        public const string GreaterThan = "Plus grand que";
        public const string EqualTo = "Égal à";
        public const string NoOperator = "Aucun choix";

        // styles de couleurs
        public const string StylePath = @"ManuRecyEco;component/Styles.xaml";
        public const string DefaultThemePath = @"ManuRecyEco;component/DefaultTheme.xaml";
        public const string Theme1Path = @"ManuRecyEco;component/Theme1.xaml";
        public const string Theme2Path = @"ManuRecyEco;component/Theme2.xaml";
        public const string Theme3Path = @"ManuRecyEco;component/Theme3.xaml";
    }
}
