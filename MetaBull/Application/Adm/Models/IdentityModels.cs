using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;


namespace Sistema.Models
{
   public class ApplicationUserLogin : IdentityUserLogin<int> { }
   public class ApplicationUserClaim : IdentityUserClaim<int> { }
   public class ApplicationUserRole : IdentityUserRole<int> { }

   public class ApplicationRole : IdentityRole<int, ApplicationUserRole>, IRole<int>
   {
      public string Description { get; set; }

      public ApplicationRole() : base() { }
      public ApplicationRole(string name)
         : this()
      {
         this.Name = name;
      }

      public ApplicationRole(string name, string description)
         : this(name)
      {
         this.Description = description;
      }
   }

   public class ApplicationUser : IdentityUser<int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>, IUser<int>
   {
        public bool IsGoogleAuthenticatorEnabled { get; set; }
        public string GoogleAuthenticatorSecretKey { get; set; }

        public async Task<ClaimsIdentity>
          GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
      {
         var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
         return userIdentity;
      }
   }

   public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
   {
      public ApplicationDbContext()
         : base("DefaultConnection")
      {
      }

      static ApplicationDbContext()
      {
            //Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public static ApplicationDbContext Create()
      {
         return new ApplicationDbContext();
      }

      // Add the AutenticacaoGrupos property:
      public virtual IDbSet<AutenticacaoGrupo> AutenticacaoGrupos { get; set; }

      // Override OnModelsCreating:
      protected override void OnModelCreating(DbModelBuilder modelBuilder)
      {

         // Make sure to call the base method first:
         base.OnModelCreating(modelBuilder);

         modelBuilder.Entity<ApplicationUser>().Ignore(x => x.PhoneNumber);
         modelBuilder.Entity<ApplicationUser>().Ignore(x => x.PhoneNumberConfirmed);

         modelBuilder.Entity<ApplicationUser>().ToTable("Autenticacao");
         modelBuilder.Entity<ApplicationRole>().ToTable("AutenticacaoRegra");
         modelBuilder.Entity<ApplicationUserRole>().ToTable("AutenticacaoUsuarioRegra");
         modelBuilder.Entity<ApplicationUserLogin>().ToTable("AutenticacaoUsuarioLogin");
         modelBuilder.Entity<ApplicationUserClaim>().ToTable("AutenticacaoUsuarioPermissao");

         // Map Users to Groups:
         modelBuilder.Entity<AutenticacaoGrupo>()
             .HasMany<AutenticacaoUsuarioGrupo>((AutenticacaoGrupo g) => g.ApplicationUsers)
             .WithRequired()
             .HasForeignKey<int>((AutenticacaoUsuarioGrupo ag) => ag.AutenticacaoGrupoId);

         modelBuilder.Entity<AutenticacaoUsuarioGrupo>()
             .HasKey((AutenticacaoUsuarioGrupo r) =>
                 new
                 {
                    ApplicationUserId = r.ApplicationUserId,
                    AutenticacaoGrupoId = r.AutenticacaoGrupoId
                 }).ToTable("AutenticacaoUsuarioGrupo");

         // Map Roles to Groups:
         modelBuilder.Entity<AutenticacaoGrupo>()
             .HasMany<AutenticacaoGrupoRegra>((AutenticacaoGrupo g) => g.ApplicationRoles)
             .WithRequired()
             .HasForeignKey<int>((AutenticacaoGrupoRegra ap) => ap.AutenticacaoGrupoId);

         modelBuilder.Entity<AutenticacaoGrupoRegra>().HasKey((AutenticacaoGrupoRegra gr) =>
             new
             {
                ApplicationRoleId = gr.ApplicationRoleId,
                AutenticacaoGrupoId = gr.AutenticacaoGrupoId
             }).ToTable("AutenticacaoGrupoRegra");

         modelBuilder.Entity<AutenticacaoGrupo>().ToTable("AutenticacaoGrupo");

      }

   }

   public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>, IUserStore<ApplicationUser, int>, IDisposable
   {
      public ApplicationUserStore()
         : this(new IdentityDbContext())
      {
         base.DisposeContext = true;
      }

      public ApplicationUserStore(DbContext context)
         : base(context)
      {
      }
   }

   public class ApplicationRoleStore : RoleStore<ApplicationRole, int, ApplicationUserRole>, IQueryableRoleStore<ApplicationRole, int>, IRoleStore<ApplicationRole, int>, IDisposable
   {
      public ApplicationRoleStore()
         : base(new IdentityDbContext())
      {
         base.DisposeContext = true;
      }

      public ApplicationRoleStore(DbContext context)
         : base(context)
      {
      }
   }

   //Grupos

   public class AutenticacaoGrupo
   {
      public AutenticacaoGrupo()
      {
         //this.Id = Guid.NewGuid().ToString();
         this.ApplicationRoles = new List<AutenticacaoGrupoRegra>();
         this.ApplicationUsers = new List<AutenticacaoUsuarioGrupo>();
      }

      public AutenticacaoGrupo(string name)
         : this()
      {
         this.Name = name;
      }

      public AutenticacaoGrupo(string name, string description)
         : this(name)
      {
         this.Description = description;
      }

      [Key]
      public int Id { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public virtual ICollection<AutenticacaoGrupoRegra> ApplicationRoles { get; set; }
      public virtual ICollection<AutenticacaoUsuarioGrupo> ApplicationUsers { get; set; }
   }

   public class AutenticacaoUsuarioGrupo
   {
      public int ApplicationUserId { get; set; }
      public int AutenticacaoGrupoId { get; set; }
   }

   public class AutenticacaoGrupoRegra
   {
      public int AutenticacaoGrupoId { get; set; }
      public int ApplicationRoleId { get; set; }
   }

}