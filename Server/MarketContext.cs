using System.Collections.Generic;
using System.Data.Entity;
using Common.Models;

namespace Server
{
    public class MarketContext : DbContext
    {
        public DbSet<Diginote> Diginotes { get; set; }
        public DbSet<Market> Markets { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<SalesOrder> SalesOrder { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }

        public MarketContext() : base()
        {
            Database.SetInitializer(new DbInitializer());
        }
    }

    class DbInitializer : CreateDatabaseIfNotExists<MarketContext>
    {
        protected override void Seed(MarketContext context)
        {
            context.Markets.Add(new Market(1, 1));

            var diginotes1 = Generate<Diginote>(10);
            var diginotes2 = Generate<Diginote>(10);
            context.Diginotes.AddRange(diginotes1);
            context.Diginotes.AddRange(diginotes2);

            context.Users.Add(new User("TestUser1", "user1", "user1") { Diginotes = diginotes1 });
            context.Users.Add(new User("TestUser2", "user2", "user2") { Diginotes = diginotes2 });

            base.Seed(context);
        }

        private List<T> Generate<T>(int count) where T : new()
        {
            var list = new List<T>();
            for (var i = 0; i < count; ++i)
                list.Add(new T());
            return list;
        }
    }
}
