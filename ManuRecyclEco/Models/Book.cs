using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "varchar(100)")]
        public string Title { get; set; }

        [Required, Column(TypeName = "varchar(13)")]
        public string ISBN { get; set; }

        [Required, Column(TypeName = "varchar(100)")]
        public string Author { get; set; }

        [Required, Column(TypeName = "varchar(100)")]
        public string Publisher { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int NumPages { get; set; }

        [Required]
        public double ReferencePrice { get; set; }

        public virtual ImageCopy ImageCopy { get; set; }

        [DefaultValue(null), Column(TypeName = "integer")]
        public int? ImageCopyId { get; set; }

        public virtual ICollection<AcademicCourse> AcademicCourses { get; set; }

        public virtual ICollection<BookCopy> BookCopies { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public Book()
        {
            this.AcademicCourses = new HashSet<AcademicCourse>();
            this.BookCopies = new HashSet<BookCopy>();
            this.Users = new HashSet<User>();
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
