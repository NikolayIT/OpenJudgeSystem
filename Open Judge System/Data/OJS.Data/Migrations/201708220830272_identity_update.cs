namespace OJS.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class identity_update : DbMigration
    {
        public override void Up()
        {
            this.RenameColumn(table: "dbo.AspNetUserClaims", name: "User_Id", newName: "UserId");
            this.RenameIndex(table: "dbo.AspNetUserClaims", name: "IX_User_Id", newName: "IX_UserId");
            this.DropPrimaryKey("dbo.AspNetUserLogins");
            this.AddColumn("dbo.AspNetUsers", "EmailConfirmed", c => c.Boolean(nullable: false));
            this.AddColumn("dbo.AspNetUsers", "PhoneNumber", c => c.String());
            this.AddColumn("dbo.AspNetUsers", "PhoneNumberConfirmed", c => c.Boolean(nullable: false));
            this.AddColumn("dbo.AspNetUsers", "TwoFactorEnabled", c => c.Boolean(nullable: false));
            this.AddColumn("dbo.AspNetUsers", "LockoutEndDateUtc", c => c.DateTime());
            this.AddColumn("dbo.AspNetUsers", "LockoutEnabled", c => c.Boolean(nullable: false));
            this.AddColumn("dbo.AspNetUsers", "AccessFailedCount", c => c.Int(nullable: false));
            this.AlterColumn("dbo.AspNetUsers", "UserName", c => c.String(nullable: false, maxLength: 256, unicode: false));
            this.AlterColumn("dbo.AspNetUsers", "Email", c => c.String(nullable: false, maxLength: 256, unicode: false));
            this.AlterColumn("dbo.AspNetUsers", "IsGhostUser", c => c.Boolean(nullable: false));
            this.AlterColumn("dbo.AspNetUsers", "IsDeleted", c => c.Boolean(nullable: false));
            this.AlterColumn("dbo.AspNetUsers", "CreatedOn", c => c.DateTime(nullable: false));
            this.AlterColumn("dbo.AspNetRoles", "Name", c => c.String(nullable: false, maxLength: 256));
            this.AddPrimaryKey("dbo.AspNetUserLogins", new[] { "LoginProvider", "ProviderKey", "UserId" });
            this.CreateIndex("dbo.AspNetUsers", "UserName", unique: true, name: "UserNameIndex");
            this.CreateIndex("dbo.AspNetRoles", "Name", unique: true, name: "RoleNameIndex");
            this.DropColumn("dbo.AspNetUsers", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropPrimaryKey("dbo.AspNetUserLogins");
            AlterColumn("dbo.AspNetRoles", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.AspNetUsers", "CreatedOn", c => c.DateTime());
            AlterColumn("dbo.AspNetUsers", "IsDeleted", c => c.Boolean());
            AlterColumn("dbo.AspNetUsers", "IsGhostUser", c => c.Boolean());
            AlterColumn("dbo.AspNetUsers", "Email", c => c.String(maxLength: 80, unicode: false));
            AlterColumn("dbo.AspNetUsers", "UserName", c => c.String(nullable: false, maxLength: 50, unicode: false));
            DropColumn("dbo.AspNetUsers", "AccessFailedCount");
            DropColumn("dbo.AspNetUsers", "LockoutEnabled");
            DropColumn("dbo.AspNetUsers", "LockoutEndDateUtc");
            DropColumn("dbo.AspNetUsers", "TwoFactorEnabled");
            DropColumn("dbo.AspNetUsers", "PhoneNumberConfirmed");
            DropColumn("dbo.AspNetUsers", "PhoneNumber");
            DropColumn("dbo.AspNetUsers", "EmailConfirmed");
            AddPrimaryKey("dbo.AspNetUserLogins", new[] { "UserId", "LoginProvider", "ProviderKey" });
            RenameIndex(table: "dbo.AspNetUserClaims", name: "IX_UserId", newName: "IX_User_Id");
            RenameColumn(table: "dbo.AspNetUserClaims", name: "UserId", newName: "User_Id");
        }
    }
}
