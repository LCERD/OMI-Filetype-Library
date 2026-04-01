using System;
using System.Collections.Generic;
using System.Linq;

namespace OMI.Formats.Pck
{
    using ParameterValueType = KeyValuePair<string, string>;

    internal class PckFileParameters : List<ParameterValueType>
    {
        internal void Add<T>(string key, T value)
        {
            Add(new ParameterValueType(key, value.ToString()));
        }

        internal void Add<T>((string key, T value) parameter)
        {
            Add(new ParameterValueType(parameter.key, parameter.value.ToString()));
        }

        internal void Add(string key, string value)
        {
            Add(new ParameterValueType(key, value));
        }

        internal bool TryGetParameter(string parameter, out string value)
        {
            if (Contains(parameter))
            {
                value = GetParameterValue(parameter);
                return true;
            }
            value = null;
            return false;
        }

        internal bool Remove(string parameter)
        {
            if (!Contains(parameter))
                return false;
            int index = FindIndex(p => p.Key == parameter);
            RemoveAt(index);
            return true;
        }

        internal bool Contains(string parameter)
        {
            return Exists(p => p.Key == parameter);
        }

        internal ParameterValueType GetParameter(string parameter)
        {
            return this.FirstOrDefault(p => p.Key.Equals(parameter))!;
        }

        internal T GetParameterValue<T>(string parameter, Func<string, T> func)
        {
            return func(GetParameterValue(parameter));
        }

        internal string GetParameterValue(string parameter)
        {
            return GetParameter(parameter).Value;
        }

        internal ParameterValueType[] GetParameters(string parameter)
        {
            return FindAll(p => p.Key == parameter).ToArray();
        }

        internal bool HasMoreThanOneOf(string parameter)
        {
            return GetParameters(parameter).Length > 1;
        }

        internal void Merge(PckFileParameters other)
        {
            AddRange(other);
        }

        internal void SetParameter(string parameter, string value)
        {
            if (Contains(parameter))
            {
                this[IndexOf(GetParameter(parameter))] = new ParameterValueType(parameter, value);
                return;
            }
            Add(new ParameterValueType(parameter, value));
        }

    }
}
