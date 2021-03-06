﻿using NETworkManager.Models.Settings;
using System.Windows.Input;
using System.Diagnostics;

namespace NETworkManager.ViewModels.Settings
{
    public class AboutViewModel : ViewModelBase
    {
        #region Variables
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title)
                    return;

                _title = value;
                OnPropertyChanged();
            }
        }

        private string _copyrightAndAuthor;
        public string CopyrightAndAuthor
        {
            get { return _copyrightAndAuthor; }
            set
            {
                if (value == _copyrightAndAuthor)
                    return;

                _copyrightAndAuthor = value;
                OnPropertyChanged();
            }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                if (value == _version)
                    return;

                _version = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public AboutViewModel()
        {           
            CopyrightAndAuthor = string.Format("{0} {1}.", AssemblyManager.Current.Copyright, AssemblyManager.Current.Company);

            Version = AssemblyManager.Current.AssemblyVersion.ToString();
        }
        #endregion

        #region Commands & Actions
        public ICommand OpenWebsiteCommand
        {
            get { return new RelayCommand(p => OpenWebsiteAction(p)); }
        }

        private void OpenWebsiteAction(object url)
        {
            Process.Start((string)url);
        }
        #endregion
    }
}