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
    /// 付款者
    /// </summary>
    public class Payer
    {
        /// <summary>
        /// 付款編號
        /// </summary>
        [Key]
        [Display(Name = "付款編號")]
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

        //[ForeignKey("MemberId")]
        //[JsonIgnore]
        //public virtual Member MemberIdFK { get; set; }

        /// <summary>
        /// 付款金額
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "付款金額")]
        public double PayAmount { get; set; }


    }
}