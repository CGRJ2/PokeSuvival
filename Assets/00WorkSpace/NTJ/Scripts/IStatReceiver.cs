namespace NTJ
{
    public interface IStatReceiver
    {
        void ApplyStat(ItemData item);
        void RemoveStat(ItemData item);
    }
}