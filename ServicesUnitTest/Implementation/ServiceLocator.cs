namespace ServicesUnitTest.Implementation
{
    public static class ServiceLocator
    {
        private static IServiceProvider _serviceProvider;

        public static void SetLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }
    }
}
