namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using OJS.Workers.Compilers;
    using OJS.Workers.Tools.AntiCheat;
    using OJS.Workers.Tools.AntiCheat.Contracts;
    using OJS.Workers.Tools.Similarity;

    using SimpleInjector;
    using SimpleInjector.Packaging;

    public class ToolsPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.Register<ISimilarityFinder, SimilarityFinder>(Lifestyle.Scoped);
            container.Register<IPlagiarismDetectorFactory, PlagiarismDetectorFactory>(Lifestyle.Scoped);

            container.Register(
                () => new CSharpCompiler(
                    Settings.CSharpCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);

            container.Register(
                () => new JavaCompiler(
                    Settings.JavaCompilerProcessExitTimeOutMultiplier),
                Lifestyle.Scoped);
        }
    }
}