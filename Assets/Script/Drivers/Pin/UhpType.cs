using System.Collections.Generic;

public class UhpType
{
    public enum BaseType
    {
        DISCRETE, CONTINUOUS, ARRAY, STRUCTURED
    }

    public const string JSON_BASE_TYPE_KEY = "baseType";
    public const string JSON_DISCRETE_RANGE_START_KEY = "discreteRangeStart";
    public const string JSON_DISCRETE_RANGE_SIZE_KEY = "discreteRangeSize";
    public const string JSON_CONTINUOUS_RANGE_START_KEY = "continuousRangeStart";
    public const string JSON_CONTINUOUS_RANGE_SIZE_KEY = "continuousRangeSize";
    public const string JSON_ARRAY_DIMENSION_KEY = "dimension";
    public const string JSON_ARRAY_ELEMENT_TYPE_KEY = "elementType";
    public const string JSON_STRUCT_FIELDS_KEY = "fields";

    public static readonly UhpType bit = Discrete(0L, 2L);
    public static readonly UhpType uniform = Continuous(-1.0, 2.0);
    public static readonly UhpType v2 = Array(Continuous(), 2);
    public static readonly UhpType v3 = Array(Continuous(), 3);

    public BaseType baseType { get; set; }
    public int? dimension { get; set; }
    public UhpType elementType { get; set; }
    public Dictionary<string, UhpType> fields { get; set; }
    public long? discRangeStart { get; set; }
    private long? discRangeSize { get; set; }
    private double? contRangeStart { get; set; }
    private double? contRangeSize { get; set; }

    public bool valid
    {
        get
        {
            switch (baseType)
            {
                case BaseType.DISCRETE:
                    return ((discRangeSize == null) || (discRangeSize > 0));

                case BaseType.CONTINUOUS:
                    return ((contRangeSize == null) || (contRangeSize > 0));

                case BaseType.ARRAY:
                    return (dimension != null) && (dimension >= 1) && (elementType != null) && elementType.valid;

                case BaseType.STRUCTURED:
                    if ((fields == null) || (fields.Count == 0))
                        return false;
                    foreach (var entry in fields)
                    {
                        if (string.IsNullOrEmpty(entry.Key) || (entry.Value == null) || (!entry.Value.valid))
                            return false;
                    }
                    break;
            }
            return true;
        }
    }

    public static UhpType Discrete()
    {
        return NewDiscrete(null, null);
    }

    public static UhpType Discrete(long start)
    {
        return NewDiscrete(start, null);
    }

    public static UhpType Discrete(long start, long size)
    {
        return NewDiscrete(start, size);
    }

    private static UhpType NewDiscrete(long? start, long? size)
    {
        UhpType type = new UhpType(BaseType.DISCRETE);
        type.discRangeStart = start;
        type.discRangeSize = size;
        return type;
    }

    public static UhpType Continuous()
    {
        return NewContinuous(null, null);
    }

    public static UhpType Continuous(double start)
    {
        return NewContinuous(start, null);
    }

    public static UhpType Continuous(double start, double size)
    {
        return NewContinuous(start, size);
    }

    private static UhpType NewContinuous(double? start, double? size)
    {
        UhpType type = new UhpType(BaseType.CONTINUOUS);
        type.contRangeStart = start;
        type.contRangeSize = size;
        return type;
    }

    public static UhpType Array(UhpType elementType, int dimension)
    {
        UhpType type = new UhpType(BaseType.ARRAY);
        type.elementType = elementType;
        type.dimension = dimension;
        return type;
    }

    public static UhpType Struct(Dictionary<string, UhpType> fields)
    {
        UhpType type = new UhpType(BaseType.STRUCTURED);
        type.fields = fields;
        return type;
    }

    public UhpType(BaseType baseType = BaseType.DISCRETE)
    {
        this.baseType = baseType;
    }

    public void AddField(string name, UhpType type)
    {
        if (fields == null)
            fields = new Dictionary<string, UhpType>();
        fields[name] = type;
    }



    public object ParseValue(object src)
    {
        if (src == null)
            throw new System.InvalidOperationException("source object");

        object result = null;
        switch (baseType)
        {
            case BaseType.DISCRETE:
                if (src is int)
                    result = ValidateLong((int)src);
                else if (src is long)
                    result = ValidateLong((long)src);
                else
                    result = ValidateLong(long.Parse(src.ToString()));
                break;

            case BaseType.CONTINUOUS:
                if (src is float)
                    result = ValidateDouble((float)src);
                else if (src is double)
                    result = ValidateDouble((double)src);
                else if (src is int)
                    result = ValidateDouble((int)src);
                else if (src is long)
                    result = ValidateDouble((long)src);
                else
                    result = ValidateDouble(double.Parse(src.ToString()));
                break;

            case BaseType.ARRAY:
                if (src.GetType().GetGenericTypeDefinition() == typeof(List<>))
                    result = ValidateArray(((List<object>)src).ToArray());
                else if (src.GetType().IsArray)
                    result = ValidateArray((object[])src);
                else
                    throw new System.InvalidOperationException("exptected an array or list");
                break;

            case BaseType.STRUCTURED:
                if (src.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    result = ValidateMap((Dictionary<string, object>)src);
                else
                    throw new System.InvalidOperationException("exptected a map");
                break;
        }

        return result;
    }

