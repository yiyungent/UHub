using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace UHubApi.AspNetCore
{
    public abstract class UHubApiMiddleware<T>
    {
        protected readonly RequestDelegate _next;

        protected readonly UHubApiOptions _options;

        protected readonly IUHubApiApp _uHubApiApp;

        public UHubApiMiddleware(RequestDelegate next, IOptions<UHubApiOptions> optionsAccessor, IUHubApiApp uHubApiApp)
        {
            _next = next;
            _options = optionsAccessor.Value;
            _uHubApiApp = uHubApiApp;
        }

        public virtual async Task InvokeAsync(HttpContext context)
        {
            AspNetCoreJsonHelper jsonHelper = new AspNetCoreJsonHelper();

            // 1. 效验通信秘钥
            Microsoft.Extensions.Primitives.StringValues authorHeader = context.Request.Headers["Authorization"];
            string encryptInfo = authorHeader[1];
            encryptInfo = EncryptHelper.Decrypt(encryptInfo, _options.AppSecret);
            if (encryptInfo != "AppNotice")
            {
                // 通信秘钥错误
                Console.WriteLine($"通信秘钥错误: Authorization: {encryptInfo.ToString()}");

                AppNoticeResponseModel responseModel = new AppNoticeResponseModel()
                {
                    Code = AppNoticeResponseModel.CodeEnum.Failure,
                    Message = "通信密钥错误"
                };

                string responseJsonStr = jsonHelper.Serialize(responseModel);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(responseJsonStr, Encoding.UTF8);
            }
            else
            {
                // 通信秘钥正确
                // 2. 解密通信内容
                string inputBody;
                using (var reader = new System.IO.StreamReader(
                    context.Request.Body, Encoding.UTF8))
                {
                    inputBody = await reader.ReadToEndAsync();
                }

                // 对 inputBody 解密
                inputBody = EncryptHelper.Decrypt(inputBody, _options.AppSecret);

                Console.WriteLine($"通信秘钥正确: inputBody: {inputBody}");

                T model = jsonHelper.Deserialize<T>(inputBody);
                // 3. 调用实现的接口
                AppNoticeResponseModel responseModel = ExecuteApi(model);
                string responseJsonStr = jsonHelper.Serialize(responseModel);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(responseJsonStr, Encoding.UTF8);
            }
        }

        public abstract AppNoticeResponseModel ExecuteApi(T model);
    }
}
