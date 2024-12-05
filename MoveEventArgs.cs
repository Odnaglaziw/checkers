using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checkers
{
    public class MoveEventArgs : EventArgs
    {
        public bool MoverIsBlack { get; set; }
        public Point From { get; set; }
        public Point To { get; set; }
        public bool IsCapture { get; set; }
        public Point Capture { get; set; }
    }
}
