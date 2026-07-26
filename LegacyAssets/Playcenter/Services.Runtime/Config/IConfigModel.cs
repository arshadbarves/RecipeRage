namespace Playcenter.Services
{
    public interface IConfigModel
    {
        bool IsValid();
        bool Validate() => IsValid();
    }
}
