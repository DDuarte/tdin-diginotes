using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public class EventProxy : MarshalByRefObject
    {
        #region Event Declarations

        public event MessageArrivedEvent MessageArrived;

        #endregion

        #region Lifetime Services

        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region Local Handlers

        public void LocallyHandleMessageArrived(Update update)
        {
            if (MessageArrived != null)
                MessageArrived(update);
        }

        #endregion
    }
}
