using ExemploLogProviderCSharp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExemploLogProviderCSharp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TesteController : ControllerBase
	{
		private readonly ILogger<TesteController> logger;
		private readonly TesteService testeService;

		public TesteController(ILogger<TesteController> logger, TesteService testeService)
		{
			this.logger = logger;
			this.testeService = testeService;
		}

		[HttpGet]
		public async Task<string> Teste()
		{
			logger.LogInformation($"TesteController.Teste - Início...");

			using (var loggerScope = logger.BeginScopeLevelGroup("Controller"))
			{

				logger.LogInformation($"TesteController.Teste - Dentro do scopo de log - 1");

				await testeService.Metodo1();

				logger.LogInformation($"TesteController.Teste - Dentro do scopo de log - 2");
			}

			logger.LogInformation($"TesteController.Teste - Fim...");

			return "Teste";
		}


		[HttpPost]
		public void TesteErro()
		{
			using var loggerScope = logger.BeginScopeLevelGroup();

			try
			{
				try
				{
					GerarExcecaoInterna();
				}
				catch (Exception ex)
				{
					throw new Exception("Teste erro!", ex);
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, $"Chamado método {nameof(TesteErro)}");
			}
		}

		private void GerarExcecaoInterna() =>
			throw new Exception("Gerando exceção interna!");

	}
}
