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
using System.Windows.Forms;
using System.Net.Sockets;
using Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        private TcpClient remoto;  // Cliente TCP para conectarse al servidor
        private NetworkStream flujo;  // Flujo de datos para la comunicación
        private Protocolo1 protocolo;  // Instancia de la clase Protocolo para manejar las operaciones

        public FrmValidador()
        {
            InitializeComponent();
            protocolo = new Protocolo1();  // Inicialización de la clase Protocolo para manejar los mensajes
        }

        // Método que se ejecuta cuando se carga el formulario
        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                // Intenta establecer conexión con el servidor en la IP local (127.0.0.1) en el puerto 8080
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream();  // Obtiene el flujo de datos para la comunicación
            }
            catch (SocketException ex)
            {
                // Si ocurre un error en la conexión, muestra un mensaje de error y cierra las conexiones
                MessageBox.Show("No se pudo establecer conexión: " + ex.Message, "ERROR");
                flujo?.Close();
                remoto?.Close();
            }

            // Deshabilita los controles relacionados con la placa hasta que el acceso sea concedido
            panPlaca.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkDomingo.Enabled = false;
            chkSabado.Enabled = false;
        }

        // Método que se ejecuta cuando el botón de iniciar es presionado
        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;

            // Si el usuario o la contraseña están vacíos, muestra un mensaje de advertencia
            if (usuario == "" || contraseña == "")
            {
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña", "ADVERTENCIA");
                return;
            }

            // Llama a la función HazOperacion para realizar el ingreso con las credenciales
            string respuesta = HazOperacion("INGRESO", new[] { usuario, contraseña });
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            // Procesa la respuesta recibida
            var (estado, mensaje) = protocolo.ProcesarRespuesta(respuesta);
            if (estado == "OK" && mensaje == "ACCESO_CONCEDIDO")
            {
                // Si el acceso es concedido, habilita el panel de placa y deshabilita el panel de login
                panPlaca.Enabled = true;
                panLogin.Enabled = false;
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus();  // Coloca el foco en el campo de modelo
            }
            else if (estado == "NOK" && mensaje == "ACCESO_NEGADO")
            {
                // Si el acceso es negado, muestra un mensaje de error y habilita el panel de login
                panPlaca.Enabled = false;
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales", "ERROR");
                txtUsuario.Focus();  // Coloca el foco en el campo de usuario
            }
        }

        // Método que maneja las operaciones de envío y recepción de mensajes con el servidor
        private string HazOperacion(string comando, string[] parametros)
        {
            if (flujo == null)
            {
                // Si no hay conexión, muestra un mensaje de error
                MessageBox.Show("No hay conexión", "ERROR");
                return null;
            }
            try
            {
                // Crea el mensaje de la operación a realizar
                string mensaje = protocolo.CrearPedido(comando, parametros);

                // Convierte el mensaje a bytes y lo envía al servidor
                byte[] bufferTx = Encoding.UTF8.GetBytes(mensaje);
                flujo.Write(bufferTx, 0, bufferTx.Length);

                // Lee la respuesta del servidor
                byte[] bufferRx = new byte[1024];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);
                return Encoding.UTF8.GetString(bufferRx, 0, bytesRx);
            }
            catch (SocketException ex)
            {
                // Si ocurre un error durante la transmisión, muestra un mensaje de error
                MessageBox.Show("Error al intentar transmitir: " + ex.Message, "ERROR");
            }
            return null;
        }

        // Método que se ejecuta cuando el botón de consulta es presionado
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            // Llama a la función HazOperacion para realizar la consulta
            string respuesta = HazOperacion("CALCULO", new[] { modelo, marca, placa });
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            // Procesa la respuesta recibida
            var (estado, mensaje) = protocolo.ProcesarRespuesta(respuesta);
            if (estado == "NOK")
            {
                // Si la respuesta indica error, muestra un mensaje y reinicia los checkboxes
                MessageBox.Show("Error en la solicitud.", "ERROR");
                ResetCheckBoxes();
            }
            else
            {
                // Si la respuesta es válida, muestra el mensaje y actualiza los checkboxes
                var partes = mensaje.Split(' ');
                MessageBox.Show("Se recibió: " + mensaje, "INFORMACIÓN");
                byte resultado = Byte.Parse(partes[1]);
                ActualizarDias(resultado);
            }
        }

        // Método para actualizar los días seleccionados según el resultado de la consulta
        private void ActualizarDias(byte resultado)
        {
            // Compara el resultado con una máscara de bits para determinar los días de la semana
            chkLunes.Checked = (resultado & 0b00100000) != 0;
            chkMartes.Checked = (resultado & 0b00010000) != 0;
            chkMiercoles.Checked = (resultado & 0b00001000) != 0;
            chkJueves.Checked = (resultado & 0b00000100) != 0;
            chkViernes.Checked = (resultado & 0b00000010) != 0;
        }

        // Método para reiniciar los checkboxes de los días
        private void ResetCheckBoxes()
        {
            chkLunes.Checked = false;
            chkMartes.Checked = false;
            chkMiercoles.Checked = false;
            chkJueves.Checked = false;
            chkViernes.Checked = false;
        }

        // Método que se ejecuta cuando el botón de consultas es presionado
        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            // Llama a la función HazOperacion para obtener el contador de consultas
            string respuesta = HazOperacion("CONTADOR", new[] { "hola" });
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            // Procesa la respuesta recibida
            var (estado, mensaje) = protocolo.ProcesarRespuesta(respuesta);
            if (estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
            }
            else
            {
                // Muestra el número de consultas realizadas
                MessageBox.Show("El número de pedidos recibidos en este cliente es " + mensaje, "INFORMACIÓN");
            }
        }

        // Método que se ejecuta cuando el formulario se está cerrando
        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cierra las conexiones cuando el formulario se cierra
            flujo?.Close();
            remoto?.Close();
        }
    }
}
