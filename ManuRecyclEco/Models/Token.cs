using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco.Models
{
    public class Token
    {
        public int Id { get; set; }

        public virtual User User { get; set; }

        public int UserId { get; set; }

        [Required, Column(TypeName = "varchar(16)"), StringLength(16)]
        public string RandomString { get; set; }

        [Required, Column(TypeName = "TEXT"), StringLength(26)]
        public string SentTime { get; set; }

        public Token()
        {

        }
    }
}
