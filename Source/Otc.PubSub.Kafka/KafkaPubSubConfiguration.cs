using System.ComponentModel.DataAnnotations;

namespace Otc.PubSub.Kafka
{
    public class KafkaPubSubConfiguration
    {
        [Required]
        public string BrokerList { get; set; }
        [Required]
        public string AutoOffsetReset { get; set; } = "latest";
    }
}