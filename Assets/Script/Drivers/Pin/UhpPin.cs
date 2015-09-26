using System.Collections.Generic;

public class UhpPin
{
    public enum IOMode
    {
        IN, OUT, INOUT
    }

    public const string JSON_NAME_KEY = "name";
    public const string JSON_MODE_KEY = "mode";
    public const string JSON_TYPE_KEY = "type";
    public const string JSON_MNEMONIC_KEY = "mnemonic";
    public const string JSON_DESCRIPTION_KEY = "description";

    public string name { get; set; }
    public IOMode mode { get; set; }
    public UhpType type { get; set; }
    public string mnemonic { get; set; }
    public string description { get; set; }

    public IDictionary<string, object> ToJSON()
    {
        var json = new Dictionary<string, object>();
        if (name != null)
            json[JSON_NAME_KEY] = name;
        json[JSON_MODE_KEY] = mode.ToString();
        if (type != null)
            json[JSON_TYPE_KEY] = type.ToJSON();
        if (mnemonic != null)
            json[JSON_MNEMONIC_KEY] = mnemonic;
        if (description != null)
            json[JSON_DESCRIPTION_KEY] = description;
        return json;
    }

    public override bool Equals(object obj)
    {
        if (obj == this)
            return true;
        if (!(obj is UhpPin))
            return false;
        UhpPin other = (UhpPin)obj;

        if (!object.Equals(this.name, other.name))
            return false;
        if (!object.Equals(this.mode, other.mode))
            return false;
        if (!object.Equals(this.type, other.type))
            return false;
        if (!object.Equals(this.mnemonic, other.mnemonic))
            return false;
        if (!object.Equals(this.description, other.description))
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        hash ^= (name != null) ? name.GetHashCode() : 0;
        hash ^= mode.GetHashCode();
        hash ^= (type != null) ? type.GetHashCode() : 0;
        hash ^= (mnemonic != null) ? mnemonic.GetHashCode() : 0;
        hash ^= (description != null) ? description.GetHashCode() : 0;
        return hash;
    }

    public override string ToString()
    {
        return MiniJSON.Json.Serialize(ToJSON());
    }
}
