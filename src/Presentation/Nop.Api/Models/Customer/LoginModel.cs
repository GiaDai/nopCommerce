using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Api.Models.Customer
{
    public class LoginModel
    {
        [NopResourceDisplayName("Account.Login.Fields.Username")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Login.Fields.Password")]
        public string Password { get; set; }
    }
}
