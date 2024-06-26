﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Sistema.Models
{
   public class RoleViewModel
   {
      public int Id { get; set; }
      [Required(AllowEmptyStrings = false)]
      [Display(Name = "RoleName")]
      public string Name { get; set; }
   }

   public class EditUserViewModel
   {
      public EditUserViewModel()
      {
         this.RolesList = new List<SelectListItem>();
         this.GroupsList = new List<SelectListItem>();
      }
      public int Id { get; set; }

      [Required(AllowEmptyStrings = false)]
      [Display(Name = "Email")]
      [EmailAddress]
      public string Email { get; set; }

      public IEnumerable<SelectListItem> RolesList { get; set; }

      // Add a GroupsList Property:
      public ICollection<SelectListItem> GroupsList { get; set; }

   }


   public class GroupViewModel
   {
      public GroupViewModel()
      {
         this.UsersList = new List<SelectListItem>();
         this.RolesList = new List<SelectListItem>();
      }
      [Required(AllowEmptyStrings = false)]
      public int Id { get; set; }
      [Required(AllowEmptyStrings = false)]
      public string Name { get; set; }
      public string Description { get; set; }
      public ICollection<SelectListItem> UsersList { get; set; }
      public ICollection<SelectListItem> RolesList { get; set; }
   }

}