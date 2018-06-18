namespace Otc.PubSub.Abstractions.Exceptions
{
    public class ReadException : PubSubException
    {
        public ReadException(object error, string message)
            : base(error, message)
        { }

        public ReadException(object error) 
            : this(error, "Error on ReadFromParticularCoordinatesException.")
        {
        }

        public ReadException()
            : this(null)
        {

        }
    }
}
