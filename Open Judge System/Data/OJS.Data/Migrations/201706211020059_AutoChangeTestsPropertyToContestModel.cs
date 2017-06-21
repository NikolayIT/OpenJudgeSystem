namespace OJS.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AutoChangeTestsPropertyToContestModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Contests", "AutoChangeTests", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Contests", "AutoChangeTests", c => c.Boolean());
        }
    }
}
