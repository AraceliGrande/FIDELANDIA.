using System;

namespace FIDELANDIA.Helpers
{
    public static class AppEvents
    {
        // Evento global que notifica cuando se crea un lote
        public static event Action LoteCreado;

        // Método que dispara el evento
        public static void NotificarLoteCreado()
        {
            LoteCreado?.Invoke();
        }
    }
}
