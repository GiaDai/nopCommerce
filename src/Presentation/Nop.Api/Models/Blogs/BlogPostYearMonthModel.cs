﻿using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Api.Models.Blogs
{
    public partial record BlogPostYearModel : BaseNopModel
    {
        public BlogPostYearModel()
        {
            Months = new List<BlogPostMonthModel>();
        }
        public int Year { get; set; }
        public IList<BlogPostMonthModel> Months { get; set; }
    }

    public partial record BlogPostMonthModel : BaseNopModel
    {
        public int Month { get; set; }

        public int BlogPostCount { get; set; }
    }
}