namespace Sistema.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adicaogoogleauth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Autenticacao", "IsGoogleAuthenticatorEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Autenticacao", "GoogleAuthenticatorSecretKey", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Autenticacao", "GoogleAuthenticatorSecretKey");
            DropColumn("dbo.Autenticacao", "IsGoogleAuthenticatorEnabled");
        }
    }
}
