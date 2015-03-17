using System;

namespace Common
{
    public class Diginote : MarshalByRefObject
    {
        private static uint _counter;
        public uint Serial { get; set; }
        public int Value { get; set; }

        public Diginote()
        {
            Serial = _counter++;
            Value = 1;
        }

        public override int GetHashCode()
        {
            return Serial.GetHashCode();
        }

    }
}
