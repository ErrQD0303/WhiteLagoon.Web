﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon.Domain.Entities
{
    public class Villa
    {
        public int Id { get; set; } //The Id key will be made the new primary key of Villa
        [MaxLength(50)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Display(Name = "Price per night")]
        [Range(10, 10000)]
        public double Price { get; set; }
        public int Sqft { get; set; }
        public int Occupancy { get; set; }
        [Display(Name = "Image Url")]
        [NotMapped] //Tell the compiler not add this property to the database
        public IFormFile? Image { get; set; }
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
        public DateTime? Created_Date { get; set; }
        public DateTime? Updated_Date { get; set; }

        [ValidateNever]
        public IEnumerable<Amenity> VillaAmenity { get; set; }

        [NotMapped]
        public bool IsAvailable { get; set; } = true;
    }
}
