using Simulation.Traffic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Test.Wpf.ViewModels
{
    public class NodeViewModel
    {
        private readonly Node node;

        public NodeViewModel(Node node)
        {
            this.node = node;
        }
    }
}
