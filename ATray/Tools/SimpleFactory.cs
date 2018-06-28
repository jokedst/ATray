using System;
using Microsoft.Extensions.DependencyInjection;

namespace ATray.Tools
{
    /// <summary>
    /// Wrapper for the service provider so we can use factories without exposing the real service provider.
    /// This way we can keep track of our dependencies.
    /// </summary>
    /// <typeparam name="T"> Type of service this factory should produce. </typeparam>
    public class SimpleFactory<T> : IFactory<T>
    {
        private readonly IServiceProvider _serviceProvider;

        public SimpleFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public T Build() => _serviceProvider.GetService<T>();
    }
}