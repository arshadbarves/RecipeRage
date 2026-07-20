using System;
using System.Collections.Generic;

namespace Playcenter.SDK
{
    public sealed class ServiceRegistry : IServiceRegistry, IPlaycenterServices
    {
        readonly Dictionary<Type, Registration> _registrations = new Dictionary<Type, Registration>();
        bool _built;

        public void AddSingleton<TService>(TService instance) where TService : class
        {
            ThrowIfBuilt();
            var serviceType = typeof(TService);
            ThrowIfDuplicate(serviceType);
            _registrations[serviceType] = new InstanceRegistration(instance);
        }

        public void AddSingleton<TService, TImpl>() where TService : class where TImpl : class, TService, new()
        {
            ThrowIfBuilt();
            var serviceType = typeof(TService);
            ThrowIfDuplicate(serviceType);
            _registrations[serviceType] = new TypeRegistration(typeof(TImpl));
        }

        public void AddSingleton<TService>(Func<IPlaycenterServices, TService> factory) where TService : class
        {
            ThrowIfBuilt();
            var serviceType = typeof(TService);
            ThrowIfDuplicate(serviceType);
            _registrations[serviceType] = new FactoryRegistration(sp => factory(sp));
        }

        public IPlaycenterServices Build()
        {
            _built = true;
            return this;
        }

        public T Get<T>() where T : class
        {
            if (!TryGet<T>(out var service))
            {
                throw new InvalidOperationException($"Service of type '{typeof(T).Name}' is not registered.");
            }
            return service;
        }

        public bool TryGet<T>(out T service) where T : class
        {
            var serviceType = typeof(T);
            if (_registrations.TryGetValue(serviceType, out var registration))
            {
                service = (T)registration.Resolve(this);
                return true;
            }
            service = null;
            return false;
        }

        public bool IsRegistered<T>() where T : class
        {
            return _registrations.ContainsKey(typeof(T));
        }

        void ThrowIfBuilt()
        {
            if (_built)
            {
                throw new InvalidOperationException("Cannot add services after Build() has been called.");
            }
        }

        void ThrowIfDuplicate(Type serviceType)
        {
            if (_registrations.ContainsKey(serviceType))
            {
                throw new InvalidOperationException($"Service of type '{serviceType.Name}' is already registered.");
            }
        }

        abstract class Registration
        {
            public abstract object Resolve(IPlaycenterServices services);
        }

        sealed class InstanceRegistration : Registration
        {
            readonly object _instance;

            public InstanceRegistration(object instance)
            {
                _instance = instance;
            }

            public override object Resolve(IPlaycenterServices services)
            {
                return _instance;
            }
        }

        sealed class TypeRegistration : Registration
        {
            readonly Type _implementationType;
            object _instance;
            readonly object _lock = new object();

            public TypeRegistration(Type implementationType)
            {
                _implementationType = implementationType;
            }

            public override object Resolve(IPlaycenterServices services)
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = Activator.CreateInstance(_implementationType);
                        }
                    }
                }
                return _instance;
            }
        }

        sealed class FactoryRegistration : Registration
        {
            readonly Func<IPlaycenterServices, object> _factory;
            object _instance;
            readonly object _lock = new object();

            public FactoryRegistration(Func<IPlaycenterServices, object> factory)
            {
                _factory = factory;
            }

            public override object Resolve(IPlaycenterServices services)
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = _factory(services);
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
