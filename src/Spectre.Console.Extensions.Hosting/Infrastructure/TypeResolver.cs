using Spectre.Console.Cli;

namespace Spectre.Console.Extensions.Hosting.Infrastructure;

    public sealed class TypeResolver : ITypeResolver, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        public TypeResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public object Resolve(Type type)
        {
            if (type == null)
            {
                return null;
            }

            return _serviceProvider.GetService(type) ?? Activator.CreateInstance(type);
        }

        public void Dispose()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
