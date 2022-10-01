using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace EasySplitProject.Models
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("name=ApplicationDbContext")
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<PaymentBank> PaymentBanks { get; set; }
        public virtual DbSet<PaymentCash> PaymentCashs { get; set; }
        public virtual DbSet<PaymentLine> PaymentLines { get; set; }

        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Member> Members { get; set; }

        public virtual DbSet<Expense> Expenses { get; set; }

        public virtual DbSet<Payer> Payers { get; set; }

        public virtual DbSet<Owner> Owners { get; set; }

        public virtual DbSet<ExpenseAlbum> ExpenseAlbums { get; set; }

        public virtual DbSet<Settlement> Settlements { get; set; }

        public virtual DbSet<Notification> Notifications { get; set; }
    }
}
