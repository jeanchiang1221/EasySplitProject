using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace EasySplitProject.Models
{
    /// <summary>
    /// 費用的相片集
    /// </summary>
    public class ExpenseAlbum
    {
        /// <summary>
        /// 相片編號
        /// </summary>
        [Key]
        [Display(Name = "相片編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "相片名稱")]
        public string Name { get; set; }

        /// <summary>
        /// 所屬費用編號
        /// </summary>
        [Display(Name = "所屬費用編號")]
        public int ExpenseId { get; set; }

        [ForeignKey("ExpenseId")]
        [JsonIgnore]
        public virtual Expense ExpenseIdFK { get; set; }

    }
}