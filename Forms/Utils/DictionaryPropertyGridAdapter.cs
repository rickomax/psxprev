using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PSXPrev.Forms.Utils
{
    public class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        private readonly IDictionary _dictionary;

        public object Key { get; }

        public DictionaryPropertyDescriptor(IDictionary dictionary, object key, string name = null, string displayName = null, string description = null, string category = null)
            : base(name ?? key.ToString(), CreateAttributes(displayName, description, category))
        {
            _dictionary = dictionary;
            Key = key;
        }

        private static Attribute[] CreateAttributes(string displayName, string description, string category)
        {
            var attributes = new List<Attribute>();
            if (displayName != null)
            {
                attributes.Add(new DisplayNameAttribute(displayName));
            }
            if (description != null)
            {
                attributes.Add(new DescriptionAttribute(description));
            }
            if (category != null)
            {
                attributes.Add(new CategoryAttribute(category));
            }
            return attributes.ToArray();
        }

        public override bool IsReadOnly => false;

        public override Type ComponentType => null;

        public override Type PropertyType => _dictionary[Key].GetType();

        public override void SetValue(object component, object value)
        {
            _dictionary[Key] = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[Key];
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }

    public class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        private readonly IDictionary _dictionary;

        private readonly Comparison<object> _keyOrderer;
        private readonly Func<object, string> _keyName;
        private readonly Func<object, string> _keyDisplayName;
        private readonly Func<object, string> _keyDescription;
        private readonly Func<object, string> _keyCategory;

        public DictionaryPropertyGridAdapter(IDictionary dictionary, Comparison<object> keyOrderer = null,
                                             Func<object, string> keyName = null, Func<object, string> keyDisplayName = null,
                                             Func<object, string> keyDescription = null, Func<object, string> keyCategory = null)
        {
            _dictionary = dictionary;
            _keyOrderer = keyOrderer;
            _keyName = keyName;
            _keyDisplayName = keyDisplayName;
            _keyDescription = keyDescription;
            _keyCategory = keyCategory;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var properties = new DictionaryPropertyDescriptor[_dictionary.Count];
            var index = 0;
            foreach (DictionaryEntry e in _dictionary)
            {
                var key = e.Key;
                var name = _keyName?.Invoke(key);
                var displayName = _keyDisplayName?.Invoke(key);
                var description = _keyDescription?.Invoke(key);
                var category = _keyCategory?.Invoke(key);
                properties[index++] = new DictionaryPropertyDescriptor(_dictionary, key, name, displayName, description, category);
            }
            if (_keyOrderer != null)
            {
                Array.Sort(properties, KeyOrderer);
            }

            return new PropertyDescriptorCollection(properties);
        }

        private int KeyOrderer(DictionaryPropertyDescriptor a, DictionaryPropertyDescriptor b)
        {
            return _keyOrderer(a.Key, b.Key);
        }
    }
}