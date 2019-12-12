using Autofac;
using ytm.Services;
using ytm.UI.Helpers;

namespace ytm.UI
{
    public class Resolver
    {
        private readonly string[] _args;
        private readonly IContainer _container;

        public Resolver(string[] args)
        {
            _args = args;
            _container = BuildContainer();
        }

        public T Get<T>()
        {
            return _container.Resolve<T>();
        }

        private IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder
                .Register(_ => new ConfigProvider(_args.GetPathToMainConfig()))
                .As<IConfigProvider>()
                .SingleInstance();

            return builder.Build();
        }
    }
}