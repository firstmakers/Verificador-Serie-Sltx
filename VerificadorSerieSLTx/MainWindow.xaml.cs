using HID;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VerificadorSerieSLTx
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dispositivo device;
        public delegate void Desconexion();
        public delegate void Conexion();
        public MainWindow()
        {
            InitializeComponent();
            device = new Dispositivo();
            device.Conexion += device_Conexion;
            device.Desconexion += device_Desconexion;
            populateCbx();
            if (!device.conectado)
                desconexion();
            else
                conexion();

        }
        
        private void populateCbx()
        {
            this.cbx_cmd.Items.Add("89");
            this.cbx_cmd.Items.Add("88");
            this.cbx_cmd.SelectedIndex = 0;
            
        }

        void device_Desconexion()
        {
            this.Dispatcher.BeginInvoke(new Desconexion(desconexion));
        }

        private void desconexion()
        {
            send.IsEnabled = false;
            numeroSerie.Content = "";
            versionFirmware.Content = "";
            produccion.Content = "";
            device_state.Source = new BitmapImage(new Uri(@"pack://application:,,,/VerificadorSerieSLTx;component/images/device_detached.png"));

        }

        void device_Conexion()
        {
            this.Dispatcher.BeginInvoke(new Conexion(conexion));
        }

        private void conexion()
        {
            device_state.Source = new BitmapImage(new Uri(@"pack://application:,,,/VerificadorSerieSLTx;component/images/device_ok.png"));
            send.IsEnabled = true;
            Button_Click_1(this, null);
        }

        private void cmdContenidoChanged(object sender, TextChangedEventArgs e)
        {
            string contenido = "";
            if(cmd_contenido.Text != null && to_hex.IsChecked == true){
                contenido = cmd_contenido.Text;
                cmd_hex.Content = stringToHex(contenido);
            }
               
        }

        private string stringToHex(string s) {

            int length = s.Length;
            char[] charValues = s.ToCharArray();
            string hexOutput = "";

            for (int i = 0; i < length; i++ )
            {
                hexOutput += Convert.ToInt32(charValues[i]).ToString("X");
            }
            return hexOutput;
        }
        private string[] stringToHexArray(string s)
        {

            int length = s.Length;
            char[] charValues = s.ToCharArray();
            string[] hexOutput = new string[length];

            for (int i = 0; i < length; i++)
            {
                int value = Convert.ToInt32(charValues[i]);
                hexOutput[i] = String.Format("{0:X}", value);
            }
            return hexOutput;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void toHexUncheck(object sender, RoutedEventArgs e)
        {
            if (cmd_hex.Content != null)
                cmd_hex.Content = "";
        }

        private void toHexChecked(object sender, RoutedEventArgs e)
        {
            if (cmd_contenido.Text != null && cmd_hex != null)
                cmd_hex.Content = stringToHex(cmd_contenido.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //enviar el comando a la tarjeta
            byte[] command =  new byte[63];
            byte[] content = new byte[63];
            
           
                //string hex = cmd.Text.Replace("0x","");
                uint _cmd = uint.Parse(cbx_cmd.SelectedItem.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
                command[0] = (byte) _cmd;
                            
                if (cmd_contenido.Text.Length == 9) {
                    string[] str = stringToHexArray(cmd_contenido.Text);
            
                    for(int i=0;i<str.Length; i++){
                        uint tmp = uint.Parse(str[i], System.Globalization.NumberStyles.AllowHexSpecifier);
                        command[i+1] = (byte) tmp;
                    }

                    string text="W ";
                    foreach (byte b in command)
                    {
                        text += b.ToString("X");
                        text += " ";
                    }
                    setText(text);                  
                }
                else if (cmd_contenido.Text.Length == 0 && command[0] == 0x89) {
                    setText("El comando 89 no se puede ejecutar, falta el número de serie o es incorrecto");
                    return;
                }
                device.WriteCommand(command);               
                Button_Click_1(this, null);            
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private void setText(string text) {
            console.Text += text;
            console.Text += "\n";
            console.ScrollToEnd();
        }
        private void clearConsole() {
            console.Text = "";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            device.WriteCommand(new byte[]{0x88});
            setText("W 88");
            versionFirmware.Content = device.getFirmwareVersion();
            string serie = device.getFirmwareVersion();
            numeroSerie.Content = device.getSerialNumber();
            produccion.Content = device.getProduction();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            clearConsole();
        }

    }
}
