using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EasySplitProject.Models
{
    public class PaymentLine
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
        /// 姓名
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "姓名")]
        public string Name { get; set; }

        /// <summary>
        /// LineID
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "LineID")]
        public string LineID { get; set; }

        /// <summary>
        /// 手機
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "手機")]
        [Phone]
        public string Phone { get; set; }

        /// <summary>
        /// QRCode
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "QRCode")]
        public string QRCode { get; set; }
    }
}