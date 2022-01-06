using ManuRecyEco.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Models
{
    public class User: ObservableObject
    {
        public int Id { get; set; }

        public virtual ICollection<Token> Tokens { get; set; }

        public virtual ICollection<AcademicCourse> AcademicCourses { get; set; }

        public virtual ICollection<BookCopy> BookCopies { get; set; }

        public virtual ICollection<Book> Books { get; set; }

        public int CityId { get; set; }

        public virtual City City { get; set; }

        [DefaultValue(null), Column(TypeName = "integer")]
        public int? AcademicProgramId { get; set; }

        public virtual AcademicProgram AcademicProgram { get; set; }

        private string _username;
        [Required, Column(TypeName = "varchar(20) UNIQUE")]
        public string Username
        {
            get { return _username; }
            set { OnPropertyChanged(ref _username, value); }
        }

        private string _email;
        [Required, Column(TypeName = "varchar(70) UNIQUE")]
        public string Email
        {
            get { return _email; }
            set { OnPropertyChanged(ref _email, value); }
        }

        private string _passwordSalt;
        [Required, Column(TypeName = "varchar(64)"), StringLength(64)]
        public string PasswordSalt
        {
            get { return _passwordSalt; }
            set { OnPropertyChanged(ref _passwordSalt, value); }
        }

        private string _passwordHash;
        [Required, Column(TypeName = "varchar(256)"), StringLength(256)]
        public string PasswordHash
        {
            get { return _passwordHash; }
            set { OnPropertyChanged(ref _passwordHash, value); }
        }

        private String _firstName;
        [Column(TypeName = "varchar(50)")]
        public String FirstName
        {
            get { return _firstName; }
            set { OnPropertyChanged(ref _firstName, value); }
        }

        private String _lastName;
        [Column(TypeName = "varchar(50)")]
        public String LastName
        {
            get { return _lastName; }
            set { OnPropertyChanged(ref _lastName, value); }
        }

        private int _uiStyle;
        [DefaultValue(0), Column(TypeName = "integer")]
        public int UiStyle
        {
            get { return _uiStyle; }
            set { OnPropertyChanged(ref _uiStyle, value); }
        }

        private bool _isActive;
        [Required, Range(0, 1)]
        public bool IsActive
        {
            get { return _isActive; }
            set { OnPropertyChanged(ref _isActive, value); }
        }

        public virtual ImageCopy ImageCopy { get; set; }

        [DefaultValue(null), Column(TypeName = "integer")]
        public int? ImageCopyId { get; set; }

        private String _lastLogin;
        [DefaultValue(null), Column(TypeName = "TEXT")]
        public String LastLogin
        {
            get
            {
                return _lastLogin == null ? String.Empty :
                ", dernière connexion le " +
                DateTime.ParseExact(_lastLogin, "s", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd à HH:mm:ss");
            }
            set { OnPropertyChanged(ref _lastLogin, value); }
        }

        public User()
        {
            this.Tokens = new HashSet<Token>();
            this.AcademicCourses = new HashSet<AcademicCourse>();
            this.BookCopies = new HashSet<BookCopy>();
            this.Books = new HashSet<Book>();
        }

        public User(string Username, string Email, String FirstName, String LastName, City City, int? AcademicProgramId, bool IsActive)
        {
            this.Tokens = new HashSet<Token>();
            this.AcademicCourses = new HashSet<AcademicCourse>();
            this.BookCopies = new HashSet<BookCopy>();
            this.Books = new HashSet<Book>();
            this.Username = Username;
            this.Email = Email;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.City = City;
            this.AcademicProgramId = AcademicProgramId;
            this.IsActive = IsActive;
        }

        public void CreatePasswordHashSalt(string password)
        {
            byte[] saltBytes = GenerateSalt(64);
            PasswordSalt = Convert.ToBase64String(saltBytes);
            PasswordHash = GenerateHash(saltBytes, password);
        }

        // à utiliser dans le login
        public bool PasswordMatch(string password)
        {
            byte[] saltBytes = Convert.FromBase64String(PasswordSalt);
            string hash = GenerateHash(saltBytes, password);
            return hash == PasswordHash;
        }

        // génère un salt
        private byte[] GenerateSalt(int size)
        {
            byte[] saltBytes = new byte[size];
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            provider.GetNonZeroBytes(saltBytes);
            return saltBytes;
        }

        // génère le hashed password
        private string GenerateHash(byte[] saltBytes, string password)
        {
            Rfc2898DeriveBytes hashBytes = new Rfc2898DeriveBytes(password, saltBytes, 10000);
            return Convert.ToBase64String(hashBytes.GetBytes(256));
        }

        public override string ToString()
        {
            string displayName = Username;
            displayName += FirstName == string.Empty && LastName == string.Empty ?
                string.Empty : FirstName == string.Empty ? " (" + LastName + ")" :
                LastName == string.Empty ? " (" + FirstName + ")" :
                " (" + FirstName[0] + "." + LastName + ")";

            return displayName;
        }
    }
}
