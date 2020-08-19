using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UHub.Web.Config;
using Microsoft.Extensions.DependencyInjection;
using UHub.Web.Tasks;
using Newtonsoft.Json;
using UHub.Data;
using UHub.Data.Models;

namespace UHub.Web.BackgroundServices
{
    public class AppNoticeTaskBackgroundService : TimeBackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly UHubOptions _options;

        public AppNoticeTaskBackgroundService(IServiceScopeFactory serviceScopeFactory, IOptions<UHubOptions> options)
        {
            // 注意: 不能直接注入 DbContext, 而是注入 IServiceScopeFactory，通过CreateScope，在范围内获取DbContext
            _serviceScopeFactory = serviceScopeFactory;
            _options = options.Value;
            _timerPeriod = TimeSpan.FromSeconds(options.Value.SyncRate);
        }

        protected override void DoWork(object state)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                try
                {

                    #region 执行应用通信任务
                    // 遍历 TaskQueue 表，执行其中的通信任务
                    IList<TaskQueue> taskQueues = dbContext.TaskQueue.Include(m => m.TaskInfo)
                        .Where(m =>
                            m.TaskState != TaskStateEnum.CompletedAndSuccess
                            && m.ExecCount < _options.TaskMaxExecCount
                            && m.ExpireTime > DateTime.UtcNow
                            && m.TaskInfo.TaskType == nameof(AppNoticeTask)
                           )
                        .OrderBy(m => m.ID).ToList();
                    IList<AppInfo> appInfos = dbContext.AppInfo.ToList();

                    foreach (var taskQueue in taskQueues)
                    {
                        TaskInfo taskInfo = taskQueue.TaskInfo;
                        AppNoticeTaskDataModel dataModel = JsonConvert.DeserializeObject<AppNoticeTaskDataModel>(taskQueue.TaskData);
                        // 根据 AppId 查出应用 baseUrl
                        AppInfo appInfo = appInfos.Where(m => m.ID == dataModel.AppId).FirstOrDefault();
                        if (appInfo == null)
                        {
                            // 此 应用已不存在
                            continue;
                        }
                        string appBaseUrl = appInfo?.BaseUrl;
                        var taskParameterModel = new AppNoticeTaskParameterModel()
                        {
                            AppUrl = appBaseUrl + "/" + taskInfo.Name,
                            AppSecret = appInfo.AppSecret,
                            PostData = dataModel.PostData
                        };
                        // 执行通信任务 - > 是否成功
                        bool isSuccess = taskParameterModel.Execute();

                        taskQueue.ExecCount = taskQueue.ExecCount + 1;
                        taskQueue.LastUpdateTime = DateTime.UtcNow;

                        if (isSuccess)
                        {
                            taskQueue.SuccessTime = DateTime.UtcNow;
                            taskQueue.TaskState = TaskStateEnum.CompletedAndSuccess;
                        }
                        else
                        {
                            taskQueue.TaskState = TaskStateEnum.CompletedAndFailure;
                        }
                        dbContext.TaskQueue.Update(taskQueue);
                        dbContext.SaveChanges();
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    Console.WriteLine("数据库未创建, 任务暂时不会执行");
                }

            }
        }

    }
}
