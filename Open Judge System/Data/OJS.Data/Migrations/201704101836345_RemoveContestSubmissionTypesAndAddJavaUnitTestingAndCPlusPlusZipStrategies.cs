namespace OJS.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveContestSubmissionTypesAndAddJavaUnitTestingAndCPlusPlusZipStrategies : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SubmissionTypeContests", "SubmissionType_Id", "dbo.SubmissionTypes");
            DropForeignKey("dbo.SubmissionTypeContests", "Contest_Id", "dbo.Contests");
            DropIndex("dbo.SubmissionTypeContests", new[] { "SubmissionType_Id" });
            DropIndex("dbo.SubmissionTypeContests", new[] { "Contest_Id" });
            DropTable("dbo.SubmissionTypeContests");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SubmissionTypeContests",
                c => new
                    {
                        SubmissionType_Id = c.Int(nullable: false),
                        Contest_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SubmissionType_Id, t.Contest_Id });
            
            CreateIndex("dbo.SubmissionTypeContests", "Contest_Id");
            CreateIndex("dbo.SubmissionTypeContests", "SubmissionType_Id");
            AddForeignKey("dbo.SubmissionTypeContests", "Contest_Id", "dbo.Contests", "Id", cascadeDelete: true);
            AddForeignKey("dbo.SubmissionTypeContests", "SubmissionType_Id", "dbo.SubmissionTypes", "Id", cascadeDelete: true);
        }
    }
}
