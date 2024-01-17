using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;

namespace Nop.Api.Models.Newsletter
{
    public partial record NewsletterBoxModel : BaseNopModel
    {
        [DataType(DataType.EmailAddress)]
        public string NewsletterEmail { get; set; }
        public bool AllowToUnsubscribe { get; set; }
    }
}