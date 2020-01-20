using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraniteHouse.Models.ViewModel
{
    public class ProductsViewModel
    {
        public Products Products { get; set; }
        [Display(Name = "Product Types")]
        public IEnumerable<ProductTypes> ProductTypes { get; set; }
        [Display(Name = "Special Tags")]
        public IEnumerable<SpecialTags> SpecialTags { get; set; }
    }
}
