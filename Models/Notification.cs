using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySplitProject.Models
{
    public class Notification
    {
        /// <summary>
        /// 通知編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 發起動作人的UserId 
        /// </summary>
        public int ? UserId { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatDate { get; set; }

        /// <summary>
        /// 通知內容
        /// </summary>
        public string Information { get; set; }


    }
}