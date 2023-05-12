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
				logger.LogInformation("TesteService.Metodo2 - Início...");

				using (var loggerScope = logger.BeginScopeLevelGroup("Service"))
				{
					logger.LogInformation("TesteService.Metodo2 - Dentro do scopo - 1");
					logger.LogInformation("TesteService.Metodo2 - Dentro do scopo - 2");
				}

				logger.LogInformation("TesteService.Metodo2 - Fim...");
			});
		}
    }
}
