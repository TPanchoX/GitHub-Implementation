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
        private TcpClient remoto;
        private NetworkStream flujo;

        public FrmValidador()
        {
            InitializeComponent();
        }

        private void FrmValidador_Load(object sender, EventArgs e)
        {
            try
            {
                // Intenta establecer una conexión con el servidor en la dirección local y puerto 8080
                remoto = new TcpClient("127.0.0.1", 8080);
                flujo = remoto.GetStream(); // Obtiene el flujo de datos asociado a la conexión
            }
            catch (SocketException ex)
            {
                // Muestra un mensaje de error si la conexión falla
                MessageBox.Show("No se pudo establecer conexión " + ex.Message,
                    "ERROR");
            }
            finally
            {
                // Cierra la conexión si está abierta
                flujo?.Close();
                remoto?.Close();
            }

            // Desactiva controles relacionados con la placa hasta que el usuario inicie sesión
            panPlaca.Enabled = false;
            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkDomingo.Enabled = false;
            chkSabado.Enabled = false;
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            // Captura las credenciales del usuario
            string usuario = txtUsuario.Text;
            string contraseña = txtPassword.Text;

            // Verifica que los campos de usuario y contraseña no estén vacíos
            if (usuario == "" || contraseña == "")
            {
                MessageBox.Show("Se requiere el ingreso de usuario y contraseña",
                    "ADVERTENCIA");
                return;
            }

            // Crea un objeto de pedido con el comando de ingreso
            Pedido pedido = new Pedido
            {
                Comando = "INGRESO",
                Parametros = new[] { usuario, contraseña }
            };

            // Envía el pedido al servidor y obtiene la respuesta
            Respuesta respuesta = HazOperacion(pedido);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            // Activa o desactiva la interfaz según la respuesta del servidor
            if (respuesta.Estado == "OK" && respuesta.Mensaje == "ACCESO_CONCEDIDO")
            {
                panPlaca.Enabled = true; // Activa la sección de placa si el acceso es correcto
                panLogin.Enabled = false; // Desactiva la sección de login
                MessageBox.Show("Acceso concedido", "INFORMACIÓN");
                txtModelo.Focus(); // Coloca el foco en el campo de modelo
            }
            else if (respuesta.Estado == "NOK" && respuesta.Mensaje == "ACCESO_NEGADO")
            {
                panPlaca.Enabled = false; // Mantiene desactivada la sección de placa
                panLogin.Enabled = true;
                MessageBox.Show("No se pudo ingresar, revise credenciales",
                    "ERROR");
                txtUsuario.Focus(); // Coloca el foco en el campo de usuario para un nuevo intento
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            // Captura los datos ingresados por el usuario sobre el vehículo
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text;

            // Crea un objeto de pedido con el comando CALCULO
            Pedido pedido = new Pedido
            {
                Comando = "CALCULO",
                Parametros = new[] { modelo, marca, placa }
            };

            // Envía el pedido al servidor y recibe la respuesta
            Respuesta respuesta = Protocolo.HazOperacion(pedido, flujo);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            // Verifica si la respuesta es negativa o incorrecta
            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
                // Desmarca todos los días si hay un error
                chkLunes.Checked = false;
                chkMartes.Checked = false;
                chkMiercoles.Checked = false;
                chkJueves.Checked = false;
                chkViernes.Checked = false;
            }
            else
            {
                // Procesa la respuesta y selecciona el día correspondiente
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("Se recibió: " + respuesta.Mensaje,
                    "INFORMACIÓN");
                byte resultado = Byte.Parse(partes[1]);
                switch (resultado)
                {
                    // Habilita el checkbox correspondiente según el día de restricción vehicular
                    case 0b00100000:
                        chkLunes.Checked = true;
                        chkMartes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                    case 0b00010000:
                        chkMartes.Checked = true;
                        // Desactiva otros días
                        chkLunes.Checked = false;
                        chkMiercoles.Checked = false;
                        chkJueves.Checked = false;
                        chkViernes.Checked = false;
                        break;
                        // Casos similares para el resto de los días...
                }
            }
        }

        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            // Envía una solicitud para obtener el número de consultas realizadas por este cliente
            Pedido pedido = new Pedido
            {
                Comando = "CONTADOR",
                Parametros = new[] { "hola" } // Mensaje arbitrario para cumplir con la estructura
            };

            Respuesta respuesta = Protocolo.HazOperacion(pedido, flujo);
            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == "NOK")
            {
                MessageBox.Show("Error en la solicitud.", "ERROR");
            }
            else
            {
                // Muestra el número de consultas realizadas
                var partes = respuesta.Mensaje.Split(' ');
                MessageBox.Show("El número de pedidos recibidos en este cliente es " + partes[0],
                    "INFORMACIÓN");
            }
        }

        private void FrmValidador_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cierra la conexión y el flujo al cerrar el formulario
            if (flujo != null)
                flujo.Close();
            if (remoto != null)
                if (remoto.Connected)
                    remoto.Close();
        }
    }
}