    private long ValidateLong(long value)
    {
        if (discRangeStart != null)
        {
            if ((discRangeSize != null) && (discRangeSize <= 0))
                throw new System.InvalidOperationException("range size is invalid");
            if ((value < discRangeStart) || ((discRangeSize != null) && (value >= discRangeStart + discRangeSize)))
                throw new System.InvalidOperationException("value outside of defined range");
        }
        return value;
    }

    private double ValidateDouble(double value)
    {
        if (contRangeStart != null)
        {
            if ((contRangeSize != null) && (contRangeSize <= 0))
                throw new System.InvalidOperationException("range size is invalid");
            if ((value < contRangeStart) || ((contRangeSize != null) && (value >= contRangeStart + contRangeSize)))
                throw new System.InvalidOperationException("value outside of defined range");
        }
        return value;
    }

    private object[] ValidateArray(object[] array)
    {
        if ((dimension == null) || (dimension < 1))
            throw new System.InvalidOperationException("array dimension is invalid");
        if (elementType == null)
            throw new System.InvalidOperationException("array element type is invalid");
        if (array.Length != dimension)
            throw new System.InvalidOperationException("expected array of size " + dimension);

        object[] result = new object[array.Length];
        for (int i = 0; i < array.Length; ++i)
        {
            try
            {
                result[i] = elementType.ParseValue(array[i]);
            }
            catch (System.Exception e)
            {
                throw new System.InvalidOperationException("array element at " + i + " is invalid", e);
            }
        }
        return result;
    }

    private Dictionary<string, object> ValidateMap(Dictionary<string, object> map)
    {
        if (fields == null)
            throw new System.InvalidOperationException("no map fields defined");

        var result = new Dictionary<string, object>();
        foreach (var entry in fields)
        {
            string fieldName = entry.Key;
            object value;
            if (!map.TryGetValue(fieldName, out value))
                throw new System.InvalidOperationException("field " + fieldName + " not found in src");
            try
            {
                result[fieldName] = entry.Value.ParseValue(value);
            }
            catch (System.Exception e)
            {
                throw new System.InvalidOperationException("field " + fieldName + " is invalid", e);
            }
        }
        return result;
    }

    public override bool Equals(object obj)
    {
        if (obj == this)
            return true;
        if (!(obj is UhpType))
            return false;
        UhpType other = (UhpType)obj;

        if (!object.Equals(this.baseType, other.baseType))
            return false;
        if (!object.Equals(this.dimension, other.dimension))
            return false;
        if (!object.Equals(this.elementType, other.elementType))
            return false;
        if (!object.Equals(this.fields, other.fields))
            return false;
        if (!object.Equals(this.discRangeStart, other.discRangeStart))
            return false;
        if (!object.Equals(this.discRangeSize, other.discRangeSize))
            return false;
        if (!object.Equals(this.contRangeStart, other.contRangeStart))
            return false;
        if (!object.Equals(this.contRangeSize, other.contRangeSize))
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        hash ^= baseType.GetHashCode();
        hash ^= (dimension != null) ? dimension.GetHashCode() : 0;
        hash ^= (elementType != null) ? elementType.GetHashCode() : 0;
        hash ^= (fields != null) ? fields.GetHashCode() : 0;
        hash ^= (discRangeStart != null) ? discRangeStart.GetHashCode() : 0;
        hash ^= (discRangeSize != null) ? discRangeSize.GetHashCode() : 0;
        hash ^= (contRangeStart != null) ? contRangeStart.GetHashCode() : 0;
        hash ^= (contRangeSize != null) ? contRangeSize.GetHashCode() : 0;
        return hash;
    }

    public override string ToString()
    {
        return MiniJSON.Json.Serialize(ToJSON());
    }

    public IDictionary<string, object> ToJSON()
    {
        var json = new Dictionary<string, object>();
        json[JSON_BASE_TYPE_KEY] = baseType;
        if (dimension != null)
            json[JSON_ARRAY_DIMENSION_KEY] = dimension;
        if (elementType != null)
            json[JSON_ARRAY_ELEMENT_TYPE_KEY] = elementType;
        if (fields != null)
        {
            var aux = new Dictionary<string, object>();
            foreach (var field in fields)
                aux[field.Key] = field.Value.ToJSON();
            json[JSON_STRUCT_FIELDS_KEY] = aux;
        }
        if (discRangeStart != null)
            json[JSON_DISCRETE_RANGE_START_KEY] = discRangeStart;
        if (discRangeSize != null)
            json[JSON_DISCRETE_RANGE_SIZE_KEY] = discRangeSize;
        if (contRangeStart != null)
            json[JSON_CONTINUOUS_RANGE_START_KEY] = contRangeStart;
        if (contRangeSize != null)
            json[JSON_CONTINUOUS_RANGE_SIZE_KEY] = contRangeSize;
        return json;
    }
}
