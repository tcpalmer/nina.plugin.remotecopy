using System.ComponentModel.Composition;
using System.Windows;

namespace RemoteCopy.NINAPlugin.Instructions {
    [Export(typeof(ResourceDictionary))]
    public partial class RobocopyStartTemplate : ResourceDictionary {
        public RobocopyStartTemplate() {
            InitializeComponent();
        }
    }
}
