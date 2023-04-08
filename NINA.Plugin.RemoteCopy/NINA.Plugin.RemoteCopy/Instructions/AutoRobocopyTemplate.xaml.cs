using System.ComponentModel.Composition;
using System.Windows;

namespace RemoteCopy.NINAPlugin.Instructions {
    [Export(typeof(ResourceDictionary))]
    public partial class AutoRobocopyTemplate : ResourceDictionary {
        public AutoRobocopyTemplate() {
            InitializeComponent();
        }
    }
}
