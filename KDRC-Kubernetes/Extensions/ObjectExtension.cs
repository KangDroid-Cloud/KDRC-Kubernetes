using Newtonsoft.Json;

namespace KDRC_Kubernetes.Extensions;

public static class ObjectExtension
{
    public static string ToJsonString(this object targetObject)
    {
        return JsonConvert.SerializeObject(targetObject);
    }
}