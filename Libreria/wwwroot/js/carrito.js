document.addEventListener('DOMContentLoaded', function () {
    actualizarTotal();
    const total = document.getElementById('totalAPagar').innerText;
    document.getElementById('totalAPagarPedido').innerText = total;
    document.getElementById('totalPedido').value = total; // Guardar total en el campo oculto del pedido
});

function cambiarCantidad(itemId, cambio) {
    const cantidadInput = document.querySelector(`input[name='cantidades[${itemId}]']`);
    let nuevaCantidad = parseInt(cantidadInput.value) + cambio;

    if (nuevaCantidad < 1) {
        eliminarItem(itemId);
        return;
    }

    cantidadInput.value = nuevaCantidad;
    actualizarTotal();
}

function eliminarItem(itemId) {
    fetch('@Url.Action("EliminarItem", "Carrito")', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': '@@AntiForgeryToken()'
        },
        body: JSON.stringify({ id: itemId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const row = document.querySelector(`tr[data-item-id='${itemId}']`);
                row.remove();
                actualizarTotal();
            } else {
                alert("Hubo un error al eliminar el ítem.");
            }
        })
        .catch(error => console.error('Error:', error));
}

function actualizarTotal() {
    let total = 0;
    const checkboxes = document.querySelectorAll('.item-checkbox:checked');

    checkboxes.forEach(checkbox => {
        const row = checkbox.closest('tr');
        const precio = parseFloat(row.querySelector('td:nth-last-child(2) span').innerText.replace('$', ''));
        const cantidad = parseInt(row.querySelector(`input[name='cantidades[${checkbox.value}]']`).value);
        total += precio * cantidad;
    });

    document.getElementById('totalAPagar').innerText = total.toFixed(2);
    document.getElementById('total').value = total.toFixed(2); // Actualizar total en el campo oculto del carrito
    document.getElementById('totalAPagarPedido').innerText = total.toFixed(2);
}

function validarSeleccion() {
    const checkboxes = document.querySelectorAll('.item-checkbox:checked');
    if (checkboxes.length === 0) {
        alert("Por favor, selecciona al menos un artículo antes de continuar.");
        return;
    }
    document.getElementById('carritoForm').submit();
}

document.addEventListener('DOMContentLoaded', actualizarTotal);