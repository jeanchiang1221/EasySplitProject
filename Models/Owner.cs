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
    /// 分款者
    /// </summary>
    public class Owner
    {
        /// <summary>
        /// 分款編號
        /// </summary>
        [Key]
        [Display(Name = "分款編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 所屬費用編號
        /// </summary>
        [Display(Name = "所屬費用編號")]
        public int ExpenseId { get; set; }

        [ForeignKey("ExpenseId")]
        [JsonIgnore]
        public virtual Expense ExpenseIdFK { get; set; }


        /// <summary>
        /// 所屬成員編號
        /// </summary>
        [Display(Name = "所屬成員編號")]
        public int MemberId { get; set; }


        /// <summary>
        /// 欠款金額
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "欠款金額")]
        public double OwnAmount { get; set; }



    }
}