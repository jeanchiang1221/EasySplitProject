using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static EasySplitProject.Models.EnumDB;

namespace EasySplitProject.Models.ViewModels
{
    public class ExpenseVM
    {
        /// <summary>
        /// 費用編號
        /// </summary>

        public int Id { get; set; }


        /// <summary>
        /// 所屬群組編號
        /// </summary>
        [Display(Name = "屬群組編號")]
        public int GroupId { get; set; }


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
        /// 付款者的memberId
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "付款者的資訊")]

        public PayerExpenseVM[] PayerExpenseVMs { get; set; }

        /// <summary>
        /// 分款者的memberId
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "分款者的資訊")]

        public OwnerExpenseVM[] OwnerExpenseVMs { get; set; }

        public ExpenseAlbumVM[] ExpenseAlbumVMs { get; set; }

        /// <summary>
        /// 建立時間(若沒有填，會預設為當下)
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatDate { get; set; }

        /// <summary>
        /// 註記
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]
        [Display(Name = "註記")]
        public string Memo { get; set; }
    }


    public class PayerExpenseVM
    {
        public int MemberId { get; set; }
        public double PayAmount { get; set; }

    }

    public class OwnerExpenseVM
    {
        public int MemberId { get; set; }
        public double OwnAmount { get; set; }

    }


    public class ExpenseAlbumVM
    {

        /// <summary>
        /// 相片名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 所屬費用編號
        /// </summary>
        public int ExpenseId { get; set; }

    }

}