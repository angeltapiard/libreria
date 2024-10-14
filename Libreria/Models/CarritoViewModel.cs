using System.Collections.Generic;

namespace Libreria.Models
{
    public class CarritoViewModel
    {
        public List<ItemCarrito> CarritoItems { get; set; }
        public List<Pedido> Pedidos { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }

        // Agregamos la lista de métodos de pago
        public List<MetodoPago> MetodosPago { get; set; }
        public int MetodoPagoID { get; set; }
        public string TipoTarjeta { get; set; }
        public string NumeroTarjeta { get; set; }
        public string TitularTarjeta { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string CVC { get; set; }
        public int PedidoID { get; set; }
    }
}
