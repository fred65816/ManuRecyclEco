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
    public class BookCopy
    {
        public int Id { get; set; }

        [Required]
        public int Condition { get; set; }

        [Required]
        public double Price { get; set; }

        public virtual ImageCopy ImageCopy { get; set; }

        [DefaultValue(null)]
        public int? ImageCopyId { get; set; }

        [Required, Column(TypeName = "varchar(30)")]
        public string TransactionType { get; set; }

        public virtual Book Book { get; set; }

        public virtual User User { get; set; }

        public int BookId { get; set; }

        public int UserId { get; set; }

        public BookCopy()
        {

        }
    }
}
