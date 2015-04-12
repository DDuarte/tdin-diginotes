using Common;
using OpenNETCF.ORM;

namespace Server
{
    public class DataStoreHelper
    {
        private const string DBFilename = "store.sdf";

        public static IDataStore GetDataStore()
        {
            IDataStore store = new SqlCeDataStore(DBFilename);

            store.AddType<Diginote>();
            store.AddType<PurchaseOrder>();
            store.AddType<SalesOrder>();
            store.AddType<User>();

            if (!store.StoreExists)
                store.CreateStore();

            return store;
        }
    }
}
