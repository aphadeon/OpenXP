using System.Drawing;
using System.Windows.Forms;

namespace XPT
{
    public class MPanel : Panel
    {
        //Disable scroll-to-control on focus
        protected override Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }
    }
}
