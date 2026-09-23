namespace Buckl.Application.Common;

/// <summary>One field of a partial update. <c>default</c> means "leave the field as it is"; a
/// value, including <c>null</c>, means "set the field to this". Needed because a JSON body can
/// omit a property or send it as null, and those mean different things.</summary>
/// <remarks><see cref="Value"/> is meaningful only when <see cref="IsSet"/> is true.</remarks>
public readonly record struct FieldUpdate<T>
{
    public FieldUpdate(T value)
    {
        Value = value;
        IsSet = true;
    }

    public bool IsSet { get; }

    public T Value { get; }
}
