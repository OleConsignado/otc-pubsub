using System.Threading.Tasks;

namespace Otc.PubSub.Abstractions
{
    public interface IMessageHandler
    {
        Task OnMessageAsync(IMessage message);
        Task OnErrorAsync(object error, IMessage message);
    }
}