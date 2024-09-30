namespace Libreria.Models
{
    // ViewModel para mostrar los items del carrito
    public class ItemCarritoViewModel
    {
        public int ItemCarritoID { get; set; }
        public int? LibroID { get; set; }
        public int? SeparadorID { get; set; }
        public int Cantidad { get; set; }
    }
}
