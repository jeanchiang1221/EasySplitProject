using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models
{
    public class Group
    {
        /// <summary>
        /// 群組編號
        /// </summary>
        [Key]
        [Display(Name = "群組編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "群組名稱")]
        public string Name { get; set; }

        /// <summary>
        /// 群組guid
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "群組guid")]
        public string GroupGuid { get; set; }

        /// <summary>
        /// 群組圖檔名稱
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "群組圖檔名稱")]
        public string Image { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime? CreatDate { get; set; } //問號是可為空值的意思


        /// <summary>
        /// 創立者的使用者編號
        /// </summary>
        [Display(Name = "創立者的使用者帳號")]
        public string CreatorAccount { get; set; }


        /// <summary>
        /// 群組是否已被移除
        /// </summary>
        public bool Removed { get; set; }


        /// <summary>
        /// 群組成員
        /// </summary>
        public virtual ICollection<Member> Members { get; set; }

    }
}