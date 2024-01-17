using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Api.Models.Common;

namespace Nop.Api.Models.Profile
{
    public partial record ProfilePostsModel : BaseNopModel
    {
        public IList<PostsModel> Posts { get; set; }
        public PagerModel PagerModel { get; set; }
    }
}