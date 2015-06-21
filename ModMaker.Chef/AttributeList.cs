using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ModMaker.Chef
{
    /// <summary>
    /// This is either a dictionary of attributes or a single value.  This defines
    /// attributes in Chef.  The keys are names of an attribute and the value is
    /// either another attribute dictionary or a single value.  The root attribute
    /// list is always a dictionary.
    /// </summary>
    public sealed class AttributeList : IEnumerable<KeyValuePair<string, AttributeList>>, IEnumerable<AttributeList>
    {
        /// <summary>
        /// Contains the children attributes of this object.  May be null.
        /// </summary>
        readonly Dictionary<string, AttributeList> children;
        /// <summary>
        /// Contains the children when this is an array.
        /// </summary>
        readonly List<AttributeList> array;

        /// <summary>
        /// Gets whether this object is a dictionary.
        /// </summary>
        public bool IsDictionary { get { return children != null; } }
        /// <summary>
        /// Gets whether this object is an array.
        /// </summary>
        public bool IsArray { get { return array != null; } }
        /// <summary>
        /// Gets whether the object is read-only.
        /// </summary>
        public bool IsReadOnly { get; private set; }
        /// <summary>
        /// Gets the number of attributes in this object, must be a dictionary or an array.
        /// </summary>
        public int Count 
        {
            get 
            {
                if (IsArray)
                    return array.Count;
                else if (IsDictionary)
                    return children.Count;
                else
                    return 1;
            }
        }
        /// <summary>
        /// Gets the value of this attribute.  If this is a simple object,
        /// this will return that; otherwise it will return this.
        /// </summary>
        public object Value { get; private set; }
        /// <summary>
        /// Gets or sets the values for children attributes.  Assigning to
        /// a null value is equivilent to removing the attribute.
        /// </summary>
        /// <param name="key">The name of the attribute.</param>
        /// <returns>The attribute with the given name.</returns>
        /// <exception cref="System.ArgumentNullException">If key is null.</exception>
        /// <exception cref="System.InvalidOperationException">If this is a simple object or an array.</exception>
        public AttributeList this[string key]
        {
            get
            {
                Contract.Requires<InvalidOperationException>(IsDictionary);
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                return children[key];
            }
            set
            {
                Contract.Requires<InvalidOperationException>(!IsReadOnly);
                Contract.Requires<InvalidOperationException>(IsDictionary);
                Contract.Requires<ArgumentNullException>(key != null);
                if (value == null)
                    children.Remove(key);
                else
                    children[key] = value;
            }
        }
        /// <summary>
        /// Gets or sets the values for children attributes. Value cannot be null.
        /// </summary>
        /// <param name="key">The index of the attribute.</param>
        /// <returns>The attribute with the given index.</returns>
        /// <exception cref="System.ArgumentNullException">If value is null.</exception>
        /// <exception cref="System.IndexOutOfRangeException">If key is out of range.</exception>
        /// <exception cref="System.InvalidOperationException">If this is a simple object or a dictionary.</exception>
        public AttributeList this[int key]
        {
            get
            {
                Contract.Requires<InvalidOperationException>(IsArray);
                Contract.Requires(key >= 0 && key < Count);
                Contract.Ensures(Contract.Result<AttributeList>() != null);
                return array[key];
            }
            set
            {
                Contract.Requires<InvalidOperationException>(!IsReadOnly);
                Contract.Requires<InvalidOperationException>(IsArray);
                Contract.Requires<ArgumentNullException>(value != null);
                Contract.Requires(key >= 0 && key < Count);
                array[key] = value;
            }
        }

        /// <summary>
        /// Creates a new AttributeList that wraps the given value.
        /// Warning: This will not correctly convert C# objects to Json objects, 
        /// use a Dictionary&lt;string, object&gt;.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public AttributeList(object value)
        {
            IsReadOnly = false;
            array = null;
            children = null;

            var dict = value as Dictionary<string, object>;
            var jobj = value as JToken;
            if (jobj != null)
            {
                switch (jobj.Type)
                {
                    case JTokenType.Array:
                        Value = this;
                        array = ((JArray)jobj).Select(t => new AttributeList(t)).ToList();
                        break;
                    case JTokenType.Object:
                        Value = this;
                        children = ((JObject)jobj).ToDictionary<KeyValuePair<string, JToken>, string, AttributeList>(kv => kv.Key, kv => new AttributeList(kv.Value));
                        break;
                    case JTokenType.String:
                        Value = jobj.Value<string>();
                        break;
                    case JTokenType.TimeSpan:
                        Value = jobj.Value<TimeSpan>();
                        break;
                    case JTokenType.Integer:
                        Value = jobj.Value<int>();
                        break;
                    case JTokenType.Uri:
                        Value = jobj.Value<Uri>();
                        break;
                    case JTokenType.Boolean:
                        Value = jobj.Value<bool>();
                        break;
                    case JTokenType.Bytes:
                        Value = jobj.Value<byte[]>();
                        break;
                    case JTokenType.Date:
                        Value = jobj.Value<DateTime>();
                        break;
                    case JTokenType.Float:
                        Value = jobj.Value<float>();
                        break;
                    case JTokenType.Guid:
                        Value = jobj.Value<Guid>();
                        break;
                    case JTokenType.Null:
                        Value = null;
                        break;
                    default:
                        throw new ArgumentException(jobj.Type + " is not a supported JObject type.");
                }
            }
            else if (dict != null)
            {
                Value = this;
                children = dict.ToDictionary(kv => kv.Key, kv => new AttributeList(kv.Value));
            }
            else
            {
                Value = value;
            }
        }
        
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(string other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(bool other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(char other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(int other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(uint other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(byte other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(sbyte other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(long other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(ulong other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(float other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(double other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(decimal other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(IntPtr other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }
        /// <summary>
        /// Implicit conversion to an AttributeList.
        /// </summary>
        /// <param name="other">The value to wrap.</param>
        /// <returns>A new AttributeList that wraps the value.</returns>
        public static implicit operator AttributeList(UIntPtr other)
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            return new AttributeList(other);
        }

        /// <summary>
        /// Adds an element to the array.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <exception cref="System.ArgumentNullException">If value is null.</exception>
        /// <exception cref="System.InvalidOperationException">If this is a simple object or a dictionary.</exception>
        public void Add(object value)
        {
            Contract.Requires<InvalidOperationException>(!IsReadOnly);
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Requires<InvalidOperationException>(IsArray);

            AttributeList list = value as AttributeList ?? new AttributeList(value);
            array.Add(list);
        }
        /// <summary>
        /// Adds an element to the dictionary.  This is the same as the indexer.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        /// <param name="value">The value to add.</param>
        /// <exception cref="System.ArgumentNullException">If value or key is null.</exception>
        /// <exception cref="System.InvalidOperationException">If this is a simple object or an array.</exception>
        public void Add(string key, object value)
        {
            Contract.Requires<InvalidOperationException>(!IsReadOnly);
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Requires<ArgumentNullException>(key != null);
            Contract.Requires<InvalidOperationException>(IsDictionary);

            AttributeList list = value as AttributeList ?? new AttributeList(value);
            children[key] = list;
        }
        /// <summary>
        /// Inserts an element to the array.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <exception cref="System.ArgumentNullException">If value is null.</exception>
        /// <exception cref="System.IndexOutOfRangeException">If i is out of range.</exception>
        /// <exception cref="System.InvalidOperationException">If this is a simple object or a dictionary.</exception>
        public void Insert(int i, object value)
        {
            Contract.Requires<InvalidOperationException>(!IsReadOnly);
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Requires<InvalidOperationException>(IsArray);
            Contract.Requires(i >= 0 && i < Count);

            AttributeList list = value as AttributeList ?? new AttributeList(value);
            array.Insert(i, list);
        }
        /// <summary>
        /// Removes an element from the array.
        /// </summary>
        /// <param name="i">The index to remove.</param>
        /// <exception cref="System.IndexOutOfRangeException">If i is out of range.</exception>
        /// <exception cref="System.InvalidOperationException">If this is a simple object or a dictionary.</exception>
        public void Remove(int i)
        {
            Contract.Requires<InvalidOperationException>(!IsReadOnly);
            Contract.Requires<InvalidOperationException>(IsArray);
            Contract.Requires(i >= 0 && i < Count);
            array.RemoveAt(i);
        }
        /// <summary>
        /// Removes an element from the dictionary.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <exception cref="System.InvalidOperationException">If this is a simple object or an array.</exception>
        public void Remove(string key)
        {
            Contract.Requires<InvalidOperationException>(!IsReadOnly);
            Contract.Requires<InvalidOperationException>(IsDictionary);
            Contract.Requires<ArgumentNullException>(key != null);
            children.Remove(key);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            if (IsDictionary)
                return "{ " + string.Join(", ", children.Select(k => "\"" + k.Key + "\" : " + k.Value.ToShortString())) + " }";
            else if (IsArray)
                return "[ " + string.Join(", ", array.Select(a => a.ToShortString())) + " ]";
            else
                return (Value ?? "null").ToString();
        }
        /// <summary>
        /// Returns a short string that represents the current object.
        /// </summary>
        /// <returns>A short string that represents the current object.</returns>
        public string ToShortString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            if (IsDictionary)
                return "{ ... }";
            else if (IsArray)
                return "[ ... ]";
            else
                return (Value ?? "null").ToString();
        }
        /// <summary>
        /// Returns a long string that represents the current object.
        /// </summary>
        /// <returns>A long string that represents the current object.</returns>
        public string ToLongString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            if (IsDictionary)
                return "{ " + string.Join(", ", children.Select(k => "\"" + k.Key + "\" : " + k.Value.ToLongString())) + " }";
            else if (IsArray)
                return "[ " + string.Join(", ", array.Select(a => a.ToLongString())) + " ]";
            else
                return (Value ?? "null").ToString();
        }

        /// <summary>
        /// Makes the current object read-only and returns this.
        /// </summary>
        /// <returns>The current object.</returns>
        public AttributeList MakeReadOnly()
        {
            Contract.Ensures(Contract.Result<AttributeList>() != null);
            Contract.Ensures(IsReadOnly);

            IsReadOnly = true;
            if (IsDictionary)
            {
                foreach (var v in children)
                    v.Value.MakeReadOnly();
            }
            else if (IsArray)
            {
                foreach (var v in array)
                    v.MakeReadOnly();
            }

            return this;
        }

        /// <summary>
        /// Converts the current object to a simple Dictionary used to
        /// convert to JSON.
        /// </summary>
        /// <returns>A Json object from the instance.</returns>
        internal JToken ToData()
        {
            Contract.Ensures(Contract.Result<JToken>() != null);

            if (IsDictionary)
            {
                JObject ret = new JObject();
                foreach (var i in children)
                {
                    ret.Add(i.Key, i.Value.ToData());
                }
                return ret;
            }
            else if (IsArray)
            {
                JArray ret = new JArray();
                foreach (var i in array)
                {
                    ret.Add(i.ToData());
                }
                return ret;
            }
            else if (Value == null)
                return JValue.CreateNull();
            else
                return JToken.FromObject(Value);
        }
        
        public IEnumerator GetEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator>() != null);

            if (IsDictionary)
                return children.GetEnumerator();
            else if (IsArray)
                return array.GetEnumerator();
            else
                return new[] { Value }.GetEnumerator();
        }
        IEnumerator<KeyValuePair<string, AttributeList>> IEnumerable<KeyValuePair<string, AttributeList>>.GetEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator<KeyValuePair<string, AttributeList>>>() != null);

            if (IsDictionary)
                return children.GetEnumerator();
            else
                return new List<KeyValuePair<string, AttributeList>>().GetEnumerator();
        }
        IEnumerator<AttributeList> IEnumerable<AttributeList>.GetEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator<AttributeList>>() != null);

            if (IsArray)
                return array.GetEnumerator();
            else
                return new List<AttributeList>().GetEnumerator();
        }
    }
}