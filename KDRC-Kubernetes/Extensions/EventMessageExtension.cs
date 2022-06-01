using k8s.Models;
using KDRC_Models.EventMessages.Account;

namespace KDRC_Kubernetes.Extensions;

public static class EventMessageExtension
{
    public static V1Namespace ToNamespaceObject(this AccountCreatedMessage message)
    {
        return new V1Namespace
        {
            Kind = "Namespace",
            ApiVersion = "v1",
            Metadata = new V1ObjectMeta
            {
                Name = $"{message.AccountId.ToLower()}-default"
            }
        };
    }
}