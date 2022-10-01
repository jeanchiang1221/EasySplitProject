using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasySplitProject.Models.ViewModels
{
    public class MemberEditVM
    {
        /// <summary>
        /// 成員編號
        /// </summary>
  
        public int MemberId { get; set; }


        /// <summary>
        /// 修改後成員名稱
        /// </summary>
        public string EditedName { get; set; }
    }
}