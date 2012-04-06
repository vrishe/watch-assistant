using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace watch_assistant.Model.Dictionary
{
    class Thesaurus : ISerializable, IDeserializationCallback
    {
        #region Fields
        private readonly Dictionary<string, HashSet<WeakReference>> _dictionary = new Dictionary<string, HashSet<WeakReference>>();
        private readonly HashSet<string> _freeDefinitions = new HashSet<string>();
        #endregion (Fields)

        #region Events
        public event EventHandler Deserialized;
        #endregion (Events)

        #region Methods
        public void AddDefinition(string key, IEnumerable<string> definition, bool mutuallyReversal)
        {

        }

        #region ISerializable methods
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
        #endregion (ISerializable methods)

        #region IDeserializationCallback methods
        protected virtual void OnDeserialization(object sender) 
        {
            if (Deserialized != null) Deserialized(sender, new EventArgs());
        }
        #endregion (IDeserializationCallback methods)
        #endregion (Methods)

        #region Properties
        public string Name { get; set; }
        #endregion (Properties)

        #region Constructors
        public Thesaurus()
        {
            Name = "Thesaurus";
        }
        #endregion (Constructors)
    }
}
