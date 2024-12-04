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

using System.Linq;

namespace Protocolo
{
    public class Protocolo1
    {
        // Método para generar un mensaje de pedido a partir de un comando y sus parámetros
        // El comando se convierte a mayúsculas y los parámetros se concatenan en una sola cadena
        public string CrearPedido(string comando, string[] parametros)
        {
            // Convierte el comando a mayúsculas y une los parámetros en una sola cadena con espacios
            return $"{comando.ToUpper()} {string.Join(" ", parametros)}";
        }

        // Método para procesar un mensaje recibido y devolver una tupla con el estado y el contenido del mensaje
        public (string Estado, string Mensaje) ProcesarRespuesta(string mensaje)
        {
            // Divide el mensaje en partes utilizando espacios como delimitador
            var partes = mensaje.Split(' ');

            // La primera parte del mensaje se considera el estado (por ejemplo, "OK" o "NOK")
            string estado = partes[0];

            // El resto del mensaje se considera el contenido del mensaje
            string contenidoMensaje = string.Join(" ", partes.Skip(1).ToArray());

            // Devuelve el estado y el contenido del mensaje como una tupla
            return (estado, contenidoMensaje);
        }
    }
}



