namespace Impact.Business.Strategies
{
    public interface IMoveHoursStrategy<in T>
    {
        bool MoveHours(T lowHoursElement, T movableHoursElement);
    }
}