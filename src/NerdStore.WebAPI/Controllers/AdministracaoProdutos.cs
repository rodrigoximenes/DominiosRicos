using Microsoft.AspNetCore.Mvc;
using NerdStore.Catalogo.Application.Services;
using NerdStore.Catalogo.Application.ViewModels;

namespace NerdStore.WebApp.API.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminProdutosController : ControllerBase
    {
        private readonly IProdutoAppService _produtoAppService;

        public AdminProdutosController(IProdutoAppService produtoAppService)
        {
            _produtoAppService = produtoAppService;
        }

        // GET api/adminprodutos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var produtos = await _produtoAppService.ObterTodos();
            return Ok(produtos);
        }

        // GET api/adminprodutos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var produto = await _produtoAppService.ObterPorId(id);
            if (produto == null)
                return NotFound();

            return Ok(produto);
        }

        // POST api/adminprodutos
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _produtoAppService.AdicionarProduto(produtoViewModel);

            return CreatedAtAction(nameof(GetById), new { id = produtoViewModel.Id }, produtoViewModel);
        }

        // PUT api/adminprodutos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProdutoViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id)
                return BadRequest("Produto ID mismatch.");

            var produtoExistente = await _produtoAppService.ObterPorId(id);
            if (produtoExistente == null)
                return NotFound();

            produtoViewModel.QuantidadeEstoque = produtoExistente.QuantidadeEstoque;

            ModelState.Remove("QuantidadeEstoque");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _produtoAppService.AtualizarProduto(produtoViewModel);

            return NoContent();
        }

        // PUT api/adminprodutos/{id}/estoque
        [HttpPut("{id}/estoque")]
        public async Task<IActionResult> UpdateEstoque(Guid id, [FromBody] int quantidade)
        {
            if (quantidade > 0)
            {
                await _produtoAppService.ReporEstoque(id, quantidade);
            }
            else
            {
                await _produtoAppService.DebitarEstoque(id, quantidade);
            }

            return NoContent();
        }

        private async Task<ProdutoViewModel> PopularCategorias(ProdutoViewModel produto)
        {
            produto.Categorias = await _produtoAppService.ObterCategorias();
            return produto;
        }
    }
}
