using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spectre.Console.Extensions.Hosting.Infrastructure;

internal abstract class ComponentActivator
{
    public abstract object Activate(DefaultTypeResolver container);

    public abstract ComponentActivator CreateCopy();
}

internal class CachingActivator : ComponentActivator
{
    private readonly ComponentActivator _activator;
    private object? _result;

    public CachingActivator(ComponentActivator activator)
    {
        _activator = activator ?? throw new ArgumentNullException(nameof(activator));
        _result = null;
    }

    public override object Activate(DefaultTypeResolver container)
    {
        return _result ??= _activator.Activate(container);
    }

    public override ComponentActivator CreateCopy()
    {
        return new CachingActivator(_activator.CreateCopy());
    }
}

internal sealed class InstanceActivator : ComponentActivator
{
    private readonly object _instance;

    public InstanceActivator(object instance)
    {
        _instance = instance;
    }

    public override object Activate(DefaultTypeResolver container)
    {
        return _instance;
    }

    public override ComponentActivator CreateCopy()
    {
        return new InstanceActivator(_instance);
    }
}

internal sealed class ReflectionActivator : ComponentActivator
{
    private readonly Type _type;
    private readonly ConstructorInfo _constructor;
    private readonly List<ParameterInfo> _parameters;

    public ReflectionActivator(Type type)
    {
        _type = type;
        _constructor = GetGreediestConstructor(type);
        _parameters = new List<ParameterInfo>();

        foreach (var parameter in _constructor.GetParameters())
        {
            _parameters.Add(parameter);
        }
    }

    public override object Activate(DefaultTypeResolver container)
    {
        var parameters = new object?[_parameters.Count];
        for (var i = 0; i < _parameters.Count; i++)
        {
            var parameter = _parameters[i];
            if (parameter.ParameterType == typeof(DefaultTypeResolver))
            {
                parameters[i] = container;
            }
            else
            {
                var resolved = container.Resolve(parameter.ParameterType);
                if (resolved == null)
                {
                    if (!parameter.IsOptional)
                    {
                        throw new InvalidOperationException($"Could not find registration for '{parameter.ParameterType.FullName}'.");
                    }

                    parameters[i] = null;
                }
                else
                {
                    parameters[i] = resolved;
                }
            }
        }

        return _constructor.Invoke(parameters);
    }

    public override ComponentActivator CreateCopy()
    {
        return new ReflectionActivator(_type);
    }

    private static ConstructorInfo GetGreediestConstructor(Type type)
    {
        ConstructorInfo? current = null;
        var count = -1;
        foreach (var constructor in type.GetTypeInfo().GetConstructors())
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length > count)
            {
                count = parameters.Length;
                current = constructor;
            }
        }

        if (current == null)
        {
            throw new InvalidOperationException($"Could not find a constructor for '{type.FullName}'.");
        }

        return current;
    }
}

internal sealed class ComponentRegistration
{
    public Type ImplementationType { get; }
    public ComponentActivator Activator { get; }
    public IReadOnlyList<Type> RegistrationTypes { get; }

    public ComponentRegistration(Type type, ComponentActivator activator, IEnumerable<Type>? registrationTypes = null)
    {
        var registrations = new List<Type>(registrationTypes ?? Array.Empty<Type>());
        if (registrations.Count == 0)
        {
            // Every registration needs at least one registration type.
            registrations.Add(type);
        }

        ImplementationType = type;
        RegistrationTypes = registrations;
        Activator = activator ?? throw new ArgumentNullException(nameof(activator));
    }

    public ComponentRegistration CreateCopy()
    {
        return new ComponentRegistration(ImplementationType, Activator.CreateCopy(), RegistrationTypes);
    }
}

internal sealed class ComponentRegistry : IDisposable
{
    private readonly Dictionary<Type, HashSet<ComponentRegistration>> _registrations;

    public ComponentRegistry()
    {
        _registrations = new Dictionary<Type, HashSet<ComponentRegistration>>();
    }

    public ComponentRegistry CreateCopy()
    {
        var registry = new ComponentRegistry();
        foreach (var registration in _registrations.SelectMany(p => p.Value))
        {
            registry.Register(registration.CreateCopy());
        }

        return registry;
    }

    public void Dispose()
    {
        foreach (var registration in _registrations)
        {
            registration.Value.Clear();
        }

        _registrations.Clear();
    }

