using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UHub.Core;
using UHub.Web.Config;

namespace UHub.Web.Tasks
{
    /// <summary>
    /// 应用通知任务
    /// </summary>
    public static class AppNoticeTask
    {
        public static bool Execute(this AppNoticeTaskParameterModel model)
        {
            bool isSuccess = false;
            try
            {
                string apiUrl = model.AppUrl;
                // AppSecret 放在 Headers["Authorization"] = Bearer AppSecret
                string bearerToken = model.AppSecret;
                // TODO: 使用 AppSecret 作为 key 对 Data 进行 AES 加密，再放在 request body
                string postData = EncryptHelper.Encrypt(model.PostData, model.AppSecret);
                // HTTP POST
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                StringContent stringContent = new StringContent(postData, Encoding.UTF8, "application/json");
                string resJsonStr = null;
                try
                {
                    resJsonStr = httpClient.PostAsync(requestUri: apiUrl, stringContent).Result.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                { }
                AppNoticeResponseModel resModel = new AppNoticeResponseModel { Code = AppNoticeResponseModel.CodeEnum.Failure };
                if (!string.IsNullOrEmpty(resJsonStr))
                {
                    try
                    {
                        resModel = JsonConvert.DeserializeObject<AppNoticeResponseModel>(resJsonStr);
                    }
                    catch (Exception ex)
                    { }
                }
                if (resModel.Code == AppNoticeResponseModel.CodeEnum.Success)
                {
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return isSuccess;
        }
    }

    /// <summary>
    /// 执行应用通信任务所需参数模型
    /// </summary>
    public class AppNoticeTaskParameterModel
    {
        /// <summary>
        /// 目标API Url
        /// </summary>
        public string AppUrl { get; set; }

        /// <summary>
        /// 通信秘钥
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// json字符串
        /// 具体消息数据
        /// 加密后 放在 request body
        /// </summary>
        public string PostData { get; set; }
    }

    /// <summary>
    /// 通信响应结果json
    /// </summary>
    public class AppNoticeResponseModel
    {
        public CodeEnum Code { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public enum CodeEnum
        {
            Failure = 0,
            Success = 1,
        }
    }

    /// <summary>
    /// 存于 TaskQueue.TaskData 中的json实体化
    /// </summary>
    public class AppNoticeTaskDataModel
    {
        public int AppId { get; set; }

        /// <summary>
        /// json字符串
        /// 具体消息数据
        /// 加密后 放在 request body
        /// </summary>
        public string PostData { get; set; }
    }
}
