using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HikariGGNPatcher.Controls
{
    public class ImageButton : Button
    {
        #region Properties & Fields

        // ReSharper disable once InconsistentNaming
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(default(ImageSource)));
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        #endregion

        #region Constructors

        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
        }

        #endregion
    }
}
