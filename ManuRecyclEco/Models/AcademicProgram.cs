using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Models
{
    public class AcademicProgram
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "varchar(80)")]
        public string Name { get; set; }

        public virtual School School { get; set; }

        public int SchoolId { get; set; }

        public virtual ICollection<AcademicCourse> AcademicCourses { get; set; }

        public virtual ICollection<User> ProgramUsers { get; set; }

        public AcademicProgram()
        {
            this.AcademicCourses = new HashSet<AcademicCourse>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
