using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Chess
{
    public static class Helpers
    {
        public static Rectangle CreatePiece(int value)
        {
            char pieceChar = Pieces.GetPieceChar(value);
            string resourceName = Pieces.ResourceNames[pieceChar];

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                ImageBrush brush = new ImageBrush(bitmap);
                Rectangle piece = new Rectangle
                {
                    Fill = brush
                };
                piece.Width = Constants.Square_Size;
                piece.Height = Constants.Square_Size;
                return piece;
            }
        }
    }
}
