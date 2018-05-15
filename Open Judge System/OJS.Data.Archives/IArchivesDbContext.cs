namespace OJS.Data.Archives
{
    using System;
    using System.Data.Entity;

    using OJS.Data.Archives.Models;

    public interface IArchivesDbContext : IDisposable
    {
        IDbSet<ArchivedSubmission> Submissions { get; }

        DbContext DbContext { get; }
    }
}