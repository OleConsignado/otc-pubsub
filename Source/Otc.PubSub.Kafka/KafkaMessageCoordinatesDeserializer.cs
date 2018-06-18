using Otc.PubSub.Abstractions;
using System;
using System.Text;

namespace Otc.PubSub.Kafka
{
    public class KafkaMessageCoordinatesDeserializer : IMessageCoordinatesDeserializer
    {
        public IMessageCoordinates Deserialize(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string text = Encoding.UTF8.GetString(data);

            var pieces = text.Split('|');

            if(pieces.Length < 3)
            {
                throw new ArgumentException($"Could not deserialize '{text}'. Expected format is 'topic|partition|offset'", nameof(data));
            }

            string topic = pieces[0];
            int partition;
            long offset;

            try
            {
                partition = Convert.ToInt32(pieces[1]);
                offset = Convert.ToInt64(pieces[2]);
            }
            catch (Exception e)
            {
                if (e is FormatException || e is OverflowException)
                {
                    throw new ArgumentException($"Could not deserialize '{text}', see innerException.", nameof(data), e);
                }
                else
                {
                    throw;
                }
            }

            return new KafkaMessageCoordinates(topic, partition, offset);
        }
    }
}
