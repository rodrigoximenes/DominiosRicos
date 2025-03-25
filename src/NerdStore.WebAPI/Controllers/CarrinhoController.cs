using MediatR;
using Microsoft.AspNetCore.Mvc;
using NerdStore.Catalogo.Application.Services;
using NerdStore.Vendas.Application.Commands;

namespace NerdStore.WebApp.MVC.Controllers
{
    [ApiController]
    [Route("api/carrinho")]
    public class CarrinhoController : ControllerBase
    {
        private readonly IProdutoAppService _produtoAppService;
        private readonly IPedidoQueries _pedidoQueries;
        private readonly IMediatorHandler _mediatorHandler;
        private readonly DomainNotificationHandler _notifications;

        protected Guid ClienteId = Guid.Parse("4885e451-b0e4-4490-b959-04fabc806d32");

        public CarrinhoController(INotificationHandler<DomainNotification> notifications,
                                  IProdutoAppService produtoAppService,
                                  IMediatorHandler mediatorHandler,
                                  IPedidoQueries pedidoQueries)
        {
            _notifications = (DomainNotificationHandler)notifications;
            _produtoAppService = produtoAppService;
            _mediatorHandler = mediatorHandler;
            _pedidoQueries = pedidoQueries;
        }

        protected bool OperacaoValida()
        {
            return !_notifications.TemNotificacao();
        }

        protected IEnumerable<string> ObterMensagensErro()
        {
            return _notifications.ObterNotificacoes().Select(c => c.Value).ToList();
        }

        protected void NotificarErro(string codigo, string mensagem)
        {
            _mediatorHandler.PublicarNotificacao(new DomainNotification(codigo, mensagem));
        }

        [HttpGet("meu-carrinho")]
        public async Task<IActionResult> Index()
        {
            return Ok(await _pedidoQueries.ObterCarrinhoCliente(ClienteId));
        }

        [HttpPost("meu-carrinho")]
        public async Task<IActionResult> AdicionarItem(Guid id, int quantidade)
        {
            var produto = await _produtoAppService.ObterPorId(id);
            if (produto == null) return BadRequest();

            if (produto.QuantidadeEstoque < quantidade)
            {
                NotificarErro("EstoqueInsuficiente", "Produto com estoque insuficiente");
                return BadRequest(ObterMensagensErro());
            }

            var command = new AdicionarItemPedidoCommand(ClienteId, produto.Id, produto.Nome, quantidade, produto.Valor);
            await _mediatorHandler.EnviarComando(command);

            if (OperacaoValida())
            {
                return Ok();
            }

            return BadRequest(ObterMensagensErro());
        }

        [HttpPost("remover-item")]
        public async Task<IActionResult> RemoverItem(Guid id)
        {
            var produto = await _produtoAppService.ObterPorId(id);
            if (produto == null) return BadRequest();

            var command = new RemoverItemPedidoCommand(ClienteId, id);
            await _mediatorHandler.EnviarComando(command);

            if (OperacaoValida())
            {
                return Ok();
            }

            return BadRequest(ObterMensagensErro());
        }

        [HttpPost("atualizar-item")]
        public async Task<IActionResult> AtualizarItem(Guid id, int quantidade)
        {
            var produto = await _produtoAppService.ObterPorId(id);
            if (produto == null) return BadRequest();

            var command = new AtualizarItemPedidoCommand(ClienteId, id, quantidade);
            await _mediatorHandler.EnviarComando(command);

            if (OperacaoValida())
            {
                return Ok();
            }

            return BadRequest(ObterMensagensErro());
        }

        [HttpPost("aplicar-voucher")]
        public async Task<IActionResult> AplicarVoucher(string voucherCodigo)
        {
            var command = new AplicarVoucherPedidoCommand(ClienteId, voucherCodigo);
            await _mediatorHandler.EnviarComando(command);

            if (OperacaoValida())
            {
                return Ok();
            }

            return BadRequest(ObterMensagensErro());
        }

        [HttpGet("resumo-da-compra")]
        public async Task<IActionResult> ResumoDaCompra()
        {
            return Ok(await _pedidoQueries.ObterCarrinhoCliente(ClienteId));
        }

        [HttpPost("iniciar-pedido")]
        public async Task<IActionResult> IniciarPedido(CarrinhoViewModel carrinhoViewModel)
        {
            var carrinho = await _pedidoQueries.ObterCarrinhoCliente(ClienteId);

            var command = new IniciarPedidoCommand(carrinho.PedidoId, ClienteId, carrinho.ValorTotal, carrinhoViewModel.Pagamento.NomeCartao,
                carrinhoViewModel.Pagamento.NumeroCartao, carrinhoViewModel.Pagamento.ExpiracaoCartao, carrinhoViewModel.Pagamento.CvvCartao);

            await _mediatorHandler.EnviarComando(command);

            if (OperacaoValida())
            {
                return Ok();
            }

            return BadRequest(ObterMensagensErro());
        }
    }
}
