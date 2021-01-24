using System;
using System.Collections.Generic;
using System.Text;

namespace UHubApi.AspNetCore
{
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
}
