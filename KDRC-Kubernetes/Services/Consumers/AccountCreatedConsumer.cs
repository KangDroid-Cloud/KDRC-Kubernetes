using k8s;
using k8s.Models;
using KDRC_Kubernetes.Extensions;
using KDRC_Models.EventMessages.Account;
using MassTransit;

namespace KDRC_Kubernetes.Services.Consumers;

public class AccountCreatedConsumer: IConsumer<AccountCreatedMessage>
{
    private readonly IKubernetes _kubernetes;
    private readonly ILogger _logger;

    public AccountCreatedConsumer(IKubernetes kubernetes, ILogger<AccountCreatedConsumer> logger)
    {
        _kubernetes = kubernetes;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<AccountCreatedMessage> context)
    {
        _logger.LogInformation("Message consumed: {msg}, Message Id: {messageId}", context.Message.ToJsonString(), context.MessageId);
        // create namespace
        var namespaceResult = await _kubernetes.CoreV1.CreateNamespaceAsync(context.Message.ToNamespaceObject());
        
        // Create Role
        var roleResult =
            await _kubernetes.RbacAuthorizationV1.CreateNamespacedRoleAsync(namespaceResult.ToRole(),
                namespaceResult.Metadata.Name);
        
        // Create Service Account
        var serviceAccountResult = await _kubernetes.CoreV1.CreateNamespacedServiceAccountAsync(new V1ServiceAccount
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"{namespaceResult.Metadata.Name}-service-account"
            }
        }, namespaceResult.Metadata.Name);
        
        // Create RoleBinding
        var roleBinding = new V1RoleBinding
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"{namespaceResult.Metadata.Name}-role-binding"
            },
            Subjects = new List<V1Subject>
            {
                new()
                {
                    Kind = "ServiceAccount",
                    Name = $"{serviceAccountResult.Metadata.Name}",
                    ApiGroup = ""
                }
            },
            RoleRef = new V1RoleRef
            {
                Kind = "Role",
                Name = $"{roleResult.Metadata.Name}",
                ApiGroup = "rbac.authorization.k8s.io"
            }
        };
        var roleBindingResult =
            await _kubernetes.RbacAuthorizationV1.CreateNamespacedRoleBindingAsync(roleBinding,
                namespaceResult.Metadata.Name);
        _logger.LogInformation("Successfully handled message, messageId: {messageId}", context.MessageId);
    }
}