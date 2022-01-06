using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Models
{
    public class BookUser
    {
        [Key, Column(Order = 1)]
        public int BookId { get; set; }

        [Key, Column(Order = 2)]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual Book Book { get; set; }

        public BookUser()
        {

        }
    }
}
