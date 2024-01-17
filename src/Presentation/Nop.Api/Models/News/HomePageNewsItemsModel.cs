using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Api.Models.News
{
    public partial record HomepageNewsItemsModel : BaseNopModel
    {
        public HomepageNewsItemsModel()
        {
            NewsItems = new List<NewsItemModel>();
        }

        public int WorkingLanguageId { get; set; }
        public IList<NewsItemModel> NewsItems { get; set; }
    }
}