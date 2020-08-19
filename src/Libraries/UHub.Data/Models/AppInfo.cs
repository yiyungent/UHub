using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UHub.Data.Models
{
    /// <summary>
    /// 通信应用
    /// </summary>
    public class AppInfo
    {
        [Key]
        public int ID { get; set; }

        [StringLength(500)]
        public string DisplayName { get; set; }

        [StringLength(500)]
        public string BaseUrl { get; set; }

        [StringLength(500)]
        public string AppSecret { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
