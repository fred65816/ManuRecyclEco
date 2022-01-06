using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Models
{
    public class AcademicCourse
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "varchar(50)")]
        public string Name { get; set; }

        [Required, Column(TypeName = "varchar(80)")]
        public string Acronym { get; set; }

        public virtual ICollection<AcademicProgram> AcademicPrograms { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public virtual ICollection<Book> Books { get; set; }

        public AcademicCourse()
        {
            this.AcademicPrograms = new HashSet<AcademicProgram>();
            this.Users = new HashSet<User>();
            this.Books = new HashSet<Book>();
        }

        public override string ToString()
        {
            return Acronym + " " + Name;
        }
    }
}
