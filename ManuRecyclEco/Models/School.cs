using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Models
{
    public class School
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "varchar(60)")]
        public string Name { get; set; }

        [Required, Column(TypeName = "varchar(100)")]
        public string Address { get; set; }

        public virtual SchoolLevel SchoolLevel { get; set; }

        public int SchoolLevelId { get; set; }

        public virtual ICollection<AcademicProgram> AcademicPrograms { get; set; }

        public School()
        {
            this.AcademicPrograms = new HashSet<AcademicProgram>();
        }
    }
}
