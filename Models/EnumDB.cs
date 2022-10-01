using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasySplitProject.Models
{
    public class EnumDB
    {
        /// <summary>
        /// 費用種類
        /// </summary>
        public enum ExpenseType
        {
            餐飲=0,
            住宿=1,
            交通=2,
            購物=3,
            生活用品=4,
            娛樂社交=5,
            健康醫療=6,
            其他=7
        }

        
        
        
        /// <summary>
        /// 付款方式
        /// </summary>
        public enum PaymentMethodList
        {
            銀行轉帳=0,
            現金面交=1,
            LinePay=2,
        }
    }
}