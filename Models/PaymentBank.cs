using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models
{
    public class PaymentBank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 所屬使用者編號
        /// </summary>
        [Display(Name = "所屬使用者編號")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User UserIdFK { get; set; }

        /// <summary>
        /// 戶名
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "戶名")]
        public string AccountName { get; set; }

        /// <summary>
        /// 銀行代碼
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "銀行代碼")]
        public string BankCode { get; set; }

        /// <summary>
        /// 銀行名稱
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "銀行名稱")]
        public string Bank { get; set; }


        /// <summary>
        /// 銀行帳號
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "銀行帳號")]
        public string Account { get; set; }

    }
}