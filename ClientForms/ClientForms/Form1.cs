using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;


namespace ClientForms { 
public partial class Form1 : Form
{
    private string serverIp = "127.0.0.1";
    private int serverPort = 12345;
    private UdpClient udpClient = new UdpClient();

    public Form1()
    {
        InitializeComponent();
    }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string commandNumber = txtCommand.Text.Trim(); 
            string parameters = txtParameters.Text.Trim(); 

            string command = GetCommandFromNumber(commandNumber);

            if (command == null)
            {
           
                MessageBox.Show("Неправильный номер команды.");
                return;
            }

            string message = $"{command}: {parameters}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, serverIp, serverPort);

            CommandInput.Items.Add("Команда: " + command);
            CommandInput.Items.Add("Параметри: " + parameters);

            txtCommand.Clear();
            txtParameters.Clear();
        }

        private string GetCommandFromNumber(string commandNumber)
        {
            switch (commandNumber)
            {
                case "1":
                    return "clear display";
                case "2":
                    return "draw pixel";
                case "3":
                    return "draw line";
                case "4":
                    return "draw rectangle";
                case "5":
                    return "fill rectangle";
                case "6":
                    return "draw ellipse";
                case "7":
                    return "fill ellipse";
                case "8":
                    return "draw text";
                case "9":
                    return "draw image";
                case "10":
                    return "timer";
                default:
                    return null;

            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            lstCommands.Items.Add("Выберите команду:");
            lstCommands.Items.Add("1. Clear Display");
            lstCommands.Items.Add("(введіть color)");
            lstCommands.Items.Add("2. Draw Pixel");
            lstCommands.Items.Add("(введіть x0, y0, color)");
            lstCommands.Items.Add("3. Draw Line");
            lstCommands.Items.Add("(введіть x0, y0, x1, y1, color)");
            lstCommands.Items.Add("4. Draw Rectangle");
            lstCommands.Items.Add("(введіть x0, y0, w, h, color)");
            lstCommands.Items.Add("5. Fill Rectangle");
            lstCommands.Items.Add("(введіть x0, y0, w, h, color)");
            lstCommands.Items.Add("6. Draw Ellipse");
            lstCommands.Items.Add("(введіть x0, y0, r_x, r_y, color)");
            lstCommands.Items.Add("7. Fill Ellipse");
            lstCommands.Items.Add("(введіть x0, y0, r_x, r_y, color)");
            lstCommands.Items.Add("8. Draw Text");
            lstCommands.Items.Add("(введіть x0, y0, color, font, length, text)");
            lstCommands.Items.Add("9. Draw Image");
            lstCommands.Items.Add("(введіть x0, y0, w, h, data)");
            lstCommands.Items.Add("10. Timer");
            lstCommands.Items.Add("(введіть x0, y0, length, color)");
        }

    }
}