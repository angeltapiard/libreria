namespace Libreria.Models
{
    public class Pedido
    {
        public int PedidoID { get; set; }
        public DateTime FechaPedido { get; set; }
        public string Calle { get; set; }
        public string Municipio { get; set; }
        public string Provincia { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public int UsuarioID { get; set; }
        public List<Pedido> Pedidos { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string EmailUsuario { get; set; }

    }
}


