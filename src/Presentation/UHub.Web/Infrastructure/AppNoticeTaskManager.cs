using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UHub.Data;
using UHub.Data.Models;
using UHub.Web.Api;
using UHub.Web.Config;
using UHub.Web.Tasks;

namespace UHub.Web.Infrastructure
{
    /// <summary>
    /// 应用通信(UHub --消息--> 所有应用) 任务管理器
    /// </summary>
    public class AppNoticeTaskManager
    {
        #region Fields
        private readonly ApplicationDbContext _dbContext;
        private readonly UHubOptions _options;
        #endregion

        #region Ctor
        public AppNoticeTaskManager(ApplicationDbContext dbContext, IOptions<UHubOptions> options)
        {
            _dbContext = dbContext;
            _options = options.Value;
        }
        #endregion

        #region Methods

        /// <summary>
        /// 添加通信应用任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="excludeAppId"></param>
        /// <returns></returns>
        public bool Add<T>(T model, int excludeAppId = 0)
            where T : AppNoticePostDataModel
        {
            bool isSuccess = true;
            string postDataJsonStr = JsonConvert.SerializeObject(model);
            // 此种 通信任务 - API 类型
            // 注意: 通信API Name 应与 类型名称相同
            // 通信API: UHub.Web.Api.xxxModel
            string taskInfoName = nameof(T).Replace("Model", "");
            TaskInfo taskInfo = _dbContext.TaskInfo.FirstOrDefault(m => m.Name == taskInfoName && m.TaskType == AppNoticeTask.TaskType);
            if (taskInfo == null)
            {
                return false;
            }

            List<AppInfo> appInfos = _dbContext.AppInfo.Where(m => m.ID != excludeAppId).ToList();
            foreach (var app in appInfos)
            {
                AppNoticeTaskDataModel taskData = new AppNoticeTaskDataModel
                {
                    AppId = app.ID,
                    PostData = postDataJsonStr
                };
                string taskDataJsonStr = JsonConvert.SerializeObject(taskData);
                TaskQueue taskQueue = new TaskQueue
                {
                    TaskInfo = taskInfo,
                    CreateTime = DateTime.UtcNow,
                    LastUpdateTime = DateTime.UtcNow,
                    ExecCount = 0,
                    ExpireTime = DateTime.UtcNow.AddMinutes(_options.TaskExpireAfter),
                    TaskState = TaskStateEnum.WithoutExec,
                    TaskData = taskDataJsonStr
                };
                _dbContext.TaskQueue.Add(taskQueue);
            }

            _dbContext.SaveChanges();

            return isSuccess;
        }

        #endregion
    }
}
