using Newtonsoft.Json;

namespace CIDashboard.Domain.Extensions
{
    public static class JsonSerialize
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        }
    }
}
