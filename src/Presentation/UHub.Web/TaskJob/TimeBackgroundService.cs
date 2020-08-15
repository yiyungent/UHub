using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UHub.Web.Config;
using UHub.Web.Data;
using UHub.Web.Models;

namespace UHub.Web.TaskJob
{
    public class TimeBackgroundService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly ApplicationDbContext _dbContext;
        private readonly UHubOptions _options;

        public TimeBackgroundService(ApplicationDbContext dbContext, IOptions<UHubOptions> options)
        {
            _dbContext = dbContext;
            _options = options.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(_options.SyncRate));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            // TODO: 遍历 TaskQueue 表，执行消息任务
            IList<TaskQueue> taskQueues = _dbContext.TaskQueue.Include(m => m.TaskInfo)
                .Where(m =>
                    m.TaskState != TaskStateEnum.CompletedAndSuccess
                    && m.ExecCount < _options.TaskMaxExecCount
                    && m.ExpireTime > DateTime.UtcNow
                   )
                .OrderBy(m => m.ID).ToList();

            foreach (var taskQueue in taskQueues)
            {
                bool isSuccess = taskQueue.Execute();
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
                _dbContext.TaskQueue.Update(taskQueue);
                _dbContext.SaveChanges();
            }

        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

    }
}
