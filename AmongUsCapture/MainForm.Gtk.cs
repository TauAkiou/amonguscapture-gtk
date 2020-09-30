using Gtk;

namespace AmongUsCapture
{
    public partial class MainForm
    {
        
        /* We're going to follow the naming conventions from the original
           program, just to keep things relatively consistent between
           this version and the original version.
           
           This means also 'following' the way Windows Designer does things,
           even though we're really rewriting this all by hand. */
        
        private Gtk.Label label2;
        public Gtk.Label playercountlabel;
        
        private void InitializeWindow()
        {
            label2 = new Label();
            playercountlabel = new Label();
            
            //
            // label2
            //
            
            label2.Name = "label2";
            label2.SetSizeRequest(138, 20);
            label2.Text = "Amount of players: ";
            
            //
            // playercountlabel
            //
            
            //
            // MainForm
            //
            
            this.Add(label2);
            this.Add(playercountlabel);
        }
        
        
    }
}