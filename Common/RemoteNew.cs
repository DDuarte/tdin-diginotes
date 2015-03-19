using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;

namespace Common
{
    public static class RemoteNew
    {
        private static readonly Dictionary<Type, WellKnownClientTypeEntry> Types = InitTypeTable();

        private static Dictionary<Type, WellKnownClientTypeEntry> InitTypeTable()
        {
            return RemotingConfiguration.GetRegisteredWellKnownClientTypes()
                .ToDictionary(entry => entry.ObjectType, entry => entry);
        }

        public static T New<T>()
        {
            var entry = Types[typeof(T)];
            if (entry == null)
                throw new RemotingException("Type " + typeof(T) + " not found");
            return (T) RemotingServices.Connect(typeof(T), entry.ObjectUrl);
        }
    }
}
