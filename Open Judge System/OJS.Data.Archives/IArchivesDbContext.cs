namespace OJS.Data.Archives
{
    using System;
    using System.Data.Entity;

    using OJS.Data.Models;

    public interface IArchivesDbContext : IDisposable
    {
        IDbSet<ArchivedSubmission> Submissions { get; }

        DbContext DbContext { get; }
    }
}