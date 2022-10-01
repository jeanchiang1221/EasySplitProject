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
    /// 群組成員
    /// </summary>
    public class Member
    {

        /// <summary>
        /// 會員編號
        /// </summary>
        [Key]
        [Display(Name = "會員編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "名稱")]
        public string Name { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime? CreatDate { get; set; } //問號是可為空值的意思


        /// <summary>
        /// 所屬群組編號
        /// </summary>
        [Display(Name = "屬群組編號")]
        public int GroupId { get; set; }

        [ForeignKey("GroupId")]
     // [JsonIgnore]
        public virtual Group GroupIdFK { get; set; }

        /// <summary>
        /// 所屬會員編號
        /// </summary>
        [Display(Name = "所屬會員編號")]
        public int ? UserId { get; set; }

        [ForeignKey("UserId")]
        //[JsonIgnore]
        public virtual User UserIdFK { get; set; }

        /// <summary>
        /// 是否已被移除群組
        /// </summary>
        public bool RemovedFromGroup { get; set; }

    }
}