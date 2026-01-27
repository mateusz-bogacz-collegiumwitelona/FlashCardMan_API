using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Request
{
    public class AddNewDeckRequest
    {
        [Required]
        [StringLength(200, ErrorMessage = "Name must be beetwen 4 and 200 characters", MinimumLength = 5)]
        public string Name { get; set; }


        public string? Description { get; set; }
    }
}
