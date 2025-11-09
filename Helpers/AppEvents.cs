using System;

namespace FIDELANDIA.Helpers
{
    public static class AppEvents
    {
        // Evento global que notifica cuando se crea un lote
        public static event Action LoteCreado;

        // Evento global que notifica cuando se crea una transaccion
        public static event Action<int> TransaccionCreada;



        // Método que dispara el evento
        public static void NotificarLoteCreado()
        {
            LoteCreado?.Invoke();
        }
        public static void NotificarVentaCreada()
        {
            LoteCreado?.Invoke();
        }
        public static void OnTransaccionCreada(int proveedorId)
        {
            TransaccionCreada?.Invoke(proveedorId);
        }
        public static void NotificarEliminado()
        {
            LoteCreado?.Invoke();
        }
    }
}
