using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Models
{
   public class ExternalLoginConfirmationViewModel
   {
      [Required]
      [Display(Name = "Email")]
      public string Email { get; set; }
   }

   public class ExternalLoginListViewModel
   {
      public string ReturnUrl { get; set; }
   }

   public class SendCodeViewModel
   {
      public string SelectedProvider { get; set; }
      public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
      public string ReturnUrl { get; set; }
   }

   public class VerifyCodeViewModel
   {
      [Required]
      public string Provider { get; set; }

      [Required]
      [Display(Name = "Code")]
      public string Code { get; set; }
      public string ReturnUrl { get; set; }

      [Display(Name = "Remember this browser?")]
      public bool RememberBrowser { get; set; }
   }

   public class ForgotViewModel
   {
      [Required]
      [Display(Name = "Email")]
      public string Email { get; set; }
   }

   public class LoginViewModel
   {
      
      [Required(ErrorMessage = "Entre com seu login")] //give your validation message here
      [Display(Name = "Login")]
      //[EmailAddress]
      public string Email { get; set; }

      [Required(ErrorMessage = "Entre com sua senha")] //give your validation message here
      [DataType(DataType.Password)]
      [Display(Name = "Password")]
      public string Password { get; set; }

      [Display(Name = "Lembrar?")]
      public bool RememberMe { get; set; }
   }

   public class RegisterViewModel
   {
      [Required(ErrorMessage = "Entre com seu login")] //give your validation message here
      //[EmailAddress]
      [Display(Name = "Login")]
      public string Login { get; set; }

      [Display(Name = "Email")]
      public string Email { get; set; }

      [Required]
      [StringLength(100, ErrorMessage = "{0} deve ser, pelo menos, {2} caracteres.", MinimumLength = 6)]
      [DataType(DataType.Password)]
      [Display(Name = "Password")]
      public string Password { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm password")]
      [Compare("Password", ErrorMessage = "A senha e a confirmação da senha não são iguais.")]
      public string ConfirmPassword { get; set; }
   }

   public class ResetPasswordViewModel
   {
      [Required]
      //[EmailAddress]
      [Display(Name = "Email")]
      public string Email { get; set; }

      [Required]
      [StringLength(100, ErrorMessage = "{0} deve ser, pelo menos, {2} caracteres.", MinimumLength = 6)]
      [DataType(DataType.Password)]
      [Display(Name = "Password")]
      public string Password { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm password")]
      [Compare("Password", ErrorMessage = "A senha e a confirmação da senha não são iguais.")]
      public string ConfirmPassword { get; set; }

      public string Code { get; set; }
   }

   public class ForgotPasswordViewModel
   {
      [Required]
      //[EmailAddress]
      [Display(Name = "Email")]
      public string Email { get; set; }
   }
}