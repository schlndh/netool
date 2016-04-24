using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Netool.Plugins.Helpers
{
    public static class InstanceStatusStripFactories
    {
        public static StatusStrip DefaultStatusStripFactory(params string[] fields)
        {
            return DefaultStatusStripFactory((IEnumerable<string>)fields);
        }

        public static StatusStrip DefaultStatusStripFactory(IEnumerable<string> fields)
        {
            var statusStrip1 = new StatusStrip();
            if (fields != null)
            {
                int i = 0;
                foreach (var field in fields)
                {
                    var toolStripStatusLabel1 = new ToolStripStatusLabel();
                    toolStripStatusLabel1.Name = "ToolStripLabel" + i.ToString();
                    toolStripStatusLabel1.Text = field;
                    if (i++ > 0)
                    {
                        toolStripStatusLabel1.BorderSides = ToolStripStatusLabelBorderSides.Left;
                        toolStripStatusLabel1.BorderStyle = Border3DStyle.Etched;
                    }
                    statusStrip1.Items.Add(toolStripStatusLabel1);
                }
            }

            return statusStrip1;
        }

        public static StatusStrip Factory(Type instanceType, object settings, bool isLogTemp, string filename)
        {
            return DefaultStatusStripFactory(instanceType.Name, GetLogField(isLogTemp, filename), settings.ToString());
        }

        public static string GetLogField(bool isLogTemp, string filename)
        {
            if (!isLogTemp)
            {
                return filename;
            }
            return "[TEMP] " + filename;
        }
    }
}