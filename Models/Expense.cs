using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using static EasySplitProject.Models.EnumDB;

namespace EasySplitProject.Models
{
    /// <summary>
    /// 費用
    /// </summary>
    public class Expense
    {

        /// <summary>
        /// 費用編號
        /// </summary>
        [Key]
        [Display(Name = " 費用編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 所屬群組編號
        /// </summary>
        [Display(Name = "屬群組編號")]
        public int GroupId { get; set; }

        [ForeignKey("GroupId")]
        // [JsonIgnore]
        public virtual Group GroupIdFK { get; set; }


        /// <summary>
        /// 費用種類
        /// </summary>
        public ExpenseType ExpenseType { get; set; }


        /// <summary>
        /// 品項
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]
        [Display(Name = "品項")]
        public string Item { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "金額")]
        public double Cost { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime ?CreatDate { get; set; }

        /// <summary>
        /// 註記
        /// </summary>

        [MaxLength(50)]
        [Display(Name = "註記")]
        public string Memo { get; set; }


          

        public virtual ICollection<Owner> Owners { get; set; }

        public virtual ICollection<Payer> Payers { get; set; }

        public virtual ICollection<ExpenseAlbum> ExpenseAlbums { get; set; }


    }
}