    public void Register(ComponentRegistration registration)
    {
        foreach (var type in new HashSet<Type>(registration.RegistrationTypes))
        {
            if (!_registrations.ContainsKey(type))
            {
                // Only add each registration type once.
                _registrations.Add(type, new HashSet<ComponentRegistration>());
            }

            _registrations[type].Add(registration);
        }
    }

    public ICollection<ComponentRegistration> GetRegistrations(Type type)
    {
        if (_registrations.ContainsKey(type))
        {
            return _registrations[type];
        }

        return new List<ComponentRegistration>();
    }
}

internal sealed class DefaultTypeResolver : IDisposable, ITypeResolver
{
    private readonly IServiceProvider? _serviceProvider;

    public ComponentRegistry Registry { get; }

    public DefaultTypeResolver(ComponentRegistry? registry = null, IServiceProvider? serviceProvider = default)
    {
        Registry = registry ?? new ComponentRegistry();
        _serviceProvider = serviceProvider;
    }

    public void Dispose()
    {
        Registry.Dispose();
    }

    public object? Resolve(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        var isEnumerable = false;
        if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                isEnumerable = true;
                type = type.GenericTypeArguments[0];
            }
        }

        var registrations = Registry.GetRegistrations(type);
        if (registrations != null)
        {
            if (isEnumerable)
            {
                var result = Array.CreateInstance(type, registrations.Count);
                for (var index = 0; index < registrations.Count; index++)
                {
                    var registration = registrations.ElementAt(index);
                    result.SetValue(Resolve(registration), index);
                }

                return result;
            }
        }

        return Resolve(registrations?.LastOrDefault()) ?? _serviceProvider?.GetService(type);
    }

    public object? Resolve(ComponentRegistration? registration)
    {
        return registration?.Activator?.Activate(this);
    }
}

internal sealed class DefaultTypeRegistrar : ITypeRegistrar
{
    private readonly Queue<Action<ComponentRegistry>> _registry;
    private readonly IServiceProvider? _serviceProvider;

    public DefaultTypeRegistrar(IServiceProvider? serviceProvider = null)
    {
        _registry = new Queue<Action<ComponentRegistry>>();
        _serviceProvider = serviceProvider;
    }

    public ITypeResolver Build()
    {
        var container = new DefaultTypeResolver(serviceProvider: _serviceProvider);
        while (_registry.Count > 0)
        {
            var action = _registry.Dequeue();
            action(container.Registry);
        }

        return container;
    }

    public void Register(Type service, Type implementation)
    {
        var registration = new ComponentRegistration(implementation, new ReflectionActivator(implementation), new[] { service });
        _registry.Enqueue(registry => registry.Register(registration));
    }

    public void RegisterInstance(Type service, object implementation)
    {
        var registration = new ComponentRegistration(service, new CachingActivator(new InstanceActivator(implementation)));
        _registry.Enqueue(registry => registry.Register(registration));
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _registry.Enqueue(registry =>
        {
            var activator = new CachingActivator(new InstanceActivator(factory()));
            var registration = new ComponentRegistration(service, activator);

            registry.Register(registration);
        });
    }
}

internal class TieredTypeResolver : ITypeResolver
{
    private readonly ITypeResolver _primary;
    private readonly IServiceProvider _secondary;

    public TieredTypeResolver(ITypeResolver primary, IServiceProvider secondary)
    {
        _primary = primary ?? throw new ArgumentNullException(nameof(primary));
        _secondary = secondary ?? throw new ArgumentNullException(nameof(secondary));
    }

    public object? Resolve(Type? type)
    {
        return _primary.Resolve(type) ?? (type == null ? null : _secondary.GetService(type));
    }
}

internal class TieredTypeRegistrar : ITypeRegistrar
{
    private readonly ITypeRegistrar _primary;
    private readonly IServiceProvider _secondary;

    public TieredTypeRegistrar(ITypeRegistrar primary, IServiceProvider secondary)
    {
        _primary = primary ?? throw new ArgumentNullException(nameof(primary));
        _secondary = secondary ?? throw new ArgumentNullException(nameof(secondary));
    }

    public static ITypeRegistrar FromServices(IServiceProvider secondary)
    {
        return new TieredTypeRegistrar(new DefaultTypeRegistrar(secondary), secondary);
    }

    public ITypeResolver Build()
    {
        return new TieredTypeResolver(_primary.Build(), _secondary);
    }

    public void Register(Type service, Type implementation)
    {
        _primary.Register(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _primary.RegisterInstance(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _primary.RegisterLazy(service, factory);
    }
}
