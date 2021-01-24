using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace UHubApi.AspNetCore
{
    public class AspNetCoreJsonHelper 
    {
        public JsonSerializerOptions JsonSerializerOptions { get; set; }

        public AspNetCoreJsonHelper()
        {
            this.JsonSerializerOptions = new JsonSerializerOptions();
            // 属性名大小写不敏感
            this.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        }

        public T Deserialize<T>(string jsonStr)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(jsonStr, JsonSerializerOptions);
        }

        public string Serialize(object jsonObj)
        {
            return System.Text.Json.JsonSerializer.Serialize(jsonObj);
        }
    }
}
