namespace Impact.Core.Interfaces
{
    public interface IAbsorbable<T>
    {
        bool AbsorbHours(T otherObject, string propertyName);
    }
}