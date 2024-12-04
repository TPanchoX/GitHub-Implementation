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
using System.Linq;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador; // Objeto para escuchar conexiones TCP entrantes
        private static Dictionary<string, int> listadoClientes = new Dictionary<string, int>(); // Diccionario para rastrear el número de solicitudes por cliente
        private static Protocolo.Protocolo1 protocolo = new Protocolo.Protocolo1(); // Instancia de la clase Protocolo para manejar mensajes

        static void Main(string[] args)
        {
            try
            {
                // Configura el servidor para escuchar en cualquier IP en el puerto 8080
                escuchador = new TcpListener(IPAddress.Any, 8080);
                escuchador.Start();
                Console.WriteLine("Servidor inició en el puerto 8080...");

                // Bucle infinito para aceptar múltiples conexiones
                while (true)
                {
                    TcpClient cliente = escuchador.AcceptTcpClient(); // Acepta un cliente entrante
                    Console.WriteLine("Cliente conectado, puerto: {0}", cliente.Client.RemoteEndPoint.ToString());
                    // Crea un nuevo hilo para manejar la conexión del cliente
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                // Manejo de errores de conexión
                Console.WriteLine("Error de socket al iniciar el servidor: " + ex.Message);
            }
            finally
            {
                // Asegura que el servidor se detenga correctamente en caso de error
                escuchador?.Stop();
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj; // Convierte el objeto recibido en un TcpClient
            NetworkStream flujo = null; // Flujo de datos para la comunicación con el cliente
            try
            {
                flujo = cliente.GetStream(); // Obtiene el flujo de datos del cliente
                byte[] bufferTx; // Buffer para enviar datos
                byte[] bufferRx = new byte[1024]; // Buffer para recibir datos
                int bytesRx;

                // Bucle para recibir datos del cliente mientras la conexión esté activa
                while ((bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length)) > 0)
                {
                    string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx); // Decodifica el mensaje recibido
                    var (comando, parametros) = DescomponerPedido(mensajeRx); // Descompone el mensaje en comando y parámetros
                    Console.WriteLine("Se recibió: " + mensajeRx);

                    string direccionCliente = cliente.Client.RemoteEndPoint.ToString(); // Obtiene la dirección del cliente
                    string respuesta = ResolverPedido(comando, parametros, direccionCliente); // Resuelve la solicitud del cliente
                    Console.WriteLine("Se envió: " + respuesta);

                    bufferTx = Encoding.UTF8.GetBytes(respuesta); // Codifica la respuesta
                    flujo.Write(bufferTx, 0, bufferTx.Length); // Envía la respuesta al cliente
                }

            }
            catch (SocketException ex)
            {
                // Manejo de errores durante la comunicación con el cliente
                Console.WriteLine("Error de socket al manejar el cliente: " + ex.Message);
            }
            finally
            {
                // Cierra el flujo y la conexión del cliente
                flujo?.Close();
                cliente?.Close();
            }
        }

        // Método para descomponer un mensaje en comando y parámetros
        private static (string comando, string[] parametros) DescomponerPedido(string mensaje)
        {
            var partes = mensaje.Split(' '); // Divide el mensaje en partes usando espacios
            string comando = partes[0]; // El primer elemento es el comando
            string[] parametros = partes.Length > 1 ? partes.Skip(1).ToArray() : new string[] { }; // Los elementos restantes son parámetros
            return (comando, parametros);
        }

        // Método para resolver la solicitud del cliente según el comando recibido
        private static string ResolverPedido(string comando, string[] parametros, string direccionCliente)
        {
            string estado = "NOK"; // Estado por defecto en caso de error
            string mensaje = "Comando no reconocido"; // Mensaje por defecto en caso de comando inválido

            switch (comando)
            {
                case "INGRESO":
                    // Valida las credenciales del usuario
                    if (parametros.Length == 2 && parametros[0] == "root" && parametros[1] == "admin20")
                    {
                        estado = new Random().Next(2) == 0 ? "OK" : "NOK"; // Simula un acceso aleatorio
                        mensaje = estado == "OK" ? "ACCESO_CONCEDIDO" : "ACCESO_NEGADO";
                    }
                    else
                    {
                        mensaje = "ACCESO_NEGADO"; // Credenciales inválidas
                    }
                    break;

                case "CALCULO":
                    // Procesa la solicitud de validación de placa
                    if (parametros.Length == 3)
                    {
                        string placa = parametros[2];
                        if (ValidarPlaca(placa)) // Valida el formato de la placa
                        {
                            byte indicadorDia = ObtenerIndicadorDia(placa); // Calcula el día permitido según la placa
                            estado = "OK";
                            mensaje = $"{placa} {indicadorDia}";
                            ContadorCliente(direccionCliente); // Actualiza el contador de solicitudes del cliente
                        }
                        else
                        {
                            mensaje = "Placa no válida"; // La placa no cumple con el formato requerido
                        }
                    }
                    break;

                case "CONTADOR":
                    // Devuelve el número de solicitudes realizadas por el cliente
                    if (listadoClientes.ContainsKey(direccionCliente))
                    {
                        estado = "OK";
                        mensaje = listadoClientes[direccionCliente].ToString();
                    }
                    else
                    {
                        mensaje = "No hay solicitudes previas"; // El cliente no ha realizado solicitudes previas
                    }
                    break;
            }

            return protocolo.CrearPedido(estado, new[] { mensaje }); // Crea y devuelve la respuesta usando el protocolo
        }

        // Método para validar el formato de una placa de vehículo
        private static bool ValidarPlaca(string placa)
        {
            return Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$"); // Expresión regular para placas: 3 letras seguidas de 4 números
        }

        // Método para determinar el día de circulación según el último dígito de la placa
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

        // Método para contar las solicitudes realizadas por cada cliente
        private static void ContadorCliente(string direccionCliente)
        {
            if (listadoClientes.ContainsKey(direccionCliente))
            {
                listadoClientes[direccionCliente]++; // Incrementa el contador si el cliente ya existe
            }
            else
            {
                listadoClientes[direccionCliente] = 1; // Inicializa el contador si es un cliente nuevo
            }
        }
    }
}
