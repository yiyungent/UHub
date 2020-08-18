using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UHub.Web.Data;
using UHub.Web.Models;

namespace UHub.Web.Controllers
{
    public class TestAddTaskQueueController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public TestAddTaskQueueController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(string par)
        {
            TaskInfo taskInfo = _dbContext.TaskInfo.First();
            _dbContext.TaskQueue.Add(new TaskQueue
            {
                CreateTime = DateTime.UtcNow,
                LastUpdateTime = DateTime.Now,
                ExpireTime = DateTime.UtcNow.AddDays(1),
                TaskInfo = taskInfo,
                TaskParameter = par
            });
            _dbContext.SaveChanges();

            return Json("添加成功: " + par);
        }
    }
}
