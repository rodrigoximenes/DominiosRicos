using Microsoft.AspNetCore.Mvc;
using NerdStore.Catalogo.Application.Services;

namespace NerdStore.WebApp.MVC.Controllers
{
    [ApiController]
    [Route("api/vitrine")]
    public class VitrineController : ControllerBase
    {
        private readonly IProdutoAppService _produtoAppService;

        public VitrineController(IProdutoAppService produtoAppService)
        {
            _produtoAppService = produtoAppService;
        }

        [HttpGet]
        public async Task<IActionResult> ObterProdutos()
        {
            var produtos = await _produtoAppService.ObterTodos();
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterProdutoDetalhe(Guid id)
        {
            var produto = await _produtoAppService.ObterPorId(id);
            if (produto == null) return NotFound();
            return Ok(produto);
        }
    }
}
