using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class Category
    {
        [Key] //khóa chính
        public int Id { get; set; }
        [Required] // cột này sẽ not null
        [MaxLength(30)]
        [DisplayName("Category Name")]
        public string? Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "tên thay thế phải nằm trong khoảng 1-100")]
        public int DisplayOrder { get; set; }
    }
}
