using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.ObjectModel;

namespace watch_assistant.Model
{
    public class Graph<TNode, TWeight> //: ISerializable
    {
        #region Enums and structs
        //private struct Node
        //{
        //    private Graph<TNode, TWeight> _owner;
        //    private TNode _value;
        //    private int _index;

        //    public TNode Value { get { return _value; } }
        //    public int Index { get { return _index; } }

        //    public Node(Graph<TNode, TWeight> owner, int index, TNode value) 
        //    {  

        //        _owner = owner; 
        //        _index = index; 
        //        _value = value; 
        //    }
        //    public override bool Equals(object obj)
        //    {
        //        if ( base.Equals(obj) ) return true;

        //        try 
        //        {
        //            return ((Node)obj)._index.Equals(_index);
        //        }
        //        catch (NullReferenceException) { return false; }
        //        catch(InvalidCastException) { return false; }
        //    }
        //    public override string ToString()
        //    {
        //        return String.Format("{0}: [Node index: {1}, Value: {2}]", base.ToString(), _index, _value);
        //    }
        //    public override int GetHashCode()
        //    {
        //        return base.GetHashCode();
        //    }
        //}
        private struct Link
        {
            private TNode _from;
            private TNode _to;
            private TWeight _weight;

            public TNode From { get { return _from; } }
            public TNode To { get { return _to; } }
            public TWeight Weight { get { return _weight; } }

            public Link(TNode from, TNode to, TWeight weight) 
            {
                _from = from;
                _to = to;
                
                _weight = weight;
            }

            public override bool Equals(object obj)
            {
                if ( base.Equals(obj) ) return true;

                try
                {
                    Link link = (Link)obj;
                    return link._from.Equals(_from) && link._to.Equals(_to) && link._weight.Equals(_weight);
                }
                catch (NullReferenceException) { return false; }
                catch (InvalidCastException) { return false; }
            }
            public override string ToString()
            {
                return String.Format("{0}: [From: {1}; Weight: {2}; To: {3}]", base.ToString(), _from, _weight, _to);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        #endregion (Enums and structs)

        #region Fields
        private static List<WeakReference> _instances = new List<WeakReference>();
        private readonly HashSet<TNode> _nodes = new HashSet<TNode>();
        private readonly HashSet<Link> _links = new HashSet<Link>();

        private bool _isOriented;
        #endregion (Fields)

        #region Properties
        public bool IsOriented { get { return _isOriented; } }
        public int NodesCount { get { return _nodes.Count; } }
        public int LinksCount { get { return _links.Count; } }
        public string Name { get; set; }
        #endregion (Properties)

        #region Methods
        public int Add(Collection<TNode> nodes)
        {
            int result = 0;
            foreach (TNode node in nodes)
            {
                if (node != null)
                {
                    _nodes.Add(node);
                    result++;
                }
#if DEBUG
                else
                {
                    throw new ArgumentNullException(String.Format("Element number {0} is null @ {1}", nodes.IndexOf(node)));            
                }
#endif
            }
            return result;
        }
        public bool Add(TNode node)
        {
            Collection<TNode> nodes = new Collection<TNode>();
            nodes.Add(node);
            return this.Add(nodes) != 0;
        }
        public int Add(Collection<Link> links, bool addInexistentNodes)
        {
            int result = 0;
            foreach (Link link in links)
            {
                if (!_links.Contains(link))
                {
                    if (addInexistentNodes)
                    {
                        Collection<TNode> nodes = new Collection<TNode>();
                        if (!_nodes.Contains(link.From)) nodes.Add(link.From);
                        if (!_nodes.Contains(link.To)) nodes.Add(link.To);

                        if (this.Add(nodes) != nodes.Count) continue;
                    }
                    else
                    {
                        if (!_nodes.Contains(link.From) || !_nodes.Contains(link.To)) continue;
                    }

                    _links.Add(link);
                    result++;
                }
            }
            return result;
        }
        public bool Add(Link link, bool addInexistentNodes)
        {
            Collection<Link> links = new Collection<Link>();
            links.Add(link);
            return this.Add(links, addInexistentNodes) != 0;
        }
        public int Remove(Collection<TNode> nodes)
        {
            int result = 0;
            foreach (TNode node in nodes)
            {
                if (_nodes.Remove(node)) result++;
            }
            return result;
        }
        public bool Remove(TNode node) 
        {
            Collection<TNode> nodes = new Collection<TNode>();
            nodes.Add(node);            
            return this.Remove(nodes) != 0; 
        }

        private static int GetInstancesCount()
        {
            _instances.RemoveAll((obj) => { return !(obj as WeakReference).IsAlive; });
            return _instances.Count;
        }
        #endregion (Methods)

        #region Constructors
        public Graph(string name, bool isOriented) { this.Name = name; _isOriented = isOriented; _instances.Add(new WeakReference(this)); }
        public Graph(bool isOriented) : this(String.Format("{0} Graph{1}", isOriented ? "Oriented" : "Non-oriented", Graph<TNode, TWeight>.GetInstancesCount()), isOriented) { }
        #endregion (Constructors)
    }
}
