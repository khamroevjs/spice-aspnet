using System.Collections.Generic;

namespace Spice.Models.ViewModels
{
    public class SubCategoryAndCategoryViewModel
    {
        public IEnumerable<Category> CategoryList { get; set; }
        public SubCategory SubCategory { get; set; }
        /// <summary>
        /// To show existing SubCategories in selected category
        /// </summary>
        public List<string> SubCategoryList { get; set; }
        public string StatusMessage { get; set; }
    }
}
