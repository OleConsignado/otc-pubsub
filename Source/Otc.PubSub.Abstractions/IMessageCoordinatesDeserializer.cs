namespace Otc.PubSub.Abstractions
{
    public interface IMessageCoordinatesDeserializer
    {
        /// <exception cref="System.ArgumentException" />
        IMessageCoordinates Deserialize(byte[] data);
    }
}
