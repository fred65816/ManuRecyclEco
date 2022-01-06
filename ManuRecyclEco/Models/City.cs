using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Models
{
    public class City
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "varchar(50)")]
        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public City()
        {
            this.Users = new HashSet<User>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
