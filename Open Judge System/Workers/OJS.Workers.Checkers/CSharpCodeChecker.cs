namespace OJS.Workers.Checkers
{
    using System;
    using System.CodeDom.Compiler;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Text;

    using Microsoft.CSharp;

    using OJS.Workers.Common;

    public class CSharpCodeChecker : Checker
    {
        private const int CacheDays = 7;

        private static readonly TimeSpan CacheDaysSlidingExpiration = TimeSpan.FromDays(CacheDays);

        private readonly ObjectCache compiledCustomCheckersCache;

        private IChecker customChecker;

        public CSharpCodeChecker()
            : this(MemoryCache.Default)
        {
        }

        public CSharpCodeChecker(ObjectCache compiledCustomCheckersCache)
        {
            if (compiledCustomCheckersCache == null)
            {
                throw new ArgumentNullException(nameof(compiledCustomCheckersCache));
            }

            this.compiledCustomCheckersCache = compiledCustomCheckersCache;
        }

        public override CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
        {
            if (this.customChecker == null)
            {
                throw new InvalidOperationException("Please call SetParameter first with non-null string.");
            }

            var result = this.customChecker.Check(inputData, receivedOutput, expectedOutput, isTrialTest);
            return result;
        }

        public override void SetParameter(string parameter)
        {
            var customCheckerFromCache = this.compiledCustomCheckersCache[parameter] as IChecker;
            if (customCheckerFromCache != null)
            {
                this.customChecker = customCheckerFromCache;
                return;
            }

            var codeProvider = new CSharpCodeProvider();
            var compilerParameters = new CompilerParameters { GenerateInMemory = true, };
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            compilerParameters.ReferencedAssemblies.Add("OJS.Workers.Common.dll");
            var compilerResults = codeProvider.CompileAssemblyFromSource(compilerParameters, parameter);
            if (compilerResults.Errors.HasErrors)
            {
                var errorsStringBuilder = new StringBuilder();
                foreach (CompilerError error in compilerResults.Errors)
                {
                    errorsStringBuilder.AppendLine(error.ToString());
                }

                // TODO: Introduce class CompilerException and throw exception of this type
                throw new Exception(
                    string.Format(
                        "Could not compile checker!{0}Errors:{0}{1}",
                        Environment.NewLine,
                        errorsStringBuilder));
            }

            var assembly = compilerResults.CompiledAssembly;

            var types = assembly.GetTypes().Where(x => typeof(IChecker).IsAssignableFrom(x)).ToList();
            if (types.Count > 1)
            {
                throw new Exception("More than one implementation of OJS.Workers.Common.IChecker was found!");
            }

            var type = types.FirstOrDefault();
            if (type == null)
            {
                throw new Exception("Implementation of OJS.Workers.Common.IChecker not found!");
            }

            var instance = Activator.CreateInstance(type) as IChecker;
            if (instance == null)
            {
                throw new Exception($"Cannot create an instance of type {type.FullName}!");
            }

            this.compiledCustomCheckersCache.Set(parameter, instance, new CacheItemPolicy { SlidingExpiration = CacheDaysSlidingExpiration });
            this.customChecker = instance;
        }
    }
}