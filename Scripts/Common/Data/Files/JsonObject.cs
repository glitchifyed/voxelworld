using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

public class JsonObject
{
    private readonly string _string;
    private readonly bool _boolean;
    private readonly JsonObject[] _array;
    private readonly Dictionary<string, JsonObject> _dictionary;

    private readonly ValueType _valueType;


    public JsonObject(string @string)
    {
        _string = @string;

        _valueType = ValueType.String;
    }
    
    public JsonObject(bool boolean)
    {
        _boolean = boolean;
        
        _valueType = ValueType.Boolean;
    }

    public JsonObject(JsonObject[] array)
    {
        _array = array;

        _valueType = ValueType.Array;
    }

    public JsonObject(Dictionary<string, JsonObject> dictionary)
    {
        _dictionary = dictionary;

        _valueType = ValueType.Dictionary;
    }


    // Getters
    public bool TryGet<T>(out T value)
    {
        var type = typeof(T);
        
        if (type == typeof(string))
        {
            value = (T) (object) _string;
            
            return IsString();
        }
        else if (type == typeof(bool))
        {
            value = (T) (object) _boolean;
            
            return IsBoolean();
        }
        
        value = default;
        
        return false;
    }

    public bool TryGet(int index, out JsonObject value)
    {
        value = IsArray() ? _array[index] : default;
        
        return !value.IsEmpty();
    }

    public bool TryGet(string key, out JsonObject value)
    {
        if (IsDictionary())
            _dictionary.TryGetValue(key, out value);
        else
            value = default;

        return !value.IsEmpty();
    }
    
    
    // Enumerators
    public IEnumerable<JsonObject> GetArrayEnumerator()
    {
        ThrowIfNot(ValueType.Array);
        
        foreach (var value in _array)
            yield return value;
    }
    
    public IEnumerable<KeyValuePair<string, JsonObject>> GetDictionaryEnumerator()
    {
        ThrowIfNot(ValueType.Dictionary);
        
        foreach (var value in _dictionary)
            yield return value;
    }


    public bool IsEmpty()
        => _valueType == ValueType.Empty;
    
    public bool IsString()
        => _valueType == ValueType.String;
    
    public bool IsBoolean()
        => _valueType == ValueType.Boolean;
    
    public bool IsArray()
        => _valueType == ValueType.Array;
    
    public bool IsDictionary()
        => _valueType == ValueType.Dictionary;
    
    
    private void ThrowIfNot(ValueType valueType)
    {
        if (_valueType != valueType)
            throw new Exception($"{_valueType} is not ValueType.{valueType}");
    }


    private enum ValueType
    {
        Empty,

        String,
        Boolean,

        Array,
        Dictionary
    }
}