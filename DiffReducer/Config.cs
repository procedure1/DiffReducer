using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffReducer.UI;
namespace DiffReducer
{
    public class Config
    {   
        internal static BS_Utils.Utilities.Config config = new BS_Utils.Utilities.Config("DiffReducer");

        public static void Read()
        {
            //  ModifierUI.instance.enabled = config.GetBool("DiffReducer", "Enabled", false, true);
            ModifierUI.instance.modEnabled = config.GetBool("DiffReducer", "Enabled", false, true);//bw so will remember setting when starts
            ModifierUI.instance.beatDivision = config.GetFloat("DiffReducer", "Section Detection", 1.5f, true);
            ModifierUI.instance.beatDivision2 = config.GetFloat("DiffReducer", "Simplification Detection", 1.5f, true);
            ModifierUI.instance.simplifySwingLength = config.GetBool("DiffReducer", "Simplify Swing Lengths", false, true);
            ModifierUI.instance.DisableBelowThisNPS = config.GetFloat("DiffReducer", "Disable Below This NPS", 5f, true);//bw

        }

        public static void Write()
        {
            Plugin.log.Debug("Writing config");
            config.SetBool("DiffReducer", "Enabled", ModifierUI.instance.modEnabled);//bw
            config.SetFloat("DiffReducer", "Section Detection", ModifierUI.instance.beatDivision);
            config.SetFloat("DiffReducer", "Simplification Detection", ModifierUI.instance.beatDivision2);
            config.SetBool("DiffReducer", "Simplify Swing Lengths", ModifierUI.instance.simplifySwingLength);
            config.SetFloat("DiffReducer", "Disable Below This NPS", ModifierUI.instance.DisableBelowThisNPS);//bw
        }
    }
}
