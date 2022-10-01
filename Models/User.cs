using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models
{
    /// <summary>
    /// 會員資料
    /// </summary>
    public class User
    {

        /// <summary>
        /// 會員編號
        /// </summary>
        [Key]
        [Display(Name = "會員編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 信箱帳號
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]
        [Display(Name = "信箱帳號")]
        public string Account { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "密碼")]
        public string HashPassword { get; set; }

        /// <summary>
        /// 雜湊鹽
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "雜湊鹽")]
        public string Salt { get; set; }

        /// <summary>
        /// 頭貼圖檔名稱
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "頭貼圖檔名稱")]
        public string Image { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "名稱")]
        public string Name { get; set; }

        /// <summary>
        /// 帳號開通
        /// </summary>
        [Display(Name = "帳號開通")]
        public bool AccountState { get; set; }
        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime? CreatDate { get; set; } //問號是可為空值的意思

        /// <summary>
        /// 信箱驗證碼
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "信箱驗證碼")]
        public string CheckMailCode { get; set; }

        /// <summary>
        /// 信箱驗證碼到期時間
        /// </summary>
        [Display(Name = "信箱驗證碼到期時間")]
        public DateTime MailCodeCreatDate { get; set; }

        /// <summary>
        /// RefreshToken
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "RefreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// RefreshToken建立時間
        /// </summary>
        [Display(Name = "RefreshToken建立時間")]
        public DateTime? RefreshTokenCreatDate { get; set; }

        
        /// <summary>
        /// 收款方式
        /// </summary>
        public virtual ICollection<PaymentBank> PaymentBanks { get; set; }
        public virtual ICollection<PaymentCash> PaymentCashs { get; set; }
        public virtual ICollection<PaymentLine> PaymentLines { get; set; }

        /// <summary>
        /// 群組中擔任的成員
        /// </summary>
        public virtual ICollection<Member> Members { get; set; }

    }
}