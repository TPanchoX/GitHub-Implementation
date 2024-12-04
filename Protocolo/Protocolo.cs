/* Escuela Politecnica Nacional
 * Nombre: Francisco Imbaquinga
 * Fecha: 03/12/2024
 * Explicacion del programa:
 * Este programa es un sistema de validación de placas vehiculares que permite a los usuarios verificar si su vehículo puede circular en un día específico de la semana. El sistema se compone de dos aplicaciones: un servidor y un cliente. El servidor escucha las solicitudes de los clientes y responde con información sobre la restricción vehicular. El cliente envía solicitudes al servidor y muestra la respuesta al usuario.
 * 
 * Conclusiones: 
 * 1.Como se pudo observar Git es un sistema que nos permite controlar las versiones de un proyecto, rastrear cambios en el código fuente para realizar seguimientos, esta herramienta es muy importante cuando se trabaja con colaboradores y no se desea que alguno interfiera en el trabajo de otro.
 * 2.En conclusion, GitHub es una plataforma que nos permite alojar proyectos en la nube, colaborar con otras personas, llevar un control de versiones de nuestro código, realizar un seguimiento de los cambios que se realizan en el código, entre otras cosas.
 * 3.Podemos concluir que uno de los mejores IDEs para usar con Git y Github es Visual Studio, porque en su entorno de desarrollo cuenta con una integración de Git y GitHub. Esto permite hacer modificaciones al código y sincronizarlas directamente desde la interfaz.
 * 
 * Recomendaciones:
 * 1.Se recomienda que al agregar cualquier proyecto a través del Git bash se recomienda estar en la ubicación del proyecto que se desea agregar y abrir el bash mediante click derecho para abrirlo con el path del archivo que se desea subir.
 * 2.Es recomendable que al guardar cualquier cambio desde visual studio a un repositorio de GitHub, se recomienda presionar la opción “push” primero y luego la opción “sincronizar”, esto permitirá que los cambios queden guardados en el repositorio de Github, algunas veces si solo se presiona sincronizar los cambios no suelen guardarse.
 */

using System;
using System.Linq;  
using System.Text;  
using System.Net.Sockets;  

namespace Protocolo
{
    public static class Protocolo
    {
        // Método para enviar una solicitud al servidor y recibir una respuesta
        public static Respuesta HazOperacion(Pedido pedido, NetworkStream flujo)
        {
            // Verifica que el flujo de red esté inicializado antes de continuar
            if (flujo == null)
            {
                throw new InvalidOperationException("No hay conexión");
            }
            try
            {
                // Prepara el mensaje de la solicitud para enviarlo al servidor
                byte[] bufferTx = Encoding.UTF8.GetBytes(
                    pedido.Comando + " " + string.Join(" ", pedido.Parametros)); // Combina el comando y los parámetros

                // Envía los datos al servidor a través del flujo de red
                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Prepara un buffer para recibir la respuesta del servidor
                byte[] bufferRx = new byte[1024];

                // Lee la respuesta del servidor y almacena la cantidad de bytes leídos
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);

                // Decodifica la respuesta recibida desde bytes a una cadena UTF-8
                string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                // Divide la respuesta en partes para separar el estado del mensaje
                var partes = mensaje.Split(' ');

                // Crea y devuelve un objeto de tipo Respuesta con la información recibida
                return new Respuesta
                {
                    Estado = partes[0],  // La primera parte es el estado de la operación (OK o NOK)
                    Mensaje = string.Join(" ", partes.Skip(1).ToArray())  // El resto es el mensaje detallado
                };
            }
            catch (SocketException ex)
            {
                // Lanza una excepción si ocurre un error durante la transmisión de datos
                throw new InvalidOperationException("Error al intentar transmitir " + ex.Message);
            }
        }
    }
}

