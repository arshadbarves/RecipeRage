namespace KitchenClash.Domain
{
    public interface IConfigModel
    {
        bool IsValid();
        bool Validate() => IsValid();
    }
}
