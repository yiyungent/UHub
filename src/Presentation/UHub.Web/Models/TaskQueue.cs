using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UHub.Web.Models
{
    /// <summary>
    /// 消息任务队列
    /// UHub -> 各个子应用
    /// </summary>
    public class TaskQueue
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("TaskInfo")]
        public int TaskInfoId { get; set; }
        [ForeignKey("TaskInfoId")]
        public TaskInfo TaskInfo { get; set; }

        /// <summary>
        /// 此任务需要的参数
        /// 消息参数
        /// json字符串
        /// </summary>
        public string TaskParameter { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime ExpireTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 完成任务且成功的时间
        /// </summary>
        public DateTime SuccessTime { get; set; }

        /// <summary>
        /// 此任务已被执行多少次
        /// </summary>
        [Required]
        public int ExecCount { get; set; }

        //public bool IsCompleted { get; set; }

        [Required]
        public TaskStateEnum TaskState { get; set; } = TaskStateEnum.WithoutExec;
    }

    public enum TaskStateEnum
    {
        /// <summary>
        /// 没有执行过
        /// </summary>
        WithoutExec = 0,

        /// <summary>
        /// 已执行过且标记为成功
        /// </summary>
        CompletedAndSuccess = 1,

        /// <summary>
        /// 已执行过且标记为失败
        /// </summary>
        CompletedAndFailure = 2,
    }
}
