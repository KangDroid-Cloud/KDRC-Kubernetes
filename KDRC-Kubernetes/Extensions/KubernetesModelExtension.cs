using k8s.Models;

namespace KDRC_Kubernetes.Extensions;

public static class KubernetesModelExtension
{
    public static V1Role ToRole(this V1Namespace targetNamespace)
    {
        return new V1Role
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"{targetNamespace.Metadata.Name}-role"
            },
            Rules = new List<V1PolicyRule>
            {
                new V1PolicyRule
                {
                    ApiGroups = new List<string> {"*"},
                    Resources = new List<string> {"*"},
                    Verbs = new List<string> {"*"}
                }
            }
        };
    }
}