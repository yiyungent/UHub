using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UHub.Core;
using UHub.Web.Models;

namespace UHub.Web.TaskJob
{
    public static class TaskQueueExtension
    {
        /// <summary>
        /// 执行此任务
        /// </summary>
        /// <param name="taskQueue"></param>
        /// <returns></returns>
        public static bool Execute(this TaskQueue taskQueue)
        {
            bool isSuccess = false;
            try
            {
                // TODO: 执行具体消息任务
                TaskInfo taskInfo = taskQueue.TaskInfo;
                //taskQueue.TaskParameter;

                Console.WriteLine($"执行任务: {taskInfo.DisplayName} {taskQueue.TaskParameter}");



                isSuccess = true;
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        private static void SwitchAction(string taskName, string taskPar)
        {


        }
    }
}
