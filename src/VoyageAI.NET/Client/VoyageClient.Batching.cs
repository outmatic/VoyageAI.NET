using System.Runtime.CompilerServices;

namespace VoyageAI;

public sealed partial class VoyageClient
{
    private static IAsyncEnumerable<TResponse> BatchAsync<TItem, TResponse>(
        IEnumerable<TItem> inputs,
        int batchSize,
        int maxConcurrency,
        Func<TItem[], CancellationToken, Task<TResponse>> send,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxConcurrency);

        return BatchCoreAsync(inputs, batchSize, maxConcurrency, send, cancellationToken);
    }

    private static async IAsyncEnumerable<TResponse> BatchCoreAsync<TItem, TResponse>(
        IEnumerable<TItem> inputs,
        int batchSize,
        int maxConcurrency,
        Func<TItem[], CancellationToken, Task<TResponse>> send,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var batchCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var batches = inputs.Chunk(batchSize).GetEnumerator();
        var pending = new Queue<Task<TResponse>>();

        bool EnqueueNextBatch()
        {
            batchCancellation.Token.ThrowIfCancellationRequested();
            if (!batches.MoveNext())
                return false;

            batchCancellation.Token.ThrowIfCancellationRequested();
            pending.Enqueue(send(batches.Current, batchCancellation.Token));
            return true;
        }

        try
        {
            for (var i = 0; i < maxConcurrency && EnqueueNextBatch(); i++) { }

            while (pending.Count > 0)
            {
                var result = await pending.Dequeue().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                yield return result;
                EnqueueNextBatch();
            }
        }
        finally
        {
            // A failed request or an early-stopping consumer must not leave requests running.
            await batchCancellation.CancelAsync().ConfigureAwait(false);
            try
            {
                await Task.WhenAll(pending).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Observe outstanding failures without replacing the original exception.
            }
        }
    }
}
