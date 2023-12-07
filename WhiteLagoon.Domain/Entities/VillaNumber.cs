using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon.Domain.Entities
{
    public class VillaNumber
    {
        //DatabaseGeneratedOption.None mean it will not populate the Key column with
        //increased Identity number
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Villa Number")]
        public int Villa_Number { get; set; }

        [ForeignKey("Villa")]
        public int VillaId { get; set; }
        [ValidateNever]
        public Villa Villa { get; set; } //Don't want to be validated by ModelState
        //So exclude it
        public string? SpecialDetails { get; set; }
    }
}
