namespace Otc.PubSub.Abstractions
{
    public interface IMessageCoordinates
    {
        byte[] Serialize();
    }
}