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
        private readonly Dictionary<string, HashSet<string>> _dictionary = new Dictionary<string, HashSet<string>>();
        #endregion (Fields)

        #region Events
        public event EventHandler Deserialized;
        #endregion (Events)

        #region Methods
        /// <summary>
        /// Appens another description to an existent definition of the key.
        /// </summary>
        /// <param name="key">Dictionary key that may have a definition</param>
        /// <param name="meaning">Dictionary meaning that opens sense of the key</param>
        /// <param name="mutual">Tells if it has to append the 'key' as a meaning to corresponding 'meaning' key mutually</param>
        /// <returns>True - if task was successfull; False - othervise</returns>
        public bool AddDefinition(string key, string meaning, bool mutual)
        {
            bool result;
            if (result = _dictionary.ContainsKey(key) && _dictionary[key].Add(meaning))
            {
                if (mutual) result &= AddDefinition(meaning, key, false);
            }
            return result;
        }
        /// <summary>
        /// Appens another description group to an existent definition of the key.
        /// </summary>
        /// <param name="key">Dictionary key that may have a definition</param>
        /// <param name="definition">Dictionary definition that opens sense of the key</param>
        /// <param name="mutual">Tells if it has to append the 'key' as a meaning with each of the corresponding 'meaning' values as keys within the thesaurus</param>
        /// <returns>True - if task was successfull; False - othervise</returns>
        public void AddDefinition(string key, IEnumerable<string> definition, bool mutual)
        {
            if (!_dictionary.ContainsKey(key)) _dictionary.Add(key, new HashSet<string>());
            
            _dictionary[key].Concat(definition);
            if (mutual)
            {
                foreach (string meaning in definition)
                {
                    AddDefinition(meaning, key, false);
                }
            }
        }
        /// <summary>
        /// Overrides currently existent key definition.
        /// </summary>
        /// <param name="key">Dictionary key that may have a definition</param>
        /// <param name="meaning">Dictionary meaning that opens sense of the key</param>
        /// <param name="mutual">Tells if it has to append the 'key' as a meaning with each of the corresponding 'meaning' values as keys within the thesaurus</param>
        /// <returns></returns>
        public void SetDefinition(string key, IEnumerable<string> definition, bool mutual)
        {
            _dictionary.Remove(key);
            AddDefinition(key, definition, mutual);
        }
        /// <summary>
        /// Removes the definition of a key specified
        /// </summary>
        /// <param name="key">Dictionary key whose definition is about to remove</param>
        /// <param name="mutual">Tells if it has to remove each of 'meaning' values as a keys within the current Thesaurus</param>
        /// <returns>True - if key removal was successfull; False = otherwise</returns>
        public bool RemoveDefinition(string key, string meaning, bool mutual)
        {
            bool result;
            if (result = _dictionary.ContainsKey(key) && _dictionary[key].Remove(meaning))
            {
                if (_dictionary[key].Count == 0) RemoveDefinition(key, mutual);
                if (mutual) RemoveDefinition(meaning, mutual);
            }
            return result;
        }
        /// <summary>
        /// Removes the definition of a key specified
        /// </summary>
        /// <param name="key">Dictionary key whose definition is about to remove</param>
        /// <param name="mutual">Tells if it has to remove each of 'meaning' values as a keys within the current Thesaurus</param>
        /// <returns>True - if key removal was successfull; False = otherwise</returns>
        public bool RemoveDefinition(string key, bool mutual)
        {
            if (_dictionary.ContainsKey(key))
            {
                if (mutual)
                {
                    foreach (string meaning in _dictionary[key])
                    {
                        RemoveDefinition(meaning, key, false);
                    }
                }
                return _dictionary.Remove(key);
            }
            return false;
            //if (mutual)
            //{
            //    HashSet<string> currentDefinition;
            //    if (_dictionary.TryGetValue(key, out currentDefinition))
            //    {
            //        Queue<HashSet<string>> removal = new Queue<HashSet<string>>();
            //        removal.Enqueue(currentDefinition);
            //        while (removal.Count > 0)
            //        {
            //            HashSet<string> removalSet = removal.Dequeue();
            //            foreach (string meaning in removalSet)
            //            {
            //                if (!_dictionary.TryGetValue(meaning, out currentDefinition) || !_dictionary.Remove(meaning)) continue;
            //                removal.Enqueue(currentDefinition);
            //            }
            //        }
            //    }
            //}
            //return _dictionary.Remove(key);
        }


        #region ISerializable methods
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context) 
        { 
            // Some serialization logic. Coming soon. April '12th...
        }
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
