using IntegrationResilience.Contracts;

namespace IntegrationResilience.Application;

public interface IResilienceObservability
{
    void Record(string correlationId, string kind, string detail);
    void SetCircuitState(string state, string detail);
    ResilienceStateResponse Read(string correlationId);
}
