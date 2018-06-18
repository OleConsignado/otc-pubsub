using System.ComponentModel.DataAnnotations;

namespace Otc.PubSub.Kafka
{
    public class KafkaPubSubConfiguration
    {
        [Required]
        public string BrokerList { get; set; }
    }
}