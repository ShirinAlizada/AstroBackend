namespace AstroBackend.Application.DTOs
{
    /// <summary>Brauzerin PushSubscription.toJSON()-unun "keys" hissəsi.</summary>
    public record PushKeysDto(string P256dh, string Auth);

    /// <summary>Brauzerin PushSubscription.toJSON() çıxışı ilə eyni şəkil (endpoint + keys).</summary>
    public record PushSubscribeRequest(string Endpoint, PushKeysDto Keys);

    public record PushUnsubscribeRequest(string Endpoint);
}
