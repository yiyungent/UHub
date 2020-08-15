using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UHub.Web.Models
{
    /// <summary>
    /// 消息任务
    /// UHub -> 各个子应用
    /// </summary>
    public class TaskInfo
    {
        [Key]
        public int ID { get; set; }

        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(500)]
        public string DisplayName { get; set; }

        /// <summary>
        /// 描述/摘要
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }

    }
}
