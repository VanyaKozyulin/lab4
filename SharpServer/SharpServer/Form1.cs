using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace UdpServer
{

    public partial class ServerForm : Form
    {
        private const int ServerPort = 12345;
        private UdpClient udpListener;

        private Label timerLabel;
        private Timer timer;

        public ServerForm()
        {
            InitializeComponent();
            DoubleBuffered = true;

            timerLabel = new Label();
            Controls.Add(timerLabel);
            timerLabel.Visible = false;

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += OnTimerTick;
        }

        private void server_Load(object sender, EventArgs e)
        {
            udpListener = new UdpClient(ServerPort);
            udpListener.BeginReceive(ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, ServerPort);
                byte[] data = udpListener.EndReceive(ar, ref clientEndPoint);
                string message = Encoding.UTF8.GetString(data);
                Invoke(new Action(() =>
                {
                    string[] parts = message.Split(':');
                    if (parts.Length == 2)
                    {
                        string command = parts[0].Trim();
                        string parameters = parts[1].Trim();

                        switch (command)
                        {
                            case "clear display":
                                ClearDisplay(parameters);
                                break;
                            case "draw line":
                                DrawLine(parameters);
                                break;
                            case "draw pixel":
                                DrawPixelCommand(parameters);
                                break;
                            case "draw rectangle":
                                DrawRectangle(parameters);
                                break;
                            case "fill rectangle":
                                FillRectangle(parameters);
                                break;
                            case "draw ellipse":
                                DrawEllipse(parameters);
                                break;
                            case "fill ellipse":
                                FillEllipse(parameters);
                                break;
                            case "draw text":
                                DrawText(parameters);
                                break;
                            case "draw image":
                                DrawImage(parameters);
                                break;
                            case "timer":
                                Timer(parameters);
                                break;

                            default:
                                break;
                        }
                        lblCommand.Text = $"Команда: {command}";
                        lblParam.Text = $"Параметры: {parameters}";
                    }
                }));
                udpListener.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при приеме данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Color ParseColor(string colorStr)
        {
            if (colorStr.StartsWith("#") && (colorStr.Length == 7 || colorStr.Length == 4))
            {
                try
                {
                    int startIndex = colorStr.StartsWith("#") ? 1 : 0;
                    int r = int.Parse(colorStr.Substring(startIndex, 2), System.Globalization.NumberStyles.HexNumber);
                    int g = int.Parse(colorStr.Substring(startIndex + 2, 2), System.Globalization.NumberStyles.HexNumber);
                    int b = int.Parse(colorStr.Substring(startIndex + 4, 2), System.Globalization.NumberStyles.HexNumber);

                    return Color.FromArgb(r, g, b);
                }
                catch (Exception)
                {
                    return Color.White;
                }
            }

            if (Enum.TryParse(colorStr, out KnownColor knownColor))
            {
                return Color.FromKnownColor(knownColor);
            }

            return Color.White;
        }

        private void ClearDisplay(string parameters)
        {
            string colorName = parameters.Trim();
            Color color = ParseColor(colorName);
            ChangeBackgroundColor(color);

            lblCommand.Text = $"Команда: clear display";
            lblParam.Text = $"Параметры: color={colorName}";
        }

        private void DrawLine(string parameters)
        {
            string[] paramParts = parameters.Split(' ');
            if (paramParts.Length == 5)
            {
                int x0, y0, x1, y1;
                if (int.TryParse(paramParts[0].Trim(), out x0) &&
                    int.TryParse(paramParts[1].Trim(), out y0) &&
                    int.TryParse(paramParts[2].Trim(), out x1) &&
                    int.TryParse(paramParts[3].Trim(), out y1))
                {
                    string colorName = paramParts[4].Trim();
                    Color color = ParseColor(colorName);

                    DrawLineOnForm(x0, y0, x1, y1, color);

                    lblCommand.Text = $"Команда: draw line";
                    lblParam.Text = $"Параметры: x0={x0}, y0={y0}, x1={x1}, y1={y1}, color={colorName}";
                }
                else
                {
                    MessageBox.Show("Ошибка разбора параметров команды 'draw line'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверное количество параметров для команды 'draw line'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawLineOnForm(int x0, int y0, int x1, int y1, Color color)
        {
            using (Graphics g = CreateGraphics())
            {
                using (Pen pen = new Pen(color))
                {
                    g.DrawLine(pen, x0, y0, x1, y1);
                }
            }
        }

        private void DrawPixel(int x, int y, Color color)
        {
            using (Graphics g = CreateGraphics())
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, x, y, 1, 1);
                }
            }
        }

        private void DrawPixelCommand(string command)
        {
            string[] parts = command.Split(',');
            if (parts.Length == 3)
            {
                if (int.TryParse(parts[0], out int x) &&
                    int.TryParse(parts[1], out int y))
                {
                    string colorName = parts[2].Trim();
                    Color color = ParseColor(colorName);
                    DrawPixel(x, y, color);
                }
                else
                {
                    MessageBox.Show("Ошибка разбора параметров команды 'draw pixel'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверное количество параметров для команды 'draw pixel'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawRectangle(string parameters)
        {
            string[] paramParts = parameters.Split(' ');
            if (paramParts.Length == 5)
            {
                int x0, y0, w, h;
                if (int.TryParse(paramParts[0].Trim(), out x0) &&
                    int.TryParse(paramParts[1].Trim(), out y0) &&
                    int.TryParse(paramParts[2].Trim(), out w) &&
                    int.TryParse(paramParts[3].Trim(), out h))
                {
                    string colorName = paramParts[4].Trim();
                    Color color = ParseColor(colorName);

                    DrawRectangleOnForm(x0, y0, w, h, color);

                    lblCommand.Text = $"Команда: draw rectangle";
                    lblParam.Text = $"Параметры: x0={x0}, y0={y0}, w={w}, h={h}, color={colorName}";
                }
                else
                {
                    MessageBox.Show("Ошибка разбора параметров команды 'draw rectangle'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверное количество параметров для команды 'draw rectangle'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawRectangleOnForm(int x0, int y0, int wRect, int hRect, Color color)
        {
            using (Graphics g = CreateGraphics())
            {
                using (Pen pen = new Pen(color))
                {
                    g.DrawRectangle(pen, x0, y0, wRect, hRect);
                }
            }
        }



        private void FillRectangle(string parameters)
        {
            string[] paramParts = parameters.Split(' ');
            if (paramParts.Length == 5)
            {
                int x0, y0, w, h;
                if (int.TryParse(paramParts[0].Trim(), out x0) &&
                    int.TryParse(paramParts[1].Trim(), out y0) &&
                    int.TryParse(paramParts[2].Trim(), out w) &&
                    int.TryParse(paramParts[3].Trim(), out h))
                {
                    string colorName = paramParts[4].Trim();
                    Color color = ParseColor(colorName);

                    FillRectangleOnForm(x0, y0, w, h, color);

                    lblCommand.Text = $"Команда: fill rectangle";
                    lblParam.Text = $"Параметры: x0={x0}, y0={y0}, w={w}, h={h}, color={colorName}";
                }
                else
                {
                    MessageBox.Show("Ошибка разбора параметров команды 'fill rectangle'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверное количество параметров для команды 'fill rectangle'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FillRectangleOnForm(int x0, int y0, int w, int h, Color color)
        {
            using (Graphics g = CreateGraphics())
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, x0, y0, w, h);
                }
            }
        }

        private void DrawEllipse(string parameters)
        {
            string[] paramParts = parameters.Split(' ');
            if (paramParts.Length == 5)
            {
                int x, y, radiusX, radiusY;
                if (int.TryParse(paramParts[0].Trim(), out x) &&
                    int.TryParse(paramParts[1].Trim(), out y) &&
                    int.TryParse(paramParts[2].Trim(), out radiusX) &&
                    int.TryParse(paramParts[3].Trim(), out radiusY))
                {
                    string colorName = paramParts[4].Trim();
                    Color color = ParseColor(colorName);

                    DrawEllipseOnForm(x, y, radiusX, radiusY, color);

                    lblCommand.Text = $"Команда: draw ellipse";
                    lblParam.Text = $"Параметры: x={x}, y={y}, radiusX={radiusX}, radiusY={radiusY}, color={colorName}";
                }
                else
                {
                    MessageBox.Show("Ошибка разбора параметров команды 'draw ellipse'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверное количество параметров для команды 'draw ellipse'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawEllipseOnForm(int x, int y, int radiusX, int radiusY, Color color)
        {
            using (Graphics g = CreateGraphics())
            {
                using (Pen pen = new Pen(color))
                {
                    g.DrawEllipse(pen, x - radiusX, y - radiusY, 2 * radiusX, 2 * radiusY);
                }
            }
        }

        private void FillEllipse(string parameters)
        {
            string[] paramParts = parameters.Split(' ');
            if (paramParts.Length == 5)
            {
                int x, y, radiusX, radiusY;
                if (int.TryParse(paramParts[0].Trim(), out x) &&
                    int.TryParse(paramParts[1].Trim(), out y) &&
                    int.TryParse(paramParts[2].Trim(), out radiusX) &&
                    int.TryParse(paramParts[3].Trim(), out radiusY))
                {
                    string colorName = paramParts[4].Trim();
                    Color color = ParseColor(colorName);

                    FillEllipseOnForm(x, y, radiusX, radiusY, color);
                    lblCommand.Text = $"Команда: fill ellipse";
                    lblParam.Text = $"Параметры: x={x}, y={y}, radiusX={radiusX}, radiusY={radiusY}, color={colorName}";
                }
                else
                {
                    MessageBox.Show("Ошибка разбора параметров команды 'fill ellipse'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверное количество параметров для команды 'fill ellipse'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FillEllipseOnForm(int x, int y, int radiusX, int radiusY, Color color)
        {
            using (Graphics g = CreateGraphics())
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, x - radiusX, y - radiusY, 2 * radiusX, 2 * radiusY);
                }
            }
        }

        private void DrawText(string parameters)
        {
            string[] paramParts = parameters.Split(' ');
            if (paramParts.Length == 6)
            {
                int x, y0, fontSize;
                string colorName, fontName, text;

                if (int.TryParse(paramParts[0].Trim(), out x) &&
                    int.TryParse(paramParts[1].Trim(), out y0) &&
                    int.TryParse(paramParts[4].Trim(), out fontSize))
                {
                    colorName = paramParts[2].Trim();
                    fontName = paramParts[3].Trim();
                    text = paramParts[5].Trim();

                    Color color = ParseColor(colorName);
                    Font font = new Font(fontName, fontSize);
                    DrawTextOnForm(x, y0, color, font, text);
                    lblCommand.Text = $"Команда: draw text";
                    lblParam.Text = $"Параметры: x={x}, y0={y0}, color={colorName}, font={fontName}, fontSize={fontSize}, text={text}";
                }
                else
                {
                    MessageBox.Show("Ошибка разбора параметров команды 'draw text'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DrawTextOnForm(int x0, int y0, Color color, Font font, string text)
        {
            using (Graphics g = CreateGraphics())
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.DrawString(text, font, brush, x0, y0);
                }
            }
        }
        private void DrawImage(string parameters)
        {

            string[] paramParts = parameters.Split(' ');
            if (paramParts.Length == 5)
            {
                int x0, y0, w, h;
                if (int.TryParse(paramParts[0].Trim(), out x0) &&
                    int.TryParse(paramParts[1].Trim(), out y0) &&
                    int.TryParse(paramParts[2].Trim(), out w) &&
                    int.TryParse(paramParts[3].Trim(), out h))
                {
                    string imageData = paramParts[4].Trim();

                    byte[] imageBytes = Convert.FromBase64String(imageData);
                    using (MemoryStream stream = new MemoryStream(imageBytes))
                    {
                        using (Image image = Image.FromStream(stream))
                        {
                            UpdateImageOnForm(x0, y0, w, h, image);
                        }
                    }
                    lblCommand.Text = $"Команда: draw image";
                    lblParam.Text = $"Параметры: x0={x0}, y0={y0}, w={w}, h={h}";
                }
                else
                {
                    MessageBox.Show("Ошибка разбора параметров команды 'draw image'", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateImageOnForm(int x0, int y0, int w, int h, Image image)
        {
            if (image != null && w > 0 && h > 0)
            {
                Rectangle imageRect = new Rectangle(x0, y0, w, h);
                using (Graphics g = CreateGraphics())
                {
                    g.DrawImage(image, imageRect);
                }
            }
        }

        private void ChangeBackgroundColor(Color color)
        {
            this.BackColor = color;
        }

        private void Timer(string parameters)
        {
            string[] paramParts = parameters.Split(' ');

            if (paramParts.Length == 4)
            {
                if (int.TryParse(paramParts[0].Trim(), out int x) &&
                    int.TryParse(paramParts[1].Trim(), out int y) &&
                    int.TryParse(paramParts[2].Trim(), out int fontSize))
                {
                    string colorStr = paramParts[3].Trim();
                    Color textColor = ParseColor(colorStr);

                    timerLabel.Location = new Point(x, y);
                    timerLabel.Font = new Font("Arial", fontSize);
                    timerLabel.ForeColor = textColor;
                    timerLabel.Visible = true;

                    timer.Start();
                }
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            timerLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        }





    }
}
