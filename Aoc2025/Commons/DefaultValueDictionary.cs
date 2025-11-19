namespace Advent_of_Code_2025.Commons;

public class DefaultValueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    readonly Dictionary<TKey, TValue> mDict;
    private readonly Func<TValue> mDefaultValueFactory;

    public DefaultValueDictionary(Func<TValue> defaultValueFactory)
    {
        mDict = new Dictionary<TKey, TValue>();
        mDefaultValueFactory = defaultValueFactory;
    }

    public Dictionary<TKey, TValue> Inner => mDict;

    public Dictionary<TKey, TValue> .KeyCollection Keys => mDict.Keys;

    public TValue this[TKey key]
    {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    private void SetValue(TKey key, TValue value)
    {
        mDict[key] = value;
    }

    private TValue GetValue(TKey key)
    {
        if (mDict.ContainsKey(key))
        {
            return mDict[key];
        }
        var result = mDefaultValueFactory();
        mDict[key] = result;
        return result;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return mDict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return mDict.GetEnumerator();
    }

}
