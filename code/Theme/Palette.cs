using System.Drawing;

namespace Xpera.Theme
{
    
    public class Palette
    {
        private Color _primary;
        private Color _secondary;
        private Color _textPrimary;
        private Color _textSecondary;
        private Color _borderPrimary;
        private Color _borderSecondary;
        private Color _backgroundPrimary;
        private Color _backgroundSecondary;

        public Palette()
        {
            _primary = ColorTranslator.FromHtml("#F43F5E");   
            _secondary = ColorTranslator.FromHtml("#121212"); 

            _textPrimary = Shades.Carbon.AlmostBlack;         
            _textSecondary = Tints.Carbon.DimGray;            

            _borderPrimary = Tints.Carbon.LightGray;          
            _borderSecondary = Tints.Carbon.VeryLightGray;   

            _backgroundPrimary = Tints.Carbon.White;          
            _backgroundSecondary = Tints.SignalRed.MistyRose; 
        }

        public Color Primary
        {
            get { return _primary; }
            set { _primary = value; }
        }

        public Color Secondary
        {
            get { return _secondary; }
            set { _secondary = value; }
        }

        public Color TextPrimary
        {
            get { return _textPrimary; }
            set { _textPrimary = value; }
        }

        public Color TextSecondary
        {
            get { return _textSecondary; }
            set { _textSecondary = value; }
        }

        public Color BorderPrimary
        {
            get { return _borderPrimary; }
            set { _borderPrimary = value; }
        }

        public Color BorderSecondary
        {
            get { return _borderSecondary; }
            set { _borderSecondary = value; }
        }

        public Color BackgroundPrimary
        {
            get { return _backgroundPrimary; }
            set { _backgroundPrimary = value; }
        }

        public Color BackgroundSecondary
        {
            get { return _backgroundSecondary; }
            set { _backgroundSecondary = value; }
        }
    }
}
