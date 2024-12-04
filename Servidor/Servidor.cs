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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Protocolo;

namespace Servidor
{
    class Servidor
    {
        static void Main(string[] args)
        {
            TcpListener escucha1 = null;
            try
            {
                // Crea un objeto TcpListener para escuchar conexiones en el puerto 8080
                escucha1 = new TcpListener(IPAddress.Any, 8080);
                escucha1.Start(); // Inicia el servidor

                while (true)
                {
                    Console.WriteLine("Esperando conexión...");
                    // Espera la llegada de un cliente y acepta la conexión
                    TcpClient cliente = escucha1.AcceptTcpClient();
                    NetworkStream flujo = cliente.GetStream(); // Obtiene el flujo de datos del cliente

                    // Crea un buffer para recibir datos del cliente
                    byte[] bufferRx = new byte[1024];
                    int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length); // Lee los datos recibidos
                    string mensaje = Encoding.UTF8.GetString(bufferRx, 0, bytesRx); // Convierte los bytes a una cadena

                    // Extrae el comando y los parámetros del mensaje recibido
                    Pedido pedido = new Pedido
                    {
                        Comando = mensaje.Split(' ')[0], // Primer palabra del mensaje como comando
                        Parametros = mensaje.Split(' ').Skip(1).ToArray() // Resto de las palabras como parámetros
                    };

                    // Procesa el pedido recibido y genera una respuesta
                    Respuesta respuesta = ResolverPedido(pedido);

                    // Convierte la respuesta en bytes y la envía al cliente
                    byte[] bufferTx = Encoding.UTF8.GetBytes(respuesta.Estado + " " + respuesta.Mensaje);
                    flujo.Write(bufferTx, 0, bufferTx.Length);

                    // Cierra la conexión con el cliente después de enviar la respuesta
                    flujo.Close();
                    cliente.Close();
                }
            }
            catch (SocketException ex)
            {
                // Captura y muestra errores relacionados con la conexión
                Console.WriteLine("SocketException: {0}", ex);
            }
            finally
            {
                // Detiene el servidor de escuchar nuevas conexiones
                escucha1?.Stop();
            }
        }

        // Procesa los diferentes comandos enviados por el cliente
        private static Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            switch (pedido.Comando)
            {
                case "INGRESO":
                    // Verifica si las credenciales son correctas
                    if (pedido.Parametros[0] == "admin" && pedido.Parametros[1] == "1234")
                    {
                        return new Respuesta { Estado = "OK", Mensaje = "ACCESO_CONCEDIDO" };
                    }
                    else
                    {
                        return new Respuesta { Estado = "NOK", Mensaje = "ACCESO_DENEGADO" };
                    }

                case "CALCULO":
                    // Valida la estructura de los parámetros recibidos
                    if (pedido.Parametros.Length == 3)
                    {
                        string modelo = pedido.Parametros[0];
                        string marca = pedido.Parametros[1];
                        string placa = pedido.Parametros[2];
                        if (ValidarPlaca(placa)) // Verifica si la placa tiene un formato válido
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa); // Determina el día de restricción vehicular
                            ContadorCliente(direccionCliente); // Incrementa el contador de solicitudes del cliente
                            return new Respuesta { Estado = "OK", Mensaje = $"{placa} {indicadorDia}" };
                        }
                        else
                        {
                            return new Respuesta { Estado = "NOK", Mensaje = "Placa no válida" };
                        }
                    }
                    return new Respuesta { Estado = "NOK", Mensaje = "Parámetros incorrectos" };

                case "CONTADOR":
                    // Verifica si el cliente ha realizado solicitudes previas
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        return new Respuesta { Estado = "OK", Mensaje = listadoClientes[direccionCliente].ToString() };
                    }
                    else
                    {
                        return new Respuesta { Estado = "NOK", Mensaje = "No hay solicitudes previas" };
                    }

                default:
                    // Maneja comandos desconocidos o no implementados
                    return new Respuesta { Estado = "NOK", Mensaje = "COMANDO_DESCONOCIDO" };
            }
        }

        // Valida el formato de la placa vehicular usando una expresión regular
        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
        }

        // Determina el día de restricción vehicular basado en el último dígito de la placa
        private static byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(6, 1)); // Extrae el último dígito de la placa
            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return 0b00100000; // Lunes
                case 3:
                case 4:
                    return 0b00010000; // Martes
                case 5:
                case 6:
                    return 0b00001000; // Miércoles
                case 7:
                case 8:
                    return 0b00000100; // Jueves
                case 9:
                case 0:
                    return 0b00000010; // Viernes
                default:
                    return 0;
            }
        }

        // Cuenta las solicitudes realizadas por un cliente específico
        private static void ContadorCliente(string direccionCliente)
        {
            if (listadoClientes.ContainsKey(direccionCliente))
            {
                listadoClientes[direccionCliente]++; // Incrementa el contador si el cliente ya existe
            }
            else
            {
                listadoClientes[direccionCliente] = 1; // Inicia el contador si es la primera solicitud
            }
        }
    }
}
