using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace watch_assistant.Model.Dictionary
{
    public enum PermutationMethod
    {
        NONE,
        SINGLE,
        FULL
    }

    public sealed class Thesaurus
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
        public IEnumerable<string> Keys { get { return _dictionary.Keys; } }
        #endregion (Properties)

        #region Methods
        /// <summary>
        /// Appens another description group to an existent definition of the key.
        /// </summary>
        /// <param name="key">Dictionary key that may have a definition.</param>
        /// <param name="definition">Dictionary definition that opens sense of the key.</param>
        /// <param name="mutual">Tells if it has to append the 'key' as a meaning with each of the corresponding 'meaning' values as keys within the thesaurus.</param>
        public void AddDefinition(string key, IEnumerable<string> definition, PermutationMethod mutual)
        {
            if (definition.Count() == 0)
                throw new ArgumentException("Definition set cannot be empty.");
            if (definition.Contains(key))
                throw new ArgumentException("Definition set should not contain the 'key' meaning.");
            if (definition.Contains(String.Empty))
                throw new ArgumentException("Definition set should not contain empty tokens.");
            lock (this)
            {
                if (!_dictionary.ContainsKey(key)) _dictionary.Add(key, new HashSet<string>());
                _dictionary[key].UnionWith(definition);
                if (mutual != PermutationMethod.NONE)
                {
                    foreach (string meaning in definition)
                    {
                        HashSet<string> extensibleDefinition;
                        if (!_dictionary.TryGetValue(meaning, out extensibleDefinition)) _dictionary.Add(meaning, (extensibleDefinition = new HashSet<string>()));
                        extensibleDefinition.Add(key);
                        if (mutual == PermutationMethod.FULL) extensibleDefinition.UnionWith(_dictionary[key].Except(new string[] { meaning }));
                    }
                }
            }
        }
        /// <summary>
        /// Appens another description to an existent definition of the key.
        /// </summary>
        /// <param name="key">Dictionary key that may have a definition.</param>
        /// <param name="meaning">Dictionary meaning that opens sense of the key.</param>
        /// <param name="mutual">Tells if it has to append the 'key' as a meaning to corresponding 'meaning' key mutually.</param>
        public void AddDefinition(string key, string meaning, PermutationMethod mutual)
        {
            AddDefinition(key, new string[] { meaning }, mutual);
        }
        /// <summary>
        /// Removes the definition of a key specified.
        /// </summary>
        /// <param name="key">Dictionary key whose definition is about to remove.</param>
        /// <param name="mutual">Tells if it has to remove each of 'meaning' values as a keys within the current Thesaurus.</param>
        /// <returns>True - if key removal was successfull; False - otherwise.</returns>
        public bool RemoveDefinition(string key, string meaning, PermutationMethod mutual)
        {
            lock (this)
            {
                HashSet<string> currentDefinition;
                if (_dictionary.TryGetValue(key, out currentDefinition))
                {
                    if (currentDefinition.Remove(meaning))
                    {
                        if (currentDefinition.Count == 0) _dictionary.Remove(key);
                        if (mutual != PermutationMethod.NONE) RemoveDefinition(meaning, mutual == PermutationMethod.SINGLE ? PermutationMethod.NONE : PermutationMethod.SINGLE);
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// Removes the definition of a key specified.
        /// </summary>
        /// <param name="key">Dictionary key whose definition is about to remove.</param>
        /// <param name="mutual">Tells if it has to remove each of 'meaning' values as a keys within the current Thesaurus.</param>
        /// <returns>True - if key removal was successfull; False - otherwise.</returns>
        public bool RemoveDefinition(string key, PermutationMethod mutual)
        {
            lock (this)
            {
                HashSet<string> currentDefinition;
                if (mutual != PermutationMethod.NONE && _dictionary.TryGetValue(key, out currentDefinition))
                {
                    foreach (string meaning in currentDefinition)
                    {
                        RemoveDefinition(meaning, key, mutual == PermutationMethod.SINGLE ? PermutationMethod.NONE : PermutationMethod.SINGLE);
                    }
                }
                return _dictionary.Remove(key);
            }
        }
        /// <summary>
        /// Overrides currently existent key definition.
        /// </summary>
        /// <param name="key">Dictionary key that may have a definition.</param>
        /// <param name="meaning">Dictionary meaning that opens sense of the key.</param>
        /// <param name="mutual">Tells if it has to append the 'key' as a meaning with each of the corresponding 'meaning' values as keys within the thesaurus.</param>
        public void SetDefinition(string key, IEnumerable<string> definition, PermutationMethod mutual)
        {
            lock (this)
            {
                RemoveDefinition(key, mutual);
                AddDefinition(key, definition, mutual);
            }
        }
        /// <summary>
        /// Tries to get a definition for the key specified.
        /// </summary>
        /// <param name="key">the key whose definition is intended to retrieve.</param>
        /// <param name="definition">in case of success recieves a shallow copy of a definition's meanings list.</param>
        /// <returns>True - if definition was retrieved successfully; False - otherwise.</returns>
        public bool TryGetDefinition(string key, out IEnumerable<string> definition)
        {
            definition = null;
            HashSet<string> tempDefinition;
            if (_dictionary.TryGetValue(key, out tempDefinition))
            {
                definition = new List<string>(tempDefinition);
            }
            return definition != null;
        }
        /// <summary>
        /// Defines the Thesaurus' definition existence for the key specified.
        /// </summary>
        /// <param name="key">Dictionary key whose definition existence is intended to be specified.</param>
        /// <returns>True - if key definition exists; False - otherwise.</returns>
        public bool HasDefinitionFor(string key)
        {
            return _dictionary.ContainsKey(key);
        }
        /// <summary>
        /// Defines the Thesaurus' meaning existence within the definition for the key specified.
        /// </summary>
        /// <param name="key">Dictionary key whose definition woulf be inspected for the 'meaning' inclusion.</param>
        /// <param name="meaning">Meaning token whose existence within the definition of the 'key' would be inspected</param>
        /// <returns>True - if key meaning exists; False - otherwise.</returns>
        public bool HasMeaningFor(string key, string meaning)
        {
            return _dictionary.ContainsKey(key) && _dictionary[key].Contains(meaning);
        }
        /// <summary>
        /// Gets a complete set of all possible permutations within the given phrase with definitions included.
        /// </summary>
        /// <param name="phrase">The phrase whose permutations will be found for.</param>
        /// <returns>The string array that contains all possible permutations within the given phrase inside of the Thesaurus context.</returns>
        public string[] GetPhrasePermutations(string phrase)
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
        /// <summary>
        /// Provides a file serialization for Thesaurus dictionary.
        /// </summary>
        /// <param name="filePath">File path which will contain Thesaurus content data.</param>
        public void Serialize(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Create);

            BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            bf.Serialize(fs, _dictionary);
            fs.Close();
        }
        /// <summary>
        /// Provides deserialization from a file for Thesaurus dictionary.
        /// </summary>
        /// <param name="filePath">File path which contains Thesaurus content data.</param>
        public void Deserialize(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);

            BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            _dictionary = (Dictionary<string, HashSet<string>>)bf.Deserialize(fs);
            fs.Close();
        }
        #endregion (Methods)

        #region Constructors
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
