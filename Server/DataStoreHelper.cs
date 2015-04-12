using Common;
using OpenNETCF.ORM;

namespace Server
{
    public static class DataStoreHelper
    {
        private const string DBFilename = "store.sdf";

        public static IDataStore GetDataStore()
        {
            IDataStore store = new SqlCeDataStore(DBFilename);

            store.AddType<User>();
            store.AddType<Diginote>();
            store.AddType<PurchaseOrder>();
            store.AddType<SalesOrder>();
            store.AddType<DigiMarket>();

            if (!store.StoreExists)
                store.CreateStore();

            return store;
        }
    }
}
