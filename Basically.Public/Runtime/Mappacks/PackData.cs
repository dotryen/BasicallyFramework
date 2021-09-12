using System;

namespace Basically.Mapping {
    [Serializable]
    public struct PackData {
        internal string name;
        internal string description;
        internal string author;
        internal string[] maps;

        public string Name => name;
        public string Description => description;
        public string Author => author;
        internal string[] Maps => maps;
    }
}
