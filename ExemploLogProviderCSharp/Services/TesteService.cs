namespace ExemploLogProviderCSharp.Services
{
	public class TesteService
	{
		private readonly ILogger<TesteService> logger;

		public TesteService(ILogger<TesteService> logger)
        {
			this.logger = logger;
		}

		public async Task Metodo1()
		{
			logger.LogInformation("TesteService.Metodo1");

			await Metodo2();
		}

		public async Task Metodo2()
		{
			await Task.Run(() =>
			{
				using var loggerScope = logger.BeginScope("Scopo1.1", "A", "B");

				logger.LogInformation("TesteService.Metodo2");
			});
		}
    }
}
