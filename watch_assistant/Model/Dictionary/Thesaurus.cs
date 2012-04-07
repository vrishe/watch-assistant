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
        private static List<WeakReference> _instances;
        private readonly Dictionary<string, HashSet<string>> _dictionary = new Dictionary<string, HashSet<string>>();
        private string _name;
        #endregion (Fields)

        #region Properties
        public string Name
        {
            get { return _name; }
            set
            {
                if (String.IsNullOrEmpty(value)) value = String.Format("Thesaurus{0}", _instances.Count);
                _name = value;
            }
        }
        #endregion (Properties)

        #region Events
        public event EventHandler Deserialized;
        #endregion (Events)

        #region Methods
        /// <summary>
        /// Appens another description group to an existent definition of the key.
        /// </summary>
        /// <param name="key">Dictionary key that may have a definition</param>
        /// <param name="definition">Dictionary definition that opens sense of the key</param>
        /// <param name="mutual">Tells if it has to append the 'key' as a meaning with each of the corresponding 'meaning' values as keys within the thesaurus</param>
        public void AddDefinition(string key, IEnumerable<string> definition, bool mutual)
        {
            lock (this)
            {
                if (!_dictionary.ContainsKey(key)) _dictionary.Add(key, new HashSet<string>());
                _dictionary[key].UnionWith(definition);
                if (mutual)
                {
                    foreach (string meaning in definition)
                    {
                        HashSet<string> extensibleDefinition;
                        if (!_dictionary.TryGetValue(meaning, out extensibleDefinition)) _dictionary.Add(meaning, (extensibleDefinition = new HashSet<string>()));
                        extensibleDefinition.Add(key);
                    }
                }
            }
        }
        /// <summary>
        /// Appens another description to an existent definition of the key.
        /// </summary>
        /// <param name="key">Dictionary key that may have a definition</param>
        /// <param name="meaning">Dictionary meaning that opens sense of the key</param>
        /// <param name="mutual">Tells if it has to append the 'key' as a meaning to corresponding 'meaning' key mutually</param>
        public void AddDefinition(string key, string meaning, bool mutual)
        {
            AddDefinition(key, new string[] { meaning }, mutual);
        }
        /// <summary>
        /// Overrides currently existent key definition.
        /// </summary>
        /// <param name="key">Dictionary key that may have a definition</param>
        /// <param name="meaning">Dictionary meaning that opens sense of the key</param>
        /// <param name="mutual">Tells if it has to append the 'key' as a meaning with each of the corresponding 'meaning' values as keys within the thesaurus</param>
        public void SetDefinition(string key, IEnumerable<string> definition, bool mutual)
        {
            lock (this)
            {
                _dictionary.Remove(key);
                AddDefinition(key, definition, mutual);
            }
        }
        /// <summary>
        /// Removes the definition of a key specified
        /// </summary>
        /// <param name="key">Dictionary key whose definition is about to remove</param>
        /// <param name="mutual">Tells if it has to remove each of 'meaning' values as a keys within the current Thesaurus</param>
        /// <returns>True - if key removal was successfull; False - otherwise</returns>
        public bool RemoveDefinition(string key, string meaning, bool mutual)
        {
            lock (this)
            {
                HashSet<string> currentDefinition;
                if (_dictionary.TryGetValue(key, out currentDefinition))
                {
                    if (currentDefinition.Remove(meaning))
                    {
                        if (currentDefinition.Count == 0) _dictionary.Remove(key);
                        RemoveDefinition(meaning, false);
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// Removes the definition of a key specified
        /// </summary>
        /// <param name="key">Dictionary key whose definition is about to remove</param>
        /// <param name="mutual">Tells if it has to remove each of 'meaning' values as a keys within the current Thesaurus</param>
        /// <returns>True - if key removal was successfull; False - otherwise</returns>
        public bool RemoveDefinition(string key, bool mutual)
        {
            lock (this)
            {
                HashSet<string> currentDefinition;
                if (mutual && _dictionary.TryGetValue(key, out currentDefinition))
                {
                    foreach (string meaning in currentDefinition)
                    {
                        RemoveDefinition(meaning, key, false);
                    }
                }
                return _dictionary.Remove(key);
            }
        }


        #region ISerializable methods
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) 
        { 
            // Some serialization logic. Coming soon. April 2012th...
        }
        #endregion (ISerializable methods)

        #region IDeserializationCallback methods
        public virtual void OnDeserialization(object sender) 
        {
            if (Deserialized != null) Deserialized(sender, new EventArgs());
        }
        #endregion (IDeserializationCallback methods)
        #endregion (Methods)

        #region Constructors
        public Thesaurus()
        {
            if ( _instances == null ) _instances = new List<WeakReference>();
            _instances.Add(new WeakReference(this));
            Name = null;
        }
        #endregion (Constructors)
    }
}
