#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace burtonrodman.AspNetCore.DynamicRouting;

public class MutableEndpointDataSource : EndpointDataSource {
    private readonly Lock _lock = new Lock();

    private IReadOnlyList<Endpoint> _endpoints;

    private CancellationTokenSource _cancellationTokenSource;

    private IChangeToken _changeToken;

    public MutableEndpointDataSource() : this(Array.Empty<Endpoint>()) { }

    public MutableEndpointDataSource(IReadOnlyList<Endpoint> endpoints) {
        SetEndpoints(endpoints);
    }

    public override IChangeToken GetChangeToken() => _changeToken;

    public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

    public void SetEndpoints(IReadOnlyList<Endpoint> endpoints) {
        lock (_lock) {
            var oldCancellationTokenSource = _cancellationTokenSource;

            _endpoints = endpoints;

            _cancellationTokenSource = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);

            oldCancellationTokenSource?.Cancel();
        }
    }
}
