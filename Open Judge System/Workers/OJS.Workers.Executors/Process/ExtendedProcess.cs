namespace OJS.Workers.Executors.Process
{
    public class ExtendedProcess : System.Diagnostics.Process
    {
        public ExtendedProcess()
        {
            this.IsDisposed = false;
        }

        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            this.IsDisposed = disposing;

            base.Dispose(disposing);
        }
    }
}
