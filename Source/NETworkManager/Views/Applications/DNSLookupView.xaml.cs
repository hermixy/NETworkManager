﻿using System.Windows.Controls;
using NETworkManager.ViewModels.Applications;

namespace NETworkManager.Views.Applications
{
    public partial class DNSLookupView : UserControl
    {
        DNSLookupViewModel viewModel = new DNSLookupViewModel();

        public DNSLookupView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
