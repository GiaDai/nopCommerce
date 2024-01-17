using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Api.Models.Common;

namespace Nop.Api.Models.PrivateMessages
{
    public partial record PrivateMessageListModel : BaseNopModel
    {
        public IList<PrivateMessageModel> Messages { get; set; }
        public PagerModel PagerModel { get; set; }
    }
}