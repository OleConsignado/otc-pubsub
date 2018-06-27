using System;
using System.Threading;
using System.Threading.Tasks;

namespace Otc.PubSub.Abstractions
{
    public interface ISubscription
    {
        /// <summary>
        /// Unsbuscribe then subscribe to topics at a provided DateTimeOffset.
        /// <para>
        /// This is usefull if you ignore some messages (not commited) then you want to read it again later. 
        /// </para>
        /// </summary>
        /// <param name="time">
        /// <para>
        /// The moment to perform unsbscribe then subscribe operation. If you supply a later time compared to a previous suplied time, 
        /// it will be ignored. If you supply a early time in respect to previous one, it will be replaced.
        /// If a past time is provided, will reload immediately.
        /// </para>
        /// </param>
        void ReloadAt(DateTimeOffset time);

        /// <param name="cancellationToken" />
        /// <exception cref="OperationCanceledException">When cancellationToken has cancelled.</exception>
        Task StartAsync(CancellationToken cancellationToken);
    }
}
