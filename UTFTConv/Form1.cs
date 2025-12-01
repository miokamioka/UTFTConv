using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UTFTConv
{
    public partial class Form1 : Form
    {
        private Bitmap _currentBitmap;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "image file(*.bmp;*.jpg;*.png;*.jpeg)|*.bmp;*.jpg;*.png;*.jpeg";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        _currentBitmap = new Bitmap(fs);
                    }
                    pictureBox1.Image = _currentBitmap;
                    lblStatus.Text = $"Loaded: {_currentBitmap.Width}x{_currentBitmap.Height}";
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_currentBitmap == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "C Source File|*.c";
                sfd.FileName = "image_data.c";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string arrayName = Path.GetFileNameWithoutExtension(sfd.FileName);
                    string cContent = GenerateCFileContent(_currentBitmap, arrayName);

                    File.WriteAllText(sfd.FileName, cContent);
                    lblStatus.Text = $"Saved: {sfd.FileName}";
                    MessageBox.Show("done.");
                }
            }
        }
        private string GenerateCFileContent(Bitmap bmp, string arrayName)
        {
            StringBuilder sb = new StringBuilder();

            // hedder
            sb.AppendLine($"// Image Size: {bmp.Width}x{bmp.Height}");
            sb.AppendLine("#include <avr/pgmspace.h>");
            sb.AppendLine();
            sb.AppendLine($"const unsigned short {arrayName}[{bmp.Width * bmp.Height}] PROGMEM = {{");

            int count = 0;

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    // RGB565 Convert
                    // R: 8bit -> 5bit, G: 8bit -> 6bit, B: 8bit -> 5bit
                    ushort rgb565 = ConvertToRGB565(pixel);

                    sb.Append($"0x{rgb565:X4}"); // x16 (ex: 0xFFFF)

                    if (!((x == bmp.Width - 1) && (y == bmp.Height - 1)))
                    {
                        sb.Append(", ");
                    }

                    count++;
                    if (count % 16 == 0) sb.AppendLine();
                }
            }

            sb.AppendLine("};");
            return sb.ToString();
        }
        private ushort ConvertToRGB565(Color color)
        {
            int r = (color.R >> 3) & 0x1F;
            int g = (color.G >> 2) & 0x3F;
            int b = (color.B >> 3) & 0x1F;

            int result = (r << 11) | (g << 5) | b;

            return (ushort)result;
        }
    }
}
