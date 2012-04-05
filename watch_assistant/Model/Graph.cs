using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace watch_assistant.Model
{
    public class Graph<TNode, TWeight>
    {
        #region Enums and structs
        private struct Node
        {
            private Graph<TNode, TWeight> _owner;
            private TNode _value;
            private int _index;

            public TNode Value { get { return _value; } }
            public int Index { get { return _index; } }

            public Node(Graph<TNode, TWeight> owner, int index, TNode value) { _owner = owner; _index = index; _value = value; }
            public override bool Equals(object obj)
            {
                if ( base.Equals(obj) ) return true;

                try 
                {
                    return ((Node)obj)._index.Equals(_index);
                }
                catch (NullReferenceException) { return false; }
                catch(InvalidCastException) { return false; }
            }
            public override string ToString()
            {
                return String.Format("{0}: [Node index: {1}, Value: {2}]", base.ToString(), _index, _value);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private struct Link
        {
            private Graph<TNode, TWeight> _owner;
            private Node _from;
            private Node _to;
            private TWeight _weight;

            public Node From { get { return _from; } }
            public Node To { get { return _to; } }
            public TWeight Weight { get { return _weight; } }

            public Link(Graph<TNode, TWeight> owner, Node from, Node to, TWeight weight) 
            { 
                _owner = owner; 
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
        private readonly SortedSet<Node> _nodes = new SortedSet<Node>();
        #endregion (Fields)
    }
}
