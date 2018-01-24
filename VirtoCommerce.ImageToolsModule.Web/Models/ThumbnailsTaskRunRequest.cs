using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtoCommerce.ImageToolsModule.Web.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ThumbnailsTaskRunRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string[] TaskIds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Regenerate { get; set; }
    }
}