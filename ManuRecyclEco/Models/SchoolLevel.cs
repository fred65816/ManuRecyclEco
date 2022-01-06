using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Models
{
    public class SchoolLevel
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "varchar(20)")]
        public string Name { get; set; }

        public virtual ICollection<School> Schools { get; set; }

        public SchoolLevel()
        {
            this.Schools = new HashSet<School>();
        }
    }
}
