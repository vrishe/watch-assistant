using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace watch_assistant.Model.Dictionary
{
    [Serializable]
    public sealed class Thesaurus/* : ISerializable, IDeserializationCallback*/
    {
        #region Fields
        private static List<WeakReference> _instances = new List<WeakReference>();

        private string _name;
        private Dictionary<string, HashSet<string>> _dictionary = new Dictionary<string, HashSet<string>>();
        #endregion (Fields)

        #region Properties
        public string Name
        {
            get { return _name; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _instances.RemoveAll((obj) => { return !(obj as WeakReference).IsAlive; });
                    value = String.Format("Thesaurus{0}", _instances.Count);
                }
                _name = value;
            }
        }
        public int Count { get { return _dictionary.Count; } }
        #endregion (Properties)

        //#region Events
        //public event EventHandler Deserialized;
        //#endregion (Events)

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
                        if (mutual) RemoveDefinition(meaning, false);
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

        public bool HasDefinitionFor(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public string[] GetPhraseVariations(string phrase)
        {
            HashSet<string> variations = new HashSet<string>();

            lock (this)
            {
                Queue<KeyValuePair<string, List<string>>> processQueue = new Queue<KeyValuePair<string, List<string>>>();
                processQueue.Enqueue(new KeyValuePair<string, List<string>>(phrase.ToLower(), new List<string>()));
                do
                {
                    KeyValuePair<string, List<string>> blank = processQueue.Dequeue();
                    List<string> rest = new List<string>(_dictionary.Keys.Except(blank.Value));
                    foreach (string key in rest)
                    {
                        if (blank.Key.Contains(key))
                        {
                            IEnumerable<string> replacement = _dictionary[key].Except(blank.Value);
                            foreach (string substitution in replacement)
                            {
                                List<string> exclusion = new List<string>(blank.Value);
                                exclusion.Add(substitution);
                                string expression = blank.Key.Replace(key, substitution);
                                processQueue.Enqueue(new KeyValuePair<string, List<string>>(expression, exclusion));
                            }
                        }
                    }
                    variations.Add(blank.Key);
                } while (processQueue.Count > 0);
            }
            return variations.ToArray();
        }

        public void Serialize(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Create);

            BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            bf.Serialize(fs, _dictionary);
            fs.Close();
        }

        public void Deserialize(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);

            BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            _dictionary = (Dictionary<string, HashSet<string>>)bf.Deserialize(fs);
            fs.Close();
        }

        //#region ISerializable methods
        //[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        //public void GetObjectData(SerializationInfo info, StreamingContext context) 
        //{
        //    info.AddValue("Name", Name);
        //    _dictionary.GetObjectData(info, context);
        //}
        //#endregion (ISerializable methods)

        //#region IDeserializationCallback methods
        //public void OnDeserialization(object sender) 
        //{
        //    if (Deserialized != null) Deserialized(sender, new EventArgs());
        //}
        //#endregion (IDeserializationCallback methods)
        #endregion (Methods)

        #region Constructors
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public Thesaurus()
        {
            _instances.Add(new WeakReference(this));

            _dictionary = new Dictionary<string, HashSet<string>>();
            Name = null;
        }
        public Thesaurus(string filePath)
            : this()
        {
            Deserialize(filePath);
        }

        #endregion (Constructors)
    }
}
