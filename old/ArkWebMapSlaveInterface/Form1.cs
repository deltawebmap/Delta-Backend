using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArkWebMapSlaveServer;

namespace ArkWebMapSlaveInterface
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            ColorConverter cc = new ColorConverter();
            
            InitializeComponent();
            BackColor = (Color)cc.ConvertFromString("#33363c");
            mainView.Url = new Uri("file://"+ System.IO.Path.GetDirectoryName(Application.ExecutablePath).TrimEnd('/') + "/Media/manager.html");
        }

        public void ExecuteJS(string cmd, object[] args)
        {
            mainView.Document.InvokeScript(cmd, args);
        }

        public void ActivateButton(string text)
        {
            ExecuteJS("ActivateButton", new object[] { text });
        }

        public void DeactivateButton(string text)
        {
            ExecuteJS("DeactivateButton", new object[] { text });
        }

        public void SetFormValue(string id, string value)
        {
            ExecuteJS("SetValue", new object[] { id, value });
        }

        public void RestoreValues(ArkSlaveConfig s)
        {
            
        }
    }
}